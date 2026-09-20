namespace InFalsusCalc;

/// <summary>固定key检查点的兼容性与全部理论目标覆盖判定。</summary>
internal sealed partial class KeyRecipeSolver
{
    /// <summary>只读取同配方、资源与数学规则的检查点。</summary>
    /// <param name="catalog">当前资源快照。</param>
    /// <param name="recipe">目标配方。</param>
    /// <returns>可续算检查点；不兼容时为空。</returns>
    public static RecipeResult? Load(Catalog catalog, Recipe recipe)
    {
        RecipeResult? saved = Storage.Read<RecipeResult>(Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json"));
        return saved?.Policy == RecipePolicy(recipe) && saved.Snapshot == catalog.Data.Id && saved.Recipe == recipe.Id ? saved : null;
    }

    /// <summary>枚举结构奖励可形成的全部key，包含空区域的0槽key。</summary>
    /// <param name="recipe">区域奖励来自该配方。</param>
    /// <returns>槽数、左范围、右范围的全部封顶组合。</returns>
    public static int[][] Keys(Recipe recipe)
    {
        HashSet<(int Slots, int Left, int Right)> keys = [(0, 0, 0)];
        uint excluded = Excluded(recipe);
        foreach (BonusArea area in recipe.Areas.Where((_, i) => (excluded & (1u << i)) == 0))
        {
            int slots = area.Effects.Where(e => e.Kind == 7).Sum(e => e.Arguments[0].Value);
            int left = area.Effects.Where(e => e.Kind == 8).Sum(e => e.Arguments[0].Value);
            int right = area.Effects.Where(e => e.Kind == 8).Sum(e => e.Arguments[1].Value);
            foreach (var k in keys.ToArray()) keys.Add((Math.Min(3, k.Slots + slots), Math.Min(4, k.Left + left), Math.Min(4, k.Right + right)));
        }
        return keys.Select(k => k.Slots == 0 ? (0, 0, 0) : k).Distinct().Order()
            .Select(k => new[] { k.Item1, k.Item2, k.Item3 }).ToArray();
    }

    /// <summary>复用同key其他目标保存的合法布局，等于已证上界时直接闭合。</summary>
    /// <param name="result">同一规则与快照下的检查点。</param>
    /// <returns>本次无需搜索便闭合的目标数量。</returns>
    private static int ReuseBounds(RecipeResult result)
    {
        int closed = 0;
        foreach (var entry in result.Groups)
        {
            GroupState group = entry.Value;
            CardTemplate[] available = group.Best.Values.Distinct().Where(result.Cards.ContainsKey).Select(id => result.Cards[id])
                .Where(c => c.Valid && c.Strikes == 0 && group.Key.SequenceEqual(new[] { c.Slots, c.Slots == 0 ? 0 : c.Left, c.Slots == 0 ? 0 : c.Right }) &&
                    !c.Active.Intersect(result.ExcludedAreas).Any()).ToArray();
            foreach (var pair in group.Goals)
            {
                if (pair.Value.Status != "UNKNOWN") continue;
                int goal = pair.Key == "power" ? 0 : pair.Key == "fortitude" ? 1 : 2;
                CardTemplate? card = available.OrderByDescending(c => Craft.Panel(c, goal)).ThenBy(c => c.Id).FirstOrDefault();
                if (card is null) continue;
                int value = Craft.Panel(card, goal);
                if (value > pair.Value.UpperBound) throw new InvalidDataException($"配方{result.Recipe}/{entry.Key}/{pair.Key}的合法布局超过保存上界。");
                if (value != pair.Value.UpperBound) continue;
                group.Best[pair.Key] = card.Id; pair.Value.Status = "OPTIMAL"; closed++;
                Console.WriteLine($"[{result.Recipe}] key={entry.Key} {pair.Key} 已有合法布局达到上界，复用证明闭合。");
            }
            group.Complete = new[] { "power", "fortitude", "total" }.All(g => Done(result, entry.Key, g));
            group.Infeasible = group.Complete && group.Best.Count == 0;
        }
        return closed;
    }

    /// <summary>在启动排队前落实可直接复用的证明，避免它们等待长任务结束。</summary>
    /// <param name="catalog">当前快照。</param>
    /// <param name="recipe">目标配方。</param>
    public static void RestoreBounds(Catalog catalog, Recipe recipe)
    {
        RecipeResult? saved = Load(catalog, recipe);
        if (saved is null || saved.Complete) return;
        int imported = 0;
        RecipeResult? legacy = Storage.Read<RecipeResult>(Path.Combine(Storage.State, "recipes", $"{recipe.Id:00}.json"));
        if (legacy is not null && legacy.Snapshot == catalog.Data.Id && legacy.Recipe == recipe.Id)
            foreach (CardTemplate stored in legacy.Cards.Values)
            {
                CardTemplate card = Craft.Evaluate(catalog, recipe, stored.Placements);
                if (!card.Valid || card.Strikes != 0 || card.Active.Intersect(saved.ExcludedAreas).Any()) continue;
                string key = $"{card.Slots},{card.Left},{card.Right}";
                if (!saved.Groups.TryGetValue(key, out GroupState? group)) continue;
                foreach (string goal in new[] { "power", "fortitude", "total" })
                {
                    if (Done(saved, key, goal)) continue;
                    int index = goal == "power" ? 0 : goal == "fortitude" ? 1 : 2;
                    CardTemplate? old = group.Best.TryGetValue(goal, out string? id) ? saved.Cards.GetValueOrDefault(id) : null;
                    if (old is not null && Craft.Panel(old, index) >= Craft.Panel(card, index)) continue;
                    saved.Cards[card.Id] = card; group.Best[goal] = card.Id; imported++;
                }
            }
        int closed = ReuseBounds(saved);
        if (imported == 0 && closed == 0) return;
        if (imported > 0) Console.WriteLine($"[{recipe.Id}] 按当前规则复核旧布局，改善{imported}个目标候选；不复用旧最优标签。");
        saved.Complete = ConfidenceAnalysis.RetainedKeys(recipe).All(k => new[] { "power", "fortitude", "total" }.All(g => Done(saved, string.Join(',', k), g)));
        saved.BoundedFinalized = saved.Complete; saved.Updated = DateTimeOffset.UtcNow;
        Storage.Write(Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json"), saved);
    }

    /// <summary>判断当前范围是否真正尝试过；换范围后仅保留上界不算已搜索。</summary>
    /// <param name="saved">兼容当前范围的检查点。</param>
    /// <param name="key">固定结构key。</param>
    /// <param name="goal">目标名称。</param>
    /// <returns>是否已经在当前范围开始过计算。</returns>
    public static bool Attempted(RecipeResult? saved, string key, string goal)
    {
        KeyGoalState? state = saved?.Groups.GetValueOrDefault(key)?.Goals.GetValueOrDefault(goal);
        return state is not null && (state.SearchPolicy ?? Policy) == saved!.Policy;
    }

    /// <summary>目标只有明确无解或拥有布局的最优结论才算完成。</summary>
    /// <param name="saved">兼容的检查点。</param>
    /// <param name="key">精确结构key。</param>
    /// <param name="goal">攻击、防御或总和目标。</param>
    /// <returns>是否可以直接跳过搜索。</returns>
    public static bool Done(RecipeResult? saved, string key, string goal)
    {
        if (saved is null || !saved.Groups.TryGetValue(key, out GroupState? group) || !group.Goals.TryGetValue(goal, out KeyGoalState? state)) return false;
        return state.Status == "INFEASIBLE" || state.Status is "OPTIMAL" or "CONFIDENCE" && group.Best.TryGetValue(goal, out string? id) && saved.Cards.ContainsKey(id);
    }
}
