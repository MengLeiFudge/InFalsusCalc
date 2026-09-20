namespace InFalsusCalc;

/// <summary>根据必选奖励、重叠额度和覆盖后的剩余粒子数缩小完整几何域。</summary>
internal sealed partial class KeyRecipeSolver
{
    /// <summary>同一激活集合可复用的安全放置域，值为原始放置编号。</summary>
    private readonly Dictionary<uint, int[]> prunedDomains = [];
    /// <summary>每种放置的占用和相邻格索引，复用于连接距离松弛。</summary>
    private int[][]? pieceBorders;

    /// <summary>按局部惩罚成本与可抵消乖离的接口收益筛选正常拼法。</summary>
    /// <param name="region">被完整覆盖的奖励区域。</param>
    /// <param name="patterns">有限枚举得到的合法局部覆盖模式。</param>
    /// <returns>最低成本模式，以及额外成本能换取足够新连接的模式。</returns>
    private List<int[]> PrunePatterns(int region, List<int[]> patterns)
    {
        if (patterns.Count == 0) return patterns;
        uint TargetBits(int[] pattern)
        {
            HashSet<int> border = pattern.SelectMany(i => pieceBorders![i]).ToHashSet();
            uint targets = 0;
            for (int other = 0; other < recipe.Areas.Length; other++)
                if (other != region && recipe.Areas[other].Cells.Any(c => border.Contains(cellIndex[c.Position]))) targets |= 1u << other;
            return targets;
        }
        (int Cost, uint Targets) Describe(int[] pattern)
        {
            int outside = pattern.Count(i => pieces[i].Outside);
            int overlap = pattern.SelectMany(i => pieces[i].Cells).GroupBy(c => c).Count(g => g.Count() > 1);
            return (pattern.Length + outside + overlap, TargetBits(pattern));
        }
        pieceBorders ??= pieces.Select(p => p.Cells.SelectMany(c => Hex.Directions.Select(cells[c].Add).Append(cells[c]))
            .Where(cellIndex.ContainsKey).Select(c => cellIndex[c]).Distinct().ToArray()).ToArray();
        var described = patterns.Select(p => (Pattern: p.Distinct().Order().ToArray(), Info: Describe(p))).GroupBy(x => string.Join(',', x.Pattern)).Select(g => g.First()).ToArray();
        int minimum = described.Min(x => x.Info.Cost);
        var cheapest = described.Where(x => x.Info.Cost == minimum).ToArray();
        var kept = described.Where(candidate =>
        {
            if (candidate.Info.Cost == minimum) return true;
            int extra = candidate.Info.Cost - minimum;
            return cheapest.Any(reference => System.Numerics.BitOperations.PopCount(candidate.Info.Targets & ~reference.Info.Targets) >= extra);
        }).OrderBy(x => x.Info.Cost).ThenByDescending(x => System.Numerics.BitOperations.PopCount(x.Info.Targets)).Take(192).Select(x => x.Pattern).ToList();
        Console.WriteLine($"[{recipe.Id}] 区域{region}局部置信剪枝：{patterns.Count}→{kept.Count}，最低成本{minimum}。");
        return kept;
    }

    /// <summary>保留每种替代区域集合所需放置的并集；大集合仅应用共同必选格约束。</summary>
    /// <param name="sets">当前收益层仍可能成立的全部激活集合。</param>
    /// <returns>仍能表示至少一份最优面板合法布局的粒子放置域。</returns>
    private Piece[] PruneChoices(RegionSet[] sets)
    {
        bool exact = sets.Length <= 32;
        uint union = sets.Aggregate(0u, (mask, s) => mask | s.Mask);
        uint[] masks = exact ? sets.Select(s => s.Mask).ToArray() : [sets.Aggregate(uint.MaxValue, (mask, s) => mask & s.Mask)];
        bool[] keep = new bool[pieces.Count];
        foreach (uint mask in masks)
        {
            cancellation.ThrowIfCancellationRequested();
            if (exact && prunedDomains.TryGetValue(mask, out int[]? cached))
            {
                foreach (int i in cached) keep[i] = true;
                continue;
            }
            RegionSet[] alternatives = exact ? sets.Where(s => s.Mask == mask).ToArray() : sets;
            int Sum(uint regions, int kind) => Enumerable.Range(0, recipe.Areas.Length).Where(i => (regions & (1u << i)) != 0).Sum(i => effects[i, kind]);
            int spare = alternatives.Max(s => limit - s.Cost - Sum(s.Mask, 15));
            int countLimit = alternatives.Max(s => limit - Sum(s.Mask, 15));
            int overlap = Craft.Skills[recipe.Character].Overlap + alternatives.Max(s => Sum(s.Mask, 10));
            int outside = Craft.Skills[recipe.Character].Outside + alternatives.Max(s => Sum(s.Mask, 9));
            uint useful = exact ? mask : union;
            ulong requiredParts = exact ? Enumerable.Range(0, recipe.Areas.Length).Where(r => (mask & (1u << r)) != 0)
                .Aggregate(0UL, (bits, r) => bits | regionSafeParts[r]) : 0;
            int roots = Craft.Skills[recipe.Character].Split + 1 + (exact ? Sum(mask, 11) : 0);
            int merge = exact ? Math.Max(0, System.Numerics.BitOperations.PopCount(requiredParts) - roots) : 0;
            int[] colors = new int[cells.Length];
            HashSet<Hex> forced = [];
            for (int r = 0; r < recipe.Areas.Length; r++)
                if ((mask & (1u << r)) != 0)
                    foreach (BoardCell c in recipe.Areas[r].Cells)
                    { colors[cellIndex[c.Position]] = c.Color; forced.Add(c.Position); }
            List<HashSet<Hex>>? parts = exact ? Craft.Components(forced) : null;
            if (parts is not null && parts.Count <= Craft.Skills[recipe.Character].Split + 1 + Sum(mask, 11)) spare = 0;
            List<int> compatible = [];
            for (int i = 0; i < pieces.Count; i++)
            {
                Piece p = pieces[i];
                if (p.Outside && outside == 0) continue;
                if (exact && outside <= 1)
                {
                    ulong contacts = p.Cells.Aggregate(0UL, (bits, c) => bits | safeContacts[c]) & requiredParts;
                    // 只有一颗越界粒子时，跨安全区的连接只能经由它；它必须独自完成必要的合并数。
                    if (p.Outside && merge > 0 && System.Numerics.BitOperations.PopCount(contacts) < merge + 1) continue;
                    // 没有必选奖励的安全区只能成为这颗越界粒子的冗余支枝，删除后不影响任何奖励间的连接。
                    if (!p.Outside && contacts == 0) continue;
                }
                int color = catalog.Shapes[p.Placement.Id].Color;
                int wrong = 0;
                foreach (int c in p.Cells) if (colors[c] != 0 && colors[c] != color) wrong++;
                if (wrong > overlap) continue;
                if (spare == 0 && (p.Regions & useful) == 0) continue;
                if (exact && independent && (p.Regions & mask) != 0)
                {
                    int region = System.Numerics.BitOperations.TrailingZeroCount(p.Regions & mask);
                    int rest = (recipe.Areas[region].Cells.Length - p.Matches.Length + regionMaxCover[region] - 1) / regionMaxCover[region];
                    if (1 + coverageLower[mask & ~(1u << region)] + rest > countLimit) continue;
                }
                compatible.Add(i);
            }
            if (exact && spare is >= 1 and <= 4)
            {
                if (parts!.Count <= 64)
                {
                    ulong[] labels = new ulong[cells.Length];
                    ulong[][] near = Enumerable.Range(0, outside + 1).Select(_ => new ulong[cells.Length]).ToArray();
                    for (int r = 0; r < parts.Count; r++)
                        foreach (Hex c in parts[r]) labels[cellIndex[c]] = 1UL << r;
                    pieceBorders ??= pieces.Select(p => p.Cells.SelectMany(c => Hex.Directions.Select(cells[c].Add).Append(cells[c]))
                        .Where(cellIndex.ContainsKey).Select(c => cellIndex[c]).Distinct().ToArray()).ToArray();
                    foreach (int i in compatible)
                    {
                        Piece p = pieces[i];
                        if ((p.Regions & mask) == 0) continue;
                        ulong touched = p.Cells.Aggregate(0UL, (bits, c) => bits | labels[c]);
                        for (int budget = p.Outside ? 1 : 0; budget <= outside; budget++)
                            foreach (int c in pieceBorders[i]) near[budget][c] |= touched;
                    }
                    int[] neutral = compatible.Where(i => (pieces[i].Regions & mask) == 0).ToArray();
                    ulong[][][] reach = new ulong[spare][][];
                    for (int depth = 0; depth < spare; depth++)
                    {
                        cancellation.ThrowIfCancellationRequested();
                        reach[depth] = Enumerable.Range(0, outside + 1).Select(_ => new ulong[pieces.Count]).ToArray();
                        foreach (int i in neutral)
                        {
                            int spent = pieces[i].Outside ? 1 : 0;
                            for (int budget = spent; budget <= outside; budget++)
                                reach[depth][budget][i] = pieces[i].Cells.Aggregate(0UL, (bits, c) => bits | near[budget - spent][c]);
                        }
                        if (depth == spare - 1) break;
                        ulong[][] next = near.Select(a => (ulong[])a.Clone()).ToArray();
                        foreach (int i in neutral)
                            for (int budget = 0; budget <= outside; budget++)
                                if (reach[depth][budget][i] != 0)
                                    foreach (int c in pieceBorders[i]) next[budget][c] |= reach[depth][budget][i];
                        near = next;
                    }
                    // 最小连接布局中的纯连接粒子位于两个必选块之间；计入两端覆盖粒子，且中心粒子的越界消耗只计一次。
                    HashSet<int> dead = [];
                    foreach (int i in neutral)
                    {
                        bool possible = false;
                        for (int left = 0; left < spare && !possible; left++)
                            for (int budget = 0; budget <= outside && !possible; budget++)
                            {
                                int rightBudget = Math.Min(outside, outside + (pieces[i].Outside ? 1 : 0) - budget);
                                ulong a = reach[left][budget][i], b = reach[spare - 1 - left][rightBudget][i];
                                possible = a != 0 && b != 0 && System.Numerics.BitOperations.PopCount(a | b) >= 2;
                            }
                        if (!possible) dead.Add(i);
                    }
                    compatible.RemoveAll(dead.Contains);
                }
            }
            int[] domain = compatible.ToArray();
            if (exact) prunedDomains[mask] = domain;
            foreach (int i in domain) keep[i] = true;
        }
        Piece[] result = pieces.Where((_, i) => keep[i]).ToArray();
        Console.WriteLine($"[{recipe.Id}] 安全剪枝：{pieces.Count}→{result.Length}放置，{sets.Length}组区域。");
        return result;
    }
}
