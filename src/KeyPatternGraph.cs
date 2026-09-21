using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>用区域完整拼法作为连通节点，避免对其内部格子重复搜索连接流。</summary>
internal sealed partial class KeyRecipeSolver
{
    /// <summary>在有限局部拼法库中寻找区域接触图；失败只表示快速路径未命中。</summary>
    /// <param name="set">收益已固定的区域集合。</param>
    /// <param name="seconds">求解图的时间片，秒。</param>
    /// <returns>合法布局或未决；此方法不提供全域不可行证明。</returns>
    private (CardTemplate? Card, CpSolverStatus Status) PatternGraph(RegionSet set, double seconds)
    {
        if (SearchBudget <= 0) return (null, CpSolverStatus.Unknown);
        progress = target + " 区域图建模";
        int[] regions = Enumerable.Range(0, recipe.Areas.Length).Where(i => (set.Mask & (1u << i)) != 0).ToArray();
        if (regions.Any(i => Craft.Components(recipe.Areas[i].Cells.Select(c => c.Position)).Count != 1)) return (null, CpSolverStatus.Unknown);
        List<Option[]> pools = [];
        foreach (int region in regions)
        {
            Option[] pool = Patterns(region).Select(pattern =>
            {
                Piece[] parts = pattern.Select(i => pieces[i]).ToArray();
                return new Option(parts.Select(p => p.Placement).ToArray(), parts.SelectMany(p => p.Cells).ToArray(), parts.SelectMany(p => p.Matches).Distinct().ToArray(), 1u << region, parts.Count(p => p.Outside));
            }).ToArray();
            if (pool.Length == 0) return (null, CpSolverStatus.Unknown);
            pools.Add(pool);
        }
        int outsideLimit = Craft.Skills[recipe.Character].Outside + regions.Sum(i => effects[i, 9]);
        int overlapLimit = Craft.Skills[recipe.Character].Overlap + regions.Sum(i => effects[i, 10]);
        int countLimit = limit - regions.Sum(i => effects[i, 15]);
        int bridges = Math.Min(2, countLimit - set.Cost);
        if (bridges > 0)
        {
            Dictionary<int, uint> near = [];
            for (int r = 0; r < pools.Count; r++)
                foreach (Hex c in pools[r].SelectMany(p => p.Cells).Distinct().SelectMany(c => Hex.Directions.Select(cells[c].Add).Append(cells[c])))
                    if (cellIndex.TryGetValue(c, out int id)) near[id] = near.GetValueOrDefault(id) | (1u << r);
            var candidates = pieces.Where(p => (p.Regions & set.Mask) == 0 && (!p.Outside || outsideLimit > 0))
                .Select(p => (p, contacts: System.Numerics.BitOperations.PopCount(p.Cells.Aggregate(0u, (mask, c) => mask | near.GetValueOrDefault(c)))))
                .Where(p => p.contacts >= 2).OrderByDescending(p => p.contacts).ThenBy(p => p.p.Outside).ThenByDescending(p => p.p.Cells.Length).Take(192)
                .Select(p => new Option([p.p.Placement], p.p.Cells, p.p.Matches, p.p.Regions, p.p.Outside ? 1 : 0)).ToArray();
            Option[] bridgePool = [new Option([], [], [], 0, 0), .. candidates];
            for (int i = 0; i < bridges; i++) pools.Add(bridgePool);
        }
        CpModel model = new();
        IntVar[] choice = pools.Select((p, i) => model.NewIntVar(0, p.Length - 1, $"choice{i}")).ToArray();
        BoolVar[][] chosen = pools.Select((p, i) => p.Select((_, j) => model.NewBoolVar($"chosen{i}_{j}")).ToArray()).ToArray();
        List<LinearExpr> count = [], outside = [];
        List<BoolVar>[] cover = cells.Select(_ => new List<BoolVar>()).ToArray();
        List<BoolVar>[] matches = Enumerable.Range(0, recipe.Areas.Sum(a => a.Cells.Length)).Select(_ => new List<BoolVar>()).ToArray();
        BoolVar[] used = pools.Select((_, i) => model.NewBoolVar($"used{i}")).ToArray();
        for (int i = 0; i < pools.Count; i++)
        {
            model.AddExactlyOne(chosen[i]);
            for (int j = 0; j < pools[i].Length; j++)
            {
                model.Add(choice[i] == j).OnlyEnforceIf(chosen[i][j]);
                Option p = pools[i][j];
                count.Add(chosen[i][j] * p.Layout.Length); outside.Add(chosen[i][j] * p.Outside);
                foreach (int c in p.Cells) cover[c].Add(chosen[i][j]);
                foreach (int b in p.Matches) matches[b].Add(chosen[i][j]);
            }
            if (i < regions.Length) model.Add(used[i] == 1);
            else { model.Add(choice[i] >= 1).OnlyEnforceIf(used[i]); model.Add(choice[i] == 0).OnlyEnforceIf(used[i].Not()); }
        }
        if (bridges == 2) model.Add(choice[^2] <= choice[^1]);
        model.Add(LinearExpr.Sum(count) <= countLimit); model.Add(LinearExpr.Sum(outside) <= outsideLimit);
        List<BoolVar> overlap = [];
        for (int c = 0; c < cover.Length; c++)
        {
            if (cover[c].Count < 2) continue;
            model.Add(LinearExpr.Sum(cover[c]) <= Craft.MaxStack);
            BoolVar over = model.NewBoolVar($"over{c}");
            model.Add(LinearExpr.Sum(cover[c]) <= 1).OnlyEnforceIf(over.Not()); overlap.Add(over);
        }
        model.Add(LinearExpr.Sum(overlap) <= overlapLimit);
        int bit = 0;
        for (int r = 0; r < recipe.Areas.Length; r++)
        {
            List<ILiteral> missing = [];
            foreach (BoardCell cell in recipe.Areas[r].Cells)
            {
                if ((set.Mask & (1u << r)) == 0)
                {
                    BoolVar absent = model.NewBoolVar($"missing{bit}");
                    model.Add(LinearExpr.Sum(matches[bit]) == 0).OnlyEnforceIf(absent); missing.Add(absent);
                }
                bit++;
            }
            if (missing.Count > 0) model.AddBoolOr(missing);
        }
        int capacity = pools.Count;
        List<LinearExpr>[] flows = pools.Select(_ => new List<LinearExpr>()).ToArray();
        List<BoolVar> roots = [];
        for (int i = 0; i < regions.Length; i++)
        {
            BoolVar root = model.NewBoolVar($"root{i}"); roots.Add(root);
            if (i == 0) model.Add(root == 1);
            IntVar source = model.NewIntVar(0, capacity, $"source{i}"); model.Add(source <= capacity * root); flows[i].Add(source);
        }
        List<BoolVar>[] incidence = pools.Select(_ => new List<BoolVar>()).ToArray();
        HashSet<int>[] neighbors = pools.Select(_ => new HashSet<int>()).ToArray();
        HashSet<int>[][] adjacent = pools.Select(pool => pool.Select(p => p.Cells.SelectMany(c => Hex.Directions.Select(cells[c].Add).Append(cells[c])).Where(cellIndex.ContainsKey).Select(c => cellIndex[c]).ToHashSet()).ToArray()).ToArray();
        for (int a = 0; a < pools.Count; a++)
            for (int b = a + 1; b < pools.Count; b++)
            {
                List<(int, int)> touching = [];
                for (int x = 0; x < pools[a].Length; x++)
                    for (int y = 0; y < pools[b].Length; y++)
                        if (pools[b][y].Cells.Any(adjacent[a][x].Contains)) touching.Add((x, y));
                if (touching.Count == 0) continue;
                BoolVar edge = model.NewBoolVar($"edge{a}_{b}"); model.Add(edge <= used[a]); model.Add(edge <= used[b]);
                incidence[a].Add(edge); incidence[b].Add(edge); neighbors[a].Add(b); neighbors[b].Add(a);
                long[,] tuples = new long[touching.Count, 2];
                for (int i = 0; i < touching.Count; i++) { tuples[i, 0] = touching[i].Item1; tuples[i, 1] = touching[i].Item2; }
                model.AddAllowedAssignments([choice[a], choice[b]]).AddTuples(tuples).OnlyEnforceIf(edge);
                IntVar flow = model.NewIntVar(-capacity, capacity, $"flow{a}_{b}"); model.Add(flow <= capacity * edge); model.Add(flow >= -capacity * edge);
                flows[a].Add(-flow); flows[b].Add(flow);
            }
        for (int i = 0; i < pools.Count; i++)
        {
            model.Add(LinearExpr.Sum(flows[i]) == used[i]);
            model.Add(LinearExpr.Sum(incidence[i]) + (i < roots.Count ? (LinearExpr)roots[i] : model.NewConstant(0)) >= used[i]);
        }
        HashSet<int> remaining = Enumerable.Range(0, pools.Count).ToHashSet();
        while (remaining.Count > 0)
        {
            int firstNode = remaining.First(); remaining.Remove(firstNode);
            List<int> component = [firstNode];
            for (int j = 0; j < component.Count; j++)
                foreach (int next in neighbors[component[j]]) if (remaining.Remove(next)) component.Add(next);
            int rootNode = component.Where(n => n < regions.Length).DefaultIfEmpty(-1).Min();
            if (rootNode >= 0) model.Add(roots[rootNode] == 1);
        }
        model.Add(LinearExpr.Sum(roots) <= Craft.Skills[recipe.Character].Split + regions.Sum(i => effects[i, 11]) + 1);
        seconds = Math.Min(seconds, SearchBudget);
        if (seconds <= 0) return (null, CpSolverStatus.Unknown);
        int workers = threads;
        progress = $"{target} 区域图 {workers}线程";
        CpSolver solver = new() { StringParameters = FormattableString.Invariant($"max_time_in_seconds:{seconds} num_search_workers:{workers} cp_model_probing_level:0 linearization_level:0") };
        using CancellationTokenRegistration registration = cancellation.Register(solver.StopSearch);
        Stopwatch clock = Stopwatch.StartNew(); CpSolverStatus status = solver.Solve(model); cancellation.ThrowIfCancellationRequested();
        Console.WriteLine($"[{recipe.Id}] 区域图 {pools.Count}节点 {status} {clock.Elapsed.TotalSeconds:F3}秒。");
        if (status == CpSolverStatus.ModelInvalid) throw new InvalidDataException(solver.ResponseStats());
        if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible)) return (null, CpSolverStatus.Unknown);
        CardTemplate card = Craft.Evaluate(catalog, recipe, pools.SelectMany((p, i) => p[(int)solver.Value(choice[i])].Layout));
        uint actual = card.Active.Aggregate(0u, (mask, i) => mask | (1u << i));
        if (!card.Valid || card.Strikes != 0 || actual != set.Mask) throw new InvalidDataException($"区域图复核不一致：{recipe.Id}/{set.Mask}/{actual}。");
        return (card, status);
    }
}
