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
            foreach (var k in keys.ToArray())
                keys.Add((Math.Min(3, k.Slots + slots), Math.Min(4, k.Left + left), Math.Min(4, k.Right + right)));
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
            CardTemplate[] available = group.Best.Values.Concat(group.TotalBest).Distinct().Where(result.Cards.ContainsKey).Select(id => result.Cards[id])
                .Where(c => c.Valid && c.Strikes == group.Strikes && group.Key.SequenceEqual(new[] { c.Slots, c.Slots == 0 ? 0 : c.Left, c.Slots == 0 ? 0 : c.Right }) &&
                    !c.Active.Intersect(result.ExcludedAreas).Any()).ToArray();
            foreach (var pair in group.Goals)
            {
                if (pair.Value.Status != "UNKNOWN" || pair.Key == "total" || pair.Value.SecondaryUpperBound is null)
                    continue;
                int goal = pair.Key == "power" ? 0 : 1;
                CardTemplate? card = available.Length == 0 ? null : Craft.BestForGoal(available, pair.Key);
                if (card is null)
                    continue;
                int value = Craft.Panel(card, goal);
                int secondary = pair.Key == "power" ? card.Fortitude : card.Power;
                if (value > pair.Value.UpperBound || value == pair.Value.UpperBound && secondary > pair.Value.SecondaryUpperBound.Value)
                    throw new InvalidDataException($"配方{result.Recipe}/{entry.Key}/{pair.Key}的合法布局超过保存上界。");
                if (value != pair.Value.UpperBound || secondary != pair.Value.SecondaryUpperBound.Value)
                    continue;
                group.Best[pair.Key] = card.Id;
                pair.Value.Status = "OPTIMAL";
                pair.Value.ObjectiveComplete = true;
                closed++;
                Console.WriteLine($"[{result.Recipe}] key={entry.Key} {pair.Key} 已有合法布局达到上界，复用证明闭合。");
            }
        }
        return closed;
    }

    /// <summary>在启动排队前落实可直接复用的证明，避免它们等待长任务结束。</summary>
    /// <param name="catalog">当前快照。</param>
    /// <param name="recipe">目标配方。</param>
    public static void RestoreBounds(Catalog catalog, Recipe recipe)
    {
        RecipeResult? saved = Load(catalog, recipe);
        if (saved is null || saved.Complete)
            return;
        int imported = 0;
        RecipeResult? legacy = Storage.Read<RecipeResult>(Path.Combine(Storage.State, "recipes", $"{recipe.Id:00}.json"));
        if (legacy is not null && legacy.Snapshot == catalog.Data.Id && legacy.Recipe == recipe.Id)
            foreach (CardTemplate stored in legacy.Cards.Values)
            {
                CardTemplate card = Craft.Evaluate(catalog, recipe, stored.Placements);
                if (!card.Valid || card.Strikes != 0 || card.Active.Intersect(saved.ExcludedAreas).Any())
                    continue;
                string key = GroupKey($"{card.Slots},{card.Left},{card.Right}", 0);
                if (!saved.Groups.TryGetValue(key, out GroupState? group))
                    continue;
                foreach (string goal in new[] { "power", "fortitude", "total" })
                {
                    if (Done(saved, key, goal))
                        continue;
                    CardTemplate? old = group.Best.TryGetValue(goal, out string? id) ? saved.Cards.GetValueOrDefault(id) : null;
                    if (old is not null)
                    {
                        if (goal == "total" && old.Total > card.Total)
                            continue;
                        if (goal != "total" && Craft.BestForGoal([old, card], goal).Id == old.Id)
                            continue;
                    }
                    RecordCandidate(saved, group, goal, card);
                    imported++;
                }
            }
        int closed = ReuseBounds(saved);
        if (imported == 0 && closed == 0)
            return;
        if (imported > 0)
            Console.WriteLine($"[{recipe.Id}] 按当前规则复核旧布局，改善{imported}个目标候选；不复用旧最优标签。");
        saved.Complete = Complete(saved, recipe);
        saved.Updated = DateTimeOffset.UtcNow;
        Storage.Write(Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json"), saved);
    }

    /// <summary>生成惩罚层的稳定分组编号；惩罚0沿用旧编号以复用既有检查点。</summary>
    internal static string GroupKey(string key, int strikes) => strikes == 0 ? key : $"{key}/p{strikes}";

    /// <summary>所有保留结构和惩罚层的三个目标均有结论时才完成。</summary>
    private static bool Complete(RecipeResult result, Recipe recipe) => ConfidenceAnalysis.RetainedKeys(recipe).All(k =>
        ConfidenceAnalysis.RetainedStrikes.All(strikes => new[] { "power", "fortitude", "total" }
            .All(goal => Done(result, GroupKey(string.Join(',', k), strikes), goal))));

    /// <summary>判断当前目标合同是否真正尝试过；旧主面板最优标签按未尝试的新合同处理。</summary>
    /// <param name="saved">兼容当前范围的检查点。</param>
    /// <param name="key">固定结构key。</param>
    /// <param name="goal">目标名称。</param>
    /// <returns>是否已经在当前范围开始过计算。</returns>
    public static bool Attempted(RecipeResult? saved, string key, string goal)
    {
        KeyGoalState? state = saved?.Groups.GetValueOrDefault(key)?.Goals.GetValueOrDefault(goal);
        return state is not null && !(state.Status == "OPTIMAL" && !state.ObjectiveComplete)
            && (state.SearchPolicy ?? Policy) == saved!.Policy;
    }

    /// <summary>目标只有明确无解或拥有布局的最优结论才算完成。</summary>
    /// <param name="saved">兼容的检查点。</param>
    /// <param name="key">精确结构key。</param>
    /// <param name="goal">攻击、防御或总和目标。</param>
    /// <returns>是否可以直接跳过搜索。</returns>
    public static bool Done(RecipeResult? saved, string key, string goal)
    {
        if (saved is null || !saved.Groups.TryGetValue(key, out GroupState? group) || !group.Goals.TryGetValue(goal, out KeyGoalState? state))
            return false;
        if (state.Status == "INFEASIBLE")
            return true;
        if (state.Status == "CONFIDENCE")
            return group.Best.TryGetValue(goal, out string? confidenceId) && saved.Cards.ContainsKey(confidenceId);
        return state.Status == "OPTIMAL" && state.ObjectiveComplete && group.Best.TryGetValue(goal, out string? id) && saved.Cards.ContainsKey(id)
            && (goal != "total" || group.TotalBest.Length > 0 && group.TotalBest.All(saved.Cards.ContainsKey));
    }
}
