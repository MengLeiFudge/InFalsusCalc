namespace InFalsusCalc;

/// <summary>确定的卡牌结构；零槽统一左右范围为零。</summary>
/// <param name="Slots">普通特性槽数。</param>
/// <param name="Left">左范围。</param>
/// <param name="Right">右范围。</param>
/// <param name="Palette">同一布局可选颜色的位集合。</param>
internal readonly record struct Structure(int Slots, int Left, int Right, int Palette)
{
    /// <summary>持久化的稳定类别编号。</summary>
    public string Key => $"{Slots},{Left},{Right},{Palette}";
    /// <summary>网页使用的四元组。</summary>
    public int[] Values => [Slots, Left, Right, Palette];
}

/// <summary>布局的真实规则、区域上界与材料技能合法性。</summary>
internal static class Craft
{
    /// <summary>当前版本可解锁的数量、越界、重叠、断连容忍，按角色编号排列。</summary>
    public static readonly (int Count, int Outside, int Overlap, int Split)[] Skills =
        [(15, 1, 1, 3), (10, 4, 2, 0), (17, 2, 3, 3), (15, 3, 1, 4), (20, 1, 4, 1)];

    /// <summary>保留原生单精度倍率边界，再分别对最终攻防向上取整。</summary>
    /// <param name="value">激活区域的基础数值和。</param>
    /// <param name="strikes">净惩罚次数。</param>
    /// <returns>999效能下的卡牌单项数值。</returns>
    public static int FinalStat(int value, int strikes) => strikes < 3
        ? checked((int)Math.Ceiling(value * (double)(float)(999d / 100) * (float)((9d - strikes * strikes) / 9))) : 0;

    /// <summary>求六边格共享边的连通分量。</summary>
    /// <param name="cells">需要分组的占用格。</param>
    /// <returns>互不连接的格子集合。</returns>
    public static List<HashSet<Hex>> Components(IEnumerable<Hex> cells)
    {
        HashSet<Hex> remaining = new(cells);
        List<HashSet<Hex>> result = [];
        while (remaining.Count > 0)
        {
            Hex first = remaining.First(); remaining.Remove(first);
            HashSet<Hex> group = [first]; Stack<Hex> stack = new(); stack.Push(first);
            while (stack.TryPop(out Hex cell))
                foreach (Hex direction in Hex.Directions)
                {
                    Hex next = cell.Add(direction);
                    if (remaining.Remove(next)) { group.Add(next); stack.Push(next); }
                }
            result.Add(group);
        }
        return result;
    }

    /// <summary>从形状和坐标重新核对奖励、实际惩罚、材料能力与卡牌面板。</summary>
    /// <param name="catalog">当前兼容版本资源。</param>
    /// <param name="recipe">配方几何。</param>
    /// <param name="input">候选形状编号与平移坐标。</param>
    /// <returns>包含真实格子和材料数量的结果。</returns>
    public static CardTemplate Evaluate(Catalog catalog, Recipe recipe, IEnumerable<Placement> input)
    {
        HashSet<Hex> board = recipe.Board.Select(c => c.Position).ToHashSet();
        HashSet<Hex> safe = recipe.Safe.Select(c => c.Position).ToHashSet();
        Dictionary<Hex, int> occupied = []; HashSet<(Hex, int)> matching = [];
        List<Placement> selected = []; int outside = 0;
        int[] tiers = [0, 0, 0], profiles = new int[catalog.Data.Profiles.Length];
        foreach (Placement raw in input.OrderBy(p => p.Id).ThenBy(p => p.Q).ThenBy(p => p.R))
        {
            Shape shape = catalog.Shapes[raw.Id]; Hex at = new(raw.Q, raw.R);
            Hex[] cells = shape.Cells.Select(c => c.Add(at)).OrderBy(c => c.Q).ThenBy(c => c.R).ToArray();
            if (cells.Any(c => !board.Contains(c))) throw new InvalidDataException($"配方{recipe.Id}有粒子超出合法外框。");
            foreach (Hex cell in cells) { occupied[cell] = occupied.GetValueOrDefault(cell) + 1; matching.Add((cell, shape.Color)); }
            if (cells.Any(c => !safe.Contains(c))) outside++;
            tiers[shape.Tier - 1]++; profiles[shape.Profile]++;
            selected.Add(new Placement { Id = shape.Id, Q = raw.Q, R = raw.R, Color = shape.Color, Cells = cells });
        }
        int[] effects = new int[16]; List<int> active = []; HashSet<int> colors = [recipe.BaseColor];
        int left = 0, right = 0;
        for (int i = 0; i < recipe.Areas.Length; i++)
        {
            BonusArea area = recipe.Areas[i];
            if (!area.Cells.All(c => matching.Contains((c.Position, c.Color)))) continue;
            active.Add(i);
            foreach (RegionEffect effect in area.Effects)
            {
                if (effect.Kind == 1) colors.Add(effect.Arguments[0].Value);
                else if (effect.Kind == 8) { left += effect.Arguments[0].Value; right += effect.Arguments[1].Value; }
                else if (effect.Kind is 3 or 4 or 7 or 9 or 10 or 11 or 12 or 15) effects[effect.Kind] += effect.Arguments[0].Value;
                else throw new InvalidDataException($"未支持的区域效果{effect.Kind}。");
            }
        }
        var limits = Skills[recipe.Character]; int limit = Math.Min(limits.Count, recipe.MaxIota);
        List<HashSet<Hex>> components = Components(occupied.Keys);
        int[] rawPenalties = [Math.Max(0, selected.Count + effects[15] - limit), outside,
            occupied.Values.Count(n => n > 1), Math.Max(0, components.Count - 1)];
        int[] penalties = [rawPenalties[0], Math.Max(0, outside - limits.Outside - effects[9]),
            Math.Max(0, rawPenalties[2] - limits.Overlap - effects[10]), Math.Max(0, rawPenalties[3] - limits.Split - effects[11])];
        bool falsehood = components.Any(c => !c.Overlaps(safe)); int strikes = penalties.Sum();
        if (falsehood) strikes = Math.Max(3, strikes);
        int power = FinalStat(effects[3], strikes), fortitude = FinalStat(effects[4], strikes);
        int slots = Math.Min(3, effects[7]); left = Math.Min(4, left); right = Math.Min(4, right);
        Structure structure = new(slots, slots > 0 ? left : 0, slots > 0 ? right : 0, colors.Sum(c => 1 << c));
        double innerStructure = slots * (Math.Min(left + right + 1, 5) + 0.5 * Math.Max(left + right - 4, 0));
        double outerStructure = slots * Math.Max(4 - left - right, 0);
        double heuristicScore = Math.Max(innerStructure * 0.53, outerStructure * 0.28) + (power + fortitude) / 10000d;
        int[] available = slots == 0 ? [] : catalog.Data.Profiles.Where(p => profiles[p.Id] > 0)
            .SelectMany(p => p.Options).SelectMany(p => p.Traits).Distinct().Order().ToArray();
        CardTemplate card = new()
        {
            Recipe = recipe.Id, BaseId = recipe.BaseId, Name = recipe.Name, Power = power, Fortitude = fortitude,
            BasePower = effects[3], BaseFortitude = effects[4], Slots = slots, Left = left, Right = right,
            InnerStructure = innerStructure, OuterStructure = outerStructure, HeuristicScore = heuristicScore,
            Colors = colors.Order().ToArray(), TierCounts = tiers, AvailableTraits = available,
            RetainedPercent = Math.Max(0, (9d - strikes * strikes) / 9) * 100,
            Carriers = profiles.Select(n => Math.Min(3, n)).ToArray(), Placements = selected.ToArray(),
            Active = active.ToArray(), Strikes = strikes, Penalties = penalties, RawPenalties = rawPenalties,
            Falsehood = falsehood, Valid = power > 0 && fortitude > 0 && !falsehood,
            Group = structure.Values, FullCoverage = active.Count == recipe.Areas.Length
        };
        card.Id = Storage.Digest(new object[] { catalog.Data.Id, recipe.Id, selected.Select(p => new[] { p.Id, p.Q, p.R }).ToArray() });
        return card;
    }

    /// <summary>枚举奖励子集的结构上界，不把这些理论类别误认为均可制作。</summary>
    /// <param name="recipe">当前配方。</param>
    /// <returns>每类忽略几何时的基础攻击、防御、总值上界。</returns>
    public static Dictionary<Structure, int[]> Groups(Recipe recipe)
    {
        Dictionary<Structure, int[]> states = new() { [new(0, 0, 0, 1 << recipe.BaseColor)] = [0, 0, 0] };
        foreach (BonusArea area in recipe.Areas)
        {
            int slots = 0, left = 0, right = 0, palette = 0, power = 0, fortitude = 0;
            foreach (RegionEffect effect in area.Effects)
            {
                int value = effect.Arguments[0].Value;
                switch (effect.Kind)
                {
                    case 1: palette |= 1 << value; break;
                    case 3: power += value; break;
                    case 4: fortitude += value; break;
                    case 7: slots += value; break;
                    case 8: left += value; right += effect.Arguments[1].Value; break;
                }
            }
            foreach (var pair in states.ToArray())
            {
                Structure k = pair.Key;
                Structure next = new(Math.Min(3, k.Slots + slots), Math.Min(4, k.Left + left), Math.Min(4, k.Right + right), k.Palette | palette);
                int[] candidate = [pair.Value[0] + power, pair.Value[1] + fortitude, pair.Value[2] + power + fortitude];
                if (states.TryGetValue(next, out int[]? previous))
                    for (int i = 0; i < 3; i++) candidate[i] = Math.Max(candidate[i], previous[i]);
                states[next] = candidate;
            }
        }
        Dictionary<Structure, int[]> result = [];
        foreach (var pair in states)
        {
            Structure key = pair.Key.Slots == 0 ? pair.Key with { Left = 0, Right = 0 } : pair.Key;
            if (!result.TryGetValue(key, out int[]? values)) result[key] = (int[])pair.Value.Clone();
            else for (int i = 0; i < 3; i++) values[i] = Math.Max(values[i], pair.Value[i]);
        }
        return result;
    }

    /// <summary>取得三个实际面板目标之一，不把材料数量加入求解目标。</summary>
    /// <param name="card">已复算卡牌。</param>
    /// <param name="goal">0攻击、1防御、2总值。</param>
    /// <returns>该目标的最终值。</returns>
    public static int Panel(CardTemplate card, int goal) => goal switch { 0 => card.Power, 1 => card.Fortitude, _ => card.Total };

    /// <summary>只在已有候选间择优；同面板才比较Ⅲ、Ⅱ、Ⅰ阶用量，不另行搜索材料。</summary>
    /// <param name="candidate">新候选。</param>
    /// <param name="previous">已有代表。</param>
    /// <param name="goal">主要面板目标。</param>
    /// <returns>新候选是否更适合作为代表。</returns>
    public static bool Better(CardTemplate candidate, CardTemplate previous, int goal)
    {
        int comparison = Panel(candidate, goal).CompareTo(Panel(previous, goal));
        if (comparison != 0) return comparison > 0;
        comparison = (goal == 0 ? candidate.Fortitude : candidate.Power).CompareTo(goal == 0 ? previous.Fortitude : previous.Power);
        if (comparison != 0) return comparison > 0;
        for (int i = 2; i >= 0; i--)
            if (candidate.TierCounts[i] != previous.TierCounts[i]) return candidate.TierCounts[i] < previous.TierCounts[i];
        return string.CompareOrdinal(candidate.Id, previous.Id) < 0;
    }

    /// <summary>列出单颗粒子能从同一个真实池提供的待选技能子集。</summary>
    /// <param name="profile">材料能力组。</param>
    /// <param name="wanted">最多三个不同技能。</param>
    /// <returns>以wanted位置编码的可携带掩码。</returns>
    private static int[] Masks(MaterialProfile profile, int[] wanted)
    {
        List<int> masks = [0];
        for (int mask = 1; mask < 1 << wanted.Length; mask++)
        {
            int[] skills = wanted.Where((_, i) => (mask & (1 << i)) != 0).ToArray();
            if (profile.Options.Any(p => p.Capacity >= skills.Length && skills.All(p.Traits.Contains))) masks.Add(mask);
        }
        return masks.ToArray();
    }

    /// <summary>用最多三位的动态规划核对技能是否能由现有材料同时提供。</summary>
    /// <param name="profiles">当前版本材料能力表。</param>
    /// <param name="counts">内部截断后的能力组数量。</param>
    /// <param name="wanted">需要装备的不同技能。</param>
    /// <returns>组合是否真实可得。</returns>
    public static bool CanAssign(MaterialProfile[] profiles, int[] counts, int[] wanted)
    {
        int full = (1 << wanted.Length) - 1; HashSet<int> reachable = [0];
        for (int p = 0; p < profiles.Length; p++)
        {
            int[] masks = Masks(profiles[p], wanted);
            for (int n = 0; n < counts[p]; n++)
            {
                reachable = reachable.SelectMany(old => masks.Select(mask => old | mask)).ToHashSet();
                if (reachable.Contains(full)) return true;
            }
        }
        return reachable.Contains(full);
    }

    /// <summary>把所选技能落实到具体粒子编号与真实回想来源。</summary>
    /// <param name="catalog">材料和来源表。</param>
    /// <param name="card">需要制成的卡牌。</param>
    /// <param name="traits">实际装备技能。</param>
    /// <returns>可照着准备的材料清单，无法同时提供时为空引用。</returns>
    public static AssignedMaterial[]? Assign(Catalog catalog, CardTemplate card, int[] traits)
    {
        int[] wanted = traits.Where(t => t > 1).Distinct().Order().ToArray();
        if (wanted.Length == 0) return [];
        if (wanted.Length > card.Slots) return null;
        int full = (1 << wanted.Length) - 1;
        Dictionary<int, List<AssignedMaterial>> states = new() { [0] = [] };
        for (int index = 0; index < card.Placements.Length; index++)
        {
            Shape shape = catalog.Shapes[card.Placements[index].Id];
            int[] masks = Masks(catalog.Data.Profiles[shape.Profile], wanted);
            foreach (var prior in states.ToArray())
                foreach (int mask in masks)
                {
                    int combined = prior.Key | mask; if (states.ContainsKey(combined)) continue;
                    int[] selected = wanted.Where((_, i) => (mask & (1 << i)) != 0).ToArray();
                    int[] sources = shape.Sources.Where(s => s.Capacity >= selected.Length && selected.All(s.Traits.Contains))
                        .Select(s => s.Encounter).Distinct().Order().ToArray();
                    if (sources.Length == 0) continue;
                    states[combined] = [.. prior.Value, new AssignedMaterial { Piece = index + 1, Shape = shape.Id, Traits = selected, Sources = sources }];
                    if (combined == full) return states[combined].ToArray();
                }
        }
        return null;
    }
}
