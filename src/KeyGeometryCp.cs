using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>完整粒子域的连接证明，和启发式区域拼法图分开维护。</summary>
internal sealed partial class KeyRecipeSolver
{
    /// <summary>按越界粒子接触的必选安全分量数量，限制跨安全区连接的最大合并能力。</summary>
    /// <param name="model">固定收益层的几何模型。</param>
    /// <param name="take">粒子选择变量。</param>
    /// <param name="choices">与选择变量一致的真实放置。</param>
    /// <param name="set">本约束适用的激活区域集合。</param>
    /// <param name="condition">多集合模型的选择条件；单集合为空。</param>
    private void AddSafeConnectionBound(CpModel model, BoolVar[] take, Option[] choices, RegionSet set, ILiteral? condition = null)
    {
        int[] regions = Enumerable.Range(0, recipe.Areas.Length).Where(r => (set.Mask & (1u << r)) != 0).ToArray();
        ulong parts = regions.Aggregate(0UL, (bits, r) => bits | regionSafeParts[r]);
        int required = System.Numerics.BitOperations.PopCount(parts);
        int allowed = Craft.Skills[recipe.Character].Split + 1 + regions.Sum(r => effects[r, 11]);
        if (required <= allowed) return;
        // O颗越界粒子彼此连接最多额外合并O−1次；第i颗直接合并必选安全区的能力至多max(0,t_i−1)。
        // 因而总合并数至多sum(max(1,t_i))−1，零接触粒子也允许作为粒子间的中继。
        long[] weights = choices.Select(p => p.Outside == 0 ? 0L : Math.Max(1,
            System.Numerics.BitOperations.PopCount(p.Cells.Aggregate(0UL, (bits, c) => bits | safeContacts[c]) & parts))).ToArray();
        Constraint bound = model.Add(LinearExpr.WeightedSum(take, weights) >= required - allowed + 1);
        if (condition is not null) bound.OnlyEnforceIf(condition);
    }

    /// <summary>收缩必选连通格，以激活区域为需求锚点，限制实际布局的安全根数量。</summary>
    /// <param name="model">已经包含覆盖、重叠和材料数量限制的模型。</param>
    /// <param name="active">每个奖励区域是否完整激活。</param>
    /// <param name="cover">覆盖每个格子的粒子选择变量。</param>
    /// <param name="forced">全部替代集合都必须占用的奖励格。</param>
    /// <param name="sets">用于收紧激活锚点数量上界的区域集合。</param>
    private void AddConnection(CpModel model, BoolVar[] active, List<BoolVar>[] cover, HashSet<Hex> forced, RegionSet[] sets)
    {
        int[] nodes = Enumerable.Repeat(-1, cells.Length).ToArray();
        List<LinearExpr> enabled = [];
        foreach (HashSet<Hex> component in Craft.Components(forced))
        {
            int node = enabled.Count; enabled.Add(model.NewConstant(1));
            foreach (Hex c in component) nodes[cellIndex[c]] = node;
        }
        int fixedNodes = enabled.Count;
        for (int c = 0; c < cells.Length; c++)
            if (nodes[c] < 0 && cover[c].Count > 0)
            {
                BoolVar used = model.NewBoolVar($"used{c}"); model.AddMaxEquality(used, cover[c]);
                nodes[c] = enabled.Count; enabled.Add(used);
            }
        LinearExpr rootLimit = Craft.Skills[recipe.Character].Split +
            LinearExpr.WeightedSum(active, Enumerable.Range(0, active.Length).Select(i => (long)effects[i, 11])) + 1;
        // 连通块在任一六边格坐标轴上的投影都是连续区间，占用段数因此不能超过允许的连通块数。
        Func<Hex, int>[] axes = [c => c.Q, c => c.R, c => c.Q + c.R];
        for (int axis = 0; axis < axes.Length; axis++)
        {
            var slices = Enumerable.Range(0, cells.Length).Where(c => nodes[c] >= 0)
                .GroupBy(c => axes[axis](cells[c])).ToDictionary(g => g.Key, g => g.Select(c => nodes[c]).Distinct().ToArray());
            if (slices.Count == 0) continue;
            LinearExpr previous = model.NewConstant(0);
            List<BoolVar> starts = [];
            for (int coordinate = slices.Keys.Min(); coordinate <= slices.Keys.Max(); coordinate++)
            {
                BoolVar occupied = model.NewBoolVar($"axis{axis}_{coordinate}");
                if (slices.TryGetValue(coordinate, out int[]? members)) model.AddMaxEquality(occupied, members.Select(n => enabled[n]));
                else model.Add(occupied == 0);
                BoolVar start = model.NewBoolVar($"run{axis}_{coordinate}");
                model.Add(start >= occupied - previous); model.Add(start <= occupied); model.Add(start + previous <= 1);
                starts.Add(start); previous = occupied;
            }
            model.Add(LinearExpr.Sum(starts) <= rootLimit);
        }
        Dictionary<int, List<int>> areasAtNode = [];
        uint union = sets.Aggregate(0u, (mask, s) => mask | s.Mask);
        for (int i = 0; i < recipe.Areas.Length; i++)
        {
            if ((union & (1u << i)) == 0) continue;
            foreach (var component in Craft.Components(recipe.Areas[i].Cells.Select(c => c.Position)))
            {
                int n = nodes[cellIndex[component.First()]];
                if (n < 0) continue;
                if (!areasAtNode.TryGetValue(n, out var list)) areasAtNode[n] = list = [];
                list.Add(i);
            }
        }
        int capacity = sets.Max(s => areasAtNode.Count(p => p.Value.Any(i => (s.Mask & (1u << i)) != 0)));
        Dictionary<int, BoolVar> needed = [], rootAtNode = [];
        List<LinearExpr>[] flows = enabled.Select(_ => new List<LinearExpr>()).ToArray();
        List<BoolVar> roots = [];
        int first = Array.FindIndex(recipe.Areas, a => a.Cells.All(c => forced.Contains(c.Position)));
        int fixedRoot = first < 0 ? -1 : nodes[cellIndex[recipe.Areas[first].Cells[0].Position]];
        foreach (var pair in areasAtNode)
        {
            BoolVar demand = model.NewBoolVar($"needed{pair.Key}");
            model.AddMaxEquality(demand, pair.Value.Distinct().Select(i => active[i])); needed[pair.Key] = demand;
            model.Add(demand <= enabled[pair.Key]);
            BoolVar root = model.NewBoolVar($"root{pair.Key}"); rootAtNode[pair.Key] = root;
            model.Add(root <= demand); roots.Add(root);
            if (pair.Key == fixedRoot) model.Add(root == 1);
            IntVar supply = model.NewIntVar(0, capacity, $"supply{pair.Key}");
            model.Add(supply <= capacity * root); flows[pair.Key].Add(supply);
        }
        Dictionary<BoolVar, HashSet<int>> touchingRoots = [];
        for (int c = 0; c < cells.Length; c++)
        {
            int[] nearRoots = Hex.Directions.Select(cells[c].Add).Append(cells[c]).Where(cellIndex.ContainsKey)
                .Select(p => nodes[cellIndex[p]]).Where(rootAtNode.ContainsKey).Distinct().ToArray();
            if (nearRoots.Length == 0) continue;
            foreach (BoolVar particle in cover[c])
            {
                if (!touchingRoots.TryGetValue(particle, out var touched)) touchingRoots[particle] = touched = [];
                touched.UnionWith(nearRoots);
            }
        }
        // 一颗粒子及其相邻占用格属于同一连通分量，保留每分量一个根即可完整表示合法布局。
        foreach (var pair in touchingRoots)
            if (pair.Value.Count > 1) model.Add(LinearExpr.Sum(pair.Value.Select(n => rootAtNode[n])) <= 1).OnlyEnforceIf(pair.Key);
        HashSet<(int, int)> edges = [];
        for (int c = 0; c < cells.Length; c++)
            if (nodes[c] >= 0)
                foreach (Hex d in Hex.Directions.Take(3))
                    if (cellIndex.TryGetValue(cells[c].Add(d), out int other) && nodes[other] >= 0 && nodes[c] != nodes[other])
                        edges.Add((Math.Min(nodes[c], nodes[other]), Math.Max(nodes[c], nodes[other])));
        // 任一需求所在的节点集合，必须包含一个根或通过外边界上的粒子连接出去。
        // 在节点图上扩展，避免收缩的必占连通块被几何边界错误切开。
        HashSet<int>[] adjacent = enabled.Select(_ => new HashSet<int>()).ToArray();
        foreach ((int a, int b) in edges) { adjacent[a].Add(b); adjacent[b].Add(a); }
        HashSet<BoolVar>[] particlesAtNode = enabled.Select(_ => new HashSet<BoolVar>()).ToArray();
        for (int c = 0; c < cells.Length; c++)
            if (nodes[c] >= 0) particlesAtNode[nodes[c]].UnionWith(cover[c]);
        HashSet<string> seen = [];
        int cuts = 0;
        foreach (var demand in needed)
        {
            HashSet<int> inside = [demand.Key];
            for (int depth = 0; depth <= 2; depth++)
            {
                HashSet<int> boundary = inside.SelectMany(n => adjacent[n]).Where(n => !inside.Contains(n)).ToHashSet();
                if (!boundary.Any(n => n < fixedNodes) && !inside.Contains(fixedRoot))
                {
                    string signature = $"{demand.Key}:" + string.Join(',', inside.Order());
                    if (seen.Add(signature))
                    {
                        IEnumerable<ILiteral> exits = boundary.SelectMany(n => particlesAtNode[n]).Distinct().Cast<ILiteral>();
                        IEnumerable<ILiteral> localRoots = rootAtNode.Where(p => inside.Contains(p.Key)).Select(p => (ILiteral)p.Value);
                        model.AddBoolOr(exits.Concat(localRoots).Append(demand.Value.Not()));
                        cuts++;
                    }
                }
                if (boundary.Count == 0) break;
                inside.UnionWith(boundary);
            }
        }
        Console.WriteLine($"[{recipe.Id}] 连接边界：{needed.Count}个需求锚点，{cuts}条粒子/根约束。");
        if (needed.Count <= 12)
        {
            // 每个激活锚点独立发送一单位流到某个合法根，避免合并流的大容量系数削弱松弛。
            // 多条流可以共用格子和边：它们验证连通性，不代表额外粒子或物理流量。
            foreach (var terminal in needed.Where(p => p.Key != fixedRoot))
            {
                List<LinearExpr>[] route = enabled.Select(_ => new List<LinearExpr>()).ToArray();
                foreach (var root in rootAtNode)
                {
                    BoolVar supply = model.NewBoolVar($"unit{terminal.Key}_root{root.Key}");
                    model.Add(supply <= root.Value); model.Add(supply <= terminal.Value);
                    route[root.Key].Add(supply);
                }
                foreach ((int a, int b) in edges)
                {
                    IntVar flow = model.NewIntVar(-1, 1, $"unit{terminal.Key}_{a}_{b}");
                    model.Add(flow == 0).OnlyEnforceIf(terminal.Value.Not());
                    model.Add(flow <= enabled[a]); model.Add(flow >= -enabled[a]);
                    model.Add(flow <= enabled[b]); model.Add(flow >= -enabled[b]);
                    route[a].Add(-flow); route[b].Add(flow);
                }
                for (int n = 0; n < route.Length; n++)
                    model.Add(LinearExpr.Sum(route[n]) == (n == terminal.Key ? (LinearExpr)terminal.Value : model.NewConstant(0)));
            }
            model.Add(LinearExpr.Sum(roots) <= rootLimit);
            Console.WriteLine($"[{recipe.Id}] 单位连接流：{needed.Count}组，{edges.Count}条空间边。");
            return;
        }
        foreach ((int a, int b) in edges)
        {
            IntVar flow = model.NewIntVar(-capacity, capacity, $"flow{a}_{b}");
            model.Add(flow <= capacity * enabled[a]); model.Add(flow >= -capacity * enabled[a]);
            model.Add(flow <= capacity * enabled[b]); model.Add(flow >= -capacity * enabled[b]);
            flows[a].Add(-flow); flows[b].Add(flow);
        }
        IntVar zero = model.NewConstant(0);
        for (int n = 0; n < flows.Length; n++)
            model.Add(LinearExpr.Sum(flows[n]) == (needed.TryGetValue(n, out BoolVar? demand) ? (LinearExpr)demand : zero));
        model.Add(LinearExpr.Sum(roots) <= rootLimit);
    }
}
