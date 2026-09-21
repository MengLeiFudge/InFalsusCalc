namespace InFalsusCalc;

/// <summary>用户指定的两类无用区域及检查点范围迁移，不推广到其他低收益区域。</summary>
internal sealed partial class KeyRecipeSolver
{
    /// <summary>当前配方不允许完整激活的区域位集，棋盘格仍保留。</summary>
    private readonly uint excluded;

    /// <summary>按特征识别六色微小奖励块，不限定位置；英雄另按红框范围排除。</summary>
    /// <param name="recipe">当前资源中的配方与原始区域顺序。</param>
    /// <returns>明确排除的区域位集。</returns>
    internal static uint Excluded(Recipe recipe)
    {
        uint result = 0;
        for (int i = 0; i < recipe.Areas.Length; i++)
        {
            BonusArea area = recipe.Areas[i];
            bool tinyIsland = area.Cells.Length == 6 && area.Cells.Select(c => c.Color).Distinct().Count() >= 5 &&
                area.Effects.Length == 2 && area.Effects.Any(e => e.Kind == 3 && e.Arguments[0].Value == 1) &&
                area.Effects.Any(e => e.Kind == 4 && e.Arguments[0].Value == 1);
            // 平顶六边格的显示高度与R+Q/2成正比；12是红框区域的最高Q+2R，正常紫色长条在其上方。
            bool heroBottom = recipe.Id == 72 && area.Cells.Length > 0 && area.Cells.All(c => c.Position.Q + 2 * c.Position.R <= 12);
            if (tinyIsland || heroBottom) result |= 1u << i;
        }
        return result;
    }

    /// <summary>仅受排除规则影响的配方改变缓存签名，其余配方沿用原证明。</summary>
    /// <param name="recipe">目标配方。</param>
    /// <returns>包含排除范围的稳定策略标识。</returns>
    private static string RecipePolicy(Recipe recipe)
    {
        uint mask = Excluded(recipe);
        return mask == 0 ? Policy : $"{Policy}/excluded-v1-{mask:x}";
    }

    /// <summary>迁移遗漏“同格最多三粒子”硬限制的检查点，仅重开直接受影响的目标。</summary>
    /// <param name="catalog">当前资源快照。</param>
    /// <param name="recipe">待迁移配方。</param>
    public static void MigrateStackLimit(Catalog catalog, Recipe recipe)
    {
        RecipeResult? saved = Load(catalog, recipe);
        if (saved is null) return;
        HashSet<string> invalid = saved.Cards.Values.Where(card => card.Placements.SelectMany(p => p.Cells)
            .GroupBy(cell => cell).Any(stack => stack.Count() > Craft.MaxStack)).Select(card => card.Id).ToHashSet();
        if (invalid.Count == 0) return;
        string path = Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json");
        string backup = Path.Combine(Storage.Root, ".codex", "trash", "stack-limit-v1", $"{recipe.Id:00}-{catalog.Data.Id}.json");
        if (!File.Exists(backup)) Storage.Write(backup, saved);
        int reopened = 0;
        foreach (GroupState group in saved.Groups.Values)
        {
            HashSet<string> affected = group.Best.Where(pair => invalid.Contains(pair.Value)).Select(pair => pair.Key).ToHashSet();
            if (group.TotalBest.Any(invalid.Contains)) affected.Add("total");
            group.TotalBest = group.TotalBest.Where(id => !invalid.Contains(id)).ToArray();
            foreach (string goal in affected)
            {
                CardTemplate[] remaining = group.Best.Values.Concat(group.TotalBest).Where(id => !invalid.Contains(id) && saved.Cards.ContainsKey(id))
                    .Distinct().Select(id => saved.Cards[id]).ToArray();
                if (remaining.Length == 0) group.Best.Remove(goal);
                else group.Best[goal] = Craft.BestForGoal(remaining, goal).Id;
                if (group.Goals.TryGetValue(goal, out KeyGoalState? state))
                {
                    state.Status = "UNKNOWN";
                    state.SearchPolicy = "stack-limit-v1";
                    state.ObjectiveComplete = false;
                }
                reopened++;
            }
        }
        saved.Cards = saved.Cards.Where(pair => !invalid.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
        NormalizeBest(saved);
        HashSet<string> used = saved.Groups.Values.SelectMany(group => group.Best.Values.Concat(group.TotalBest)).ToHashSet();
        saved.Cards = saved.Cards.Where(pair => used.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
        saved.Complete = false;
        saved.Updated = DateTimeOffset.UtcNow;
        Storage.Write(path, saved);
        Console.WriteLine($"[{recipe.Id}] 移除{invalid.Count}个超过同格{Craft.MaxStack}粒子的旧代表，重新打开{reopened}个目标。");
    }

    /// <summary>启动并行计算前迁移旧范围；保留仍成立的证明，原检查点先备份。</summary>
    /// <param name="catalog">用于判断旧结果资源兼容性的快照。</param>
    /// <param name="recipe">需要检查排除范围的配方。</param>
    public static void MigrateExclusions(Catalog catalog, Recipe recipe)
    {
        uint mask = Excluded(recipe);
        if (mask == 0) return;
        string path = Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json");
        RecipeResult? saved = Storage.Read<RecipeResult>(path);
        if (saved is null || saved.Policy != Policy || saved.Snapshot != catalog.Data.Id || saved.Recipe != recipe.Id) return;
        string backup = Path.Combine(Storage.Root, ".codex", "trash", "key-exclusions-v1", $"{recipe.Id:00}-{catalog.Data.Id}.json");
        if (!File.Exists(backup)) Storage.Write(backup, saved);
        saved.Policy = RecipePolicy(recipe);
        saved.SelectionScope = "exact_key_three_panels_filtered_regions";
        saved.ExcludedAreas = Enumerable.Range(0, recipe.Areas.Length).Where(i => (mask & (1u << i)) != 0).ToArray();
        saved.Cards = saved.Cards.Where(p => p.Value.Active.All(i => (mask & (1u << i)) == 0)).ToDictionary(p => p.Key, p => p.Value);
        int reopened = 0;
        foreach (GroupState group in saved.Groups.Values)
        {
            foreach (string goal in group.Best.Keys.ToArray())
                if (!saved.Cards.ContainsKey(group.Best[goal])) group.Best.Remove(goal);
            group.TotalBest = group.TotalBest.Where(saved.Cards.ContainsKey).ToArray();
            foreach (var pair in group.Goals)
                if (pair.Value.Status == "OPTIMAL" && !group.Best.ContainsKey(pair.Key))
                { pair.Value.Status = "UNKNOWN"; reopened++; }
        }
        saved.Complete = false;
        saved.Updated = DateTimeOffset.UtcNow;
        Storage.Write(path, saved);
        Console.WriteLine($"[{recipe.Id}] 排除区域{string.Join(',', saved.ExcludedAreas)}；保留兼容证明，重新开放{reopened}个受影响目标。");
    }
}
