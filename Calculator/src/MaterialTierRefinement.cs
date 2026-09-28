using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>在卡牌结果与粒子数不变时，依次减少原始总惩罚、三阶与二阶粒子。</summary>
internal sealed class MaterialTierRefinement
{
    /// <summary>参与卡库指纹并标记检查点已完成材料精化的策略版本。</summary>
    public const string Policy = "material-tiers-v2";
    /// <summary>正常生成管线给每张卡的求解时间片参考值，单位秒。</summary>
    public const double DefaultSecondsPerCard = 30;

    private readonly Catalog catalog;
    private readonly Recipe recipe;
    private readonly Hex[] cells;
    private readonly Dictionary<Hex, int> cellIndex;
    private readonly HashSet<Hex> safe;
    private readonly Target[] targets;
    private readonly int[][] areaTargets;
    private readonly Candidate[] candidates;
    private readonly Dictionary<(int Id, int Q, int R), int> candidateIndex;
    private readonly Dictionary<string, HashSet<string>> abilitySets = [];
    private readonly Dictionary<string, long[][]> carrierVectors = [];

    /// <summary>一次精化的合法结果及本次是否闭合了更低目标值的搜索空间。</summary>
    /// <param name="Card">已由完整规则复算的卡牌；未改善时为原卡。</param>
    /// <param name="Complete">不存在更低原始总惩罚或材料阶级的合法布局，或已经找到全局最低目标值。</param>
    /// <param name="Seconds">本次求解墙钟秒数。</param>
    internal sealed record Outcome(CardTemplate Card, bool Complete, double Seconds);

    /// <summary>一个奖励格及其要求的原生颜色。</summary>
    private readonly record struct Target(Hex Cell, int Color);

    /// <summary>一项完整合法放置及其求解模型索引。</summary>
    private sealed record Candidate(Placement Placement, int[] Cells, int[] Matches, bool Outside, int Tier, int Profile);

    /// <summary>预编译一个配方的全部形状与平移，精化同配方多张卡时复用。</summary>
    /// <param name="catalog">当前材料、形状和技能资源。</param>
    /// <param name="recipe">需要精化布局的配方。</param>
    /// <param name="cancellation">预编译期间的用户取消信号。</param>
    public MaterialTierRefinement(Catalog catalog, Recipe recipe, CancellationToken cancellation)
    {
        this.catalog = catalog;
        this.recipe = recipe;
        cells = recipe.Board.Select(cell => cell.Position).Distinct().ToArray();
        cellIndex = cells.Select((cell, index) => (cell, index)).ToDictionary(item => item.cell, item => item.index);
        safe = recipe.Safe.Select(cell => cell.Position).ToHashSet();

        List<Target> targetList = [];
        areaTargets = new int[recipe.Areas.Length][];
        for (int area = 0; area < recipe.Areas.Length; area++)
        {
            areaTargets[area] = recipe.Areas[area].Cells.Select(cell =>
            {
                int index = targetList.Count;
                targetList.Add(new(cell.Position, cell.Color));
                return index;
            }).ToArray();
        }
        targets = targetList.ToArray();

        List<Candidate> placements = [];
        foreach (Shape shape in catalog.Data.Shapes.OrderBy(shape => shape.Id))
        {
            Hex origin = shape.Cells[0];
            foreach (Hex anchor in cells)
            {
                cancellation.ThrowIfCancellationRequested();
                Hex at = new(anchor.Q - origin.Q, anchor.R - origin.R);
                Hex[] occupied = shape.Cells.Select(cell => cell.Add(at)).ToArray();
                if (occupied.Any(cell => !cellIndex.ContainsKey(cell)))
                    continue;
                int[] occupiedIndices = occupied.Select(cell => cellIndex[cell]).Distinct().Order().ToArray();
                int[] matches = Enumerable.Range(0, targets.Length)
                    .Where(index => targets[index].Color == shape.Color && occupied.Contains(targets[index].Cell)).ToArray();
                placements.Add(new(new Placement { Id = shape.Id, Q = at.Q, R = at.R }, occupiedIndices, matches,
                    occupied.Any(cell => !safe.Contains(cell)), shape.Tier, shape.Profile));
            }
        }
        candidates = placements.OrderBy(item => item.Placement.Id).ThenBy(item => item.Placement.Q).ThenBy(item => item.Placement.R).ToArray();
        candidateIndex = candidates.Select((item, index) => (item, index))
            .ToDictionary(pair => (pair.item.Placement.Id, pair.item.Placement.Q, pair.item.Placement.R), pair => pair.index);
    }

    /// <summary>在给定墙钟预算内按原始总惩罚、三阶数、二阶数寻找严格改善的等价布局。</summary>
    /// <param name="source">完整复算且属于当前配方的基准卡。</param>
    /// <param name="seconds">本张卡的求解时间片；实际墙钟另含CP模型构建。</param>
    /// <param name="threads">CP-SAT可使用的原生线程数。</param>
    /// <param name="cancellation">用户或上层阶段取消信号。</param>
    /// <returns>最优性状态和预算内找到的最低目标值合法结果。</returns>
    public Outcome Refine(CardTemplate source, double seconds, int threads, CancellationToken cancellation)
    {
        if (source.Recipe != recipe.Id || seconds <= 0 || threads < 1)
            throw new ArgumentOutOfRangeException(nameof(source), "材料精化的配方、预算或线程数无效。");
        CardTemplate baseline = Craft.Evaluate(catalog, recipe, source.Placements);
        if (baseline.Id != source.Id || !SameCardResult(source, baseline) || !PreservesAbilities(source, baseline))
            throw new InvalidDataException($"配方{recipe.Id}的材料精化基准无法通过完整复算：{source.Id}。");
        if (source.Placements.Select(placement => (placement.Id, placement.Q, placement.R)).Distinct().Count() != source.Count)
            throw new InvalidDataException($"配方{recipe.Id}的布局含重复放置，布尔模型无法保持粒子数：{source.Id}。");

        Stopwatch timer = Stopwatch.StartNew();
        CardTemplate best = source;
        long bestCost = RefinementCost(best);

        CpModel model = new();
        BoolVar[] take = candidates.Select((_, index) => model.NewBoolVar($"p{index}")).ToArray();
        List<BoolVar>[] cover = cells.Select(_ => new List<BoolVar>()).ToArray();
        List<BoolVar>[] match = targets.Select(_ => new List<BoolVar>()).ToArray();
        for (int i = 0; i < candidates.Length; i++)
        {
            foreach (int cell in candidates[i].Cells)
                cover[cell].Add(take[i]);
            foreach (int target in candidates[i].Matches)
                match[target].Add(take[i]);
        }
        model.Add(LinearExpr.Sum(take) == source.Count);
        foreach (List<BoolVar> at in cover)
            model.Add(LinearExpr.Sum(at) <= Craft.MaxStack);

        BoolVar[] filled = targets.Select((_, index) =>
        {
            BoolVar value = model.NewBoolVar($"filled{index}");
            if (match[index].Count == 0)
                model.Add(value == 0);
            else
                model.AddMaxEquality(value, match[index]);
            return value;
        }).ToArray();
        HashSet<int> active = source.Active.ToHashSet();
        for (int area = 0; area < areaTargets.Length; area++)
        {
            if (active.Contains(area))
                foreach (int target in areaTargets[area])
                    model.Add(filled[target] == 1);
            else
                model.AddBoolOr(areaTargets[area].Select(target => (ILiteral)filled[target].Not()));
        }

        int outsideAllowance = Craft.Skills[recipe.Character].Outside + ActiveEffect(source, 9);
        LinearExpr outside = LinearExpr.Sum(candidates.Select((candidate, index) => (candidate, index))
            .Where(pair => pair.candidate.Outside).Select(pair => take[pair.index]));
        PreservePenalty(model, outside, outsideAllowance, source.Penalties[1]);

        BoolVar[] overlap = new BoolVar[cells.Length];
        BoolVar[] occupied = new BoolVar[cells.Length];
        for (int cell = 0; cell < cells.Length; cell++)
        {
            BoolVar used = model.NewBoolVar($"occupied{cell}");
            if (cover[cell].Count == 0)
                model.Add(used == 0);
            else
                model.AddMaxEquality(used, cover[cell]);
            occupied[cell] = used;
            BoolVar over = model.NewBoolVar($"overlap{cell}");
            model.Add(LinearExpr.Sum(cover[cell]) >= 2).OnlyEnforceIf(over);
            model.Add(LinearExpr.Sum(cover[cell]) <= 1).OnlyEnforceIf(over.Not());
            overlap[cell] = over;
        }
        int overlapAllowance = Craft.Skills[recipe.Character].Overlap + ActiveEffect(source, 10);
        PreservePenalty(model, LinearExpr.Sum(overlap), overlapAllowance, source.Penalties[2]);

        int splitAllowance = Craft.Skills[recipe.Character].Split + ActiveEffect(source, 11);
        int componentLimit = source.Penalties[3] == 0 ? splitAllowance + 1 : splitAllowance + source.Penalties[3] + 1;
        IntVar componentCount = ModelComponents(model, occupied, Math.Min(cells.Length, componentLimit));
        LinearExpr split = componentCount - 1;
        PreservePenalty(model, split, splitAllowance, source.Penalties[3]);
        ConstrainAbilities(model, take, source);

        long[] tierCosts = candidates.Select(candidate => candidate.Tier switch
        {
            3 => (long)source.Count + 1,
            2 => 1L,
            _ => 0L
        }).ToArray();
        LinearExpr tierCost = LinearExpr.WeightedSum(take, tierCosts);
        LinearExpr rawTotal = outside + LinearExpr.Sum(overlap) + split + source.RawPenalties[0];
        LinearExpr refinementCost = rawTotal * MaterialScale(source.Count) + tierCost;
        model.Add(refinementCost < bestCost);
        BoolVar repairNeighborhood = LimitRepairNeighborhood(model, take, source);
        model.AddAssumption(repairNeighborhood);
        ApplyHints(model, take, best);
        LinearExpr retained = LinearExpr.Sum(best.Placements.Select(placement =>
            take[candidateIndex[(placement.Id, placement.Q, placement.R)]]));
        model.Maximize(retained);

        (CpSolverStatus Status, CpSolver Solver) SolveUntil(double deadline, bool repairHint = false)
        {
            double remaining = Math.Max(.001, deadline - timer.Elapsed.TotalSeconds);
            string repairParameters = repairHint
                ? " repair_hint:true hint_conflict_limit:100000 search_branching:HINT_SEARCH log_search_progress:true log_to_stdout:true"
                    + " presolve_inclusion_work_limit:0 symmetry_level:0 max_presolve_iterations:1"
                : "";
            int presolveLevel = repairHint ? 0 : 2;
            CpSolver solver = new()
            {
                StringParameters = FormattableString.Invariant(
                    $"max_time_in_seconds:{remaining} num_search_workers:{threads} random_seed:1 linearization_level:{presolveLevel} cp_model_probing_level:{presolveLevel}{repairParameters}")
            };
            CpSolverStatus status;
            using (cancellation.Register(solver.StopSearch))
                status = solver.Solve(model);
            cancellation.ThrowIfCancellationRequested();
            if (status == CpSolverStatus.ModelInvalid)
                throw new InvalidDataException(solver.ResponseStats());
            return (status, solver);
        }

        CardTemplate ReadCandidate(CpSolver solver)
        {
            int[] chosen = Enumerable.Range(0, take.Length).Where(index => solver.Value(take[index]) != 0).ToArray();
            CardTemplate candidate = Craft.Evaluate(catalog, recipe, chosen.Select(index => candidates[index].Placement));
            if (!SameCardResult(source, candidate) || !PreservesAbilities(source, candidate))
                throw new InvalidDataException($"配方{recipe.Id}的完整材料模型返回了非等价布局：{source.Id}。");
            return candidate;
        }

        if (timer.Elapsed.TotalSeconds >= seconds)
            return new(best, false, timer.Elapsed.TotalSeconds);
        double repairDeadline = Math.Min(seconds, timer.Elapsed.TotalSeconds
            + Math.Max(.25, (seconds - timer.Elapsed.TotalSeconds) * .4));
        (CpSolverStatus repairStatus, CpSolver repairSolver) = SolveUntil(repairDeadline, repairHint: true);
        if (repairStatus is CpSolverStatus.Optimal or CpSolverStatus.Feasible)
        {
            CardTemplate candidate = ReadCandidate(repairSolver);
            if (RefinementCost(candidate) >= bestCost)
                throw new InvalidDataException($"配方{recipe.Id}的最小改动阶段返回了非改善布局：{source.Id}。");
            best = candidate;
            bestCost = RefinementCost(best);
        }

        if (timer.Elapsed.TotalSeconds >= seconds)
            return new(best, false, timer.Elapsed.TotalSeconds);
        model.ClearAssumptions();
        model.Add(repairNeighborhood == 0);
        model.Add(refinementCost < bestCost);
        model.Minimize(refinementCost);
        ApplyHints(model, take, best);
        (CpSolverStatus optimizeStatus, CpSolver optimizeSolver) = SolveUntil(seconds);
        if (optimizeStatus == CpSolverStatus.Infeasible)
            return new(best, true, timer.Elapsed.TotalSeconds);
        if (optimizeStatus is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible))
            return new(best, false, timer.Elapsed.TotalSeconds);

        CardTemplate optimized = ReadCandidate(optimizeSolver);
        if (RefinementCost(optimized) >= bestCost)
            throw new InvalidDataException($"配方{recipe.Id}的材料目标返回了非改善布局：{source.Id}。");
        best = optimized;
        return new(best, optimizeStatus == CpSolverStatus.Optimal, timer.Elapsed.TotalSeconds);
    }

    /// <summary>将一个完整配方检查点的全部代表替换为较低原始惩罚或材料阶级的等价布局，并重映射组引用。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="recipe">检查点所属配方。</param>
    /// <param name="result">待原位更新的完整检查点。</param>
    /// <param name="secondsPerCard">每张卡的求解时间片。</param>
    /// <param name="threads">单张卡求解线程数。</param>
    /// <param name="cancellation">上层阶段取消信号。</param>
    /// <returns>旧模板编号到新模板编号的完整映射。</returns>
    public static Dictionary<string, string> RefineResult(Catalog catalog, Recipe recipe, RecipeResult result,
        double secondsPerCard, int threads, CancellationToken cancellation)
    {
        if (result.Recipe != recipe.Id || !result.Complete)
            throw new InvalidDataException($"配方{recipe.Id}的材料精化要求完整兼容检查点。");
        if (result.MaterialPolicy == Policy)
            return result.Cards.Keys.ToDictionary(id => id, id => id);

        MaterialTierRefinement refinement = new(catalog, recipe, cancellation);
        Dictionary<string, CardTemplate> cards = [];
        Dictionary<string, string> remap = [];
        bool complete = true;
        foreach (CardTemplate source in result.Cards.Values.OrderBy(card => card.Id, StringComparer.Ordinal))
        {
            cancellation.ThrowIfCancellationRequested();
            Outcome outcome = refinement.Refine(source, secondsPerCard, threads, cancellation);
            complete &= outcome.Complete;
            remap[source.Id] = outcome.Card.Id;
            cards[outcome.Card.Id] = outcome.Card;
            if (outcome.Card.Id != source.Id)
                Console.WriteLine($"[{recipe.Id}] 材料精化 {source.Id} → {outcome.Card.Id}："
                    + $"原始惩罚[{string.Join(',', source.RawPenalties)}] → [{string.Join(',', outcome.Card.RawPenalties)}]，"
                    + $"阶级[{string.Join(',', source.TierCounts)}] → [{string.Join(',', outcome.Card.TierCounts)}]，{outcome.Seconds:F2}秒。");
        }
        foreach (GroupState group in result.Groups.Values)
        {
            foreach (string goal in group.Best.Keys.ToArray())
                group.Best[goal] = remap[group.Best[goal]];
            group.TotalBest = group.TotalBest.Select(id => remap[id]).Distinct().ToArray();
        }
        result.Cards = cards;
        result.MaterialPolicy = Policy;
        result.Updated = DateTimeOffset.UtcNow;
        if (!complete)
            Console.WriteLine($"[{recipe.Id}] 材料精化已应用预算内验证结果，完整组合空间尚未闭合。");
        return remap;
    }

    /// <summary>并行精化完整配方检查点；每个配方在替换前保留一次原始快照。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="results">与资源配方一一对应的完整检查点。</param>
    /// <param name="secondsPerCard">每张卡的最大墙钟秒数。</param>
    /// <param name="threads">同时精化的配方数量。</param>
    /// <param name="cancellation">完整计算阶段的取消信号。</param>
    public static void RefineCheckpoints(Catalog catalog, RecipeResult[] results, double secondsPerCard, int threads,
        CancellationToken cancellation)
    {
        Parallel.ForEach(results.Where(result => result.MaterialPolicy != Policy), new ParallelOptions
        {
            MaxDegreeOfParallelism = threads,
            CancellationToken = cancellation
        }, result =>
        {
            Recipe recipe = catalog.Data.Recipes.Single(item => item.Id == result.Recipe);
            string path = Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json");
            string backup = Path.Combine(Storage.Root, ".codex", "trash", Policy, $"{recipe.Id:00}-{catalog.Data.Id}.json");
            if (!File.Exists(backup))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(backup)!);
                File.Copy(path, backup);
            }
            RefineResult(catalog, recipe, result, secondsPerCard, 1, cancellation);
            cancellation.ThrowIfCancellationRequested();
            Storage.Write(path, result);
        });
    }

    /// <summary>建立围绕原占用区域的任意规模修复邻域，完整优化阶段可关闭该限制。</summary>
    /// <param name="model">接收条件邻域约束的CP-SAT模型。</param>
    /// <param name="take">全部合法放置的选择变量。</param>
    /// <param name="source">定义原占用区域和激活目标的卡牌。</param>
    /// <returns>为真时启用邻域限制的控制变量。</returns>
    private BoolVar LimitRepairNeighborhood(CpModel model, BoolVar[] take, CardTemplate source)
    {
        HashSet<int> nearbyCells = source.Placements
            .SelectMany(placement => candidates[candidateIndex[(placement.Id, placement.Q, placement.R)]].Cells).ToHashSet();
        foreach (int index in nearbyCells.ToArray())
            foreach (Hex direction in Hex.Directions)
                if (cellIndex.TryGetValue(cells[index].Add(direction), out int neighbor))
                    nearbyCells.Add(neighbor);
        HashSet<int> activeTargets = source.Active.SelectMany(area => areaTargets[area]).ToHashSet();
        HashSet<(int Id, int Q, int R)> original = source.Placements
            .Select(placement => (placement.Id, placement.Q, placement.R)).ToHashSet();
        BoolVar enabled = model.NewBoolVar("repair_neighborhood");
        for (int index = 0; index < candidates.Length; index++)
        {
            Candidate candidate = candidates[index];
            if (!original.Contains((candidate.Placement.Id, candidate.Placement.Q, candidate.Placement.R))
                && !candidate.Cells.Any(nearbyCells.Contains) && !candidate.Matches.Any(activeTargets.Contains))
                model.Add(take[index] == 0).OnlyEnforceIf(enabled);
        }
        return enabled;
    }

    /// <summary>以完整布尔赋值提示求解器从当前布局附近寻找严格改善。</summary>
    /// <param name="model">接收提示的CP-SAT模型。</param>
    /// <param name="take">全部合法放置的选择变量。</param>
    /// <param name="card">作为搜索中心的完整卡牌布局。</param>
    private void ApplyHints(CpModel model, BoolVar[] take, CardTemplate card)
    {
        HashSet<int> selected = [];
        foreach (Placement placement in card.Placements)
        {
            if (!candidateIndex.TryGetValue((placement.Id, placement.Q, placement.R), out int index))
                throw new InvalidDataException($"配方{recipe.Id}的提示放置不在完整合法域中：{placement.Id}/{placement.Q}/{placement.R}。");
            selected.Add(index);
        }
        model.ClearHints();
        for (int index = 0; index < take.Length; index++)
            model.AddHint(take[index], selected.Contains(index));
    }

    /// <summary>把与原卡完全相同的合法技能集合编码为材料载体表约束。</summary>
    /// <param name="model">接收材料约束的CP-SAT模型。</param>
    /// <param name="take">全部合法放置的选择变量。</param>
    /// <param name="source">定义技能集合能力的原始卡牌。</param>
    private void ConstrainAbilities(CpModel model, BoolVar[] take, CardTemplate source)
    {
        IntVar[] carriers = new IntVar[catalog.Data.Profiles.Length];
        for (int profile = 0; profile < carriers.Length; profile++)
        {
            IntVar count = model.NewIntVar(0, source.Count, $"profile{profile}_count");
            model.Add(count == LinearExpr.Sum(candidates.Select((candidate, index) => (candidate, index))
                .Where(pair => pair.candidate.Profile == profile).Select(pair => take[pair.index])));
            IntVar carrier = model.NewIntVar(0, 3, $"profile{profile}_carrier");
            model.AddMinEquality(carrier, [count, LinearExpr.Constant(3)]);
            carriers[profile] = carrier;
        }
        TableConstraint table = model.AddAllowedAssignments(carriers);
        foreach (long[] vector in EquivalentCarrierVectors(source))
            table.AddTuple(vector);
    }

    /// <summary>枚举所有与原卡合法技能集合完全相同的截断材料载体向量。</summary>
    /// <param name="source">提供技能槽数和原始材料能力的卡牌。</param>
    /// <returns>每项均按0至3截断的允许载体向量。</returns>
    private long[][] EquivalentCarrierVectors(CardTemplate source)
    {
        string key = AbilityKey(source);
        if (carrierVectors.TryGetValue(key, out long[][]? cached))
            return cached;

        HashSet<string> sourceSets = AbilitySets(source);
        List<long[]> result = [];
        int[] vector = new int[catalog.Data.Profiles.Length];
        Expand(0);
        if (result.Count == 0)
            throw new InvalidDataException($"配方{recipe.Id}无法建立材料能力等价类：{source.Id}。");
        return carrierVectors[key] = result.ToArray();

        void Expand(int profile)
        {
            if (profile < vector.Length)
            {
                for (int count = 0; count <= 3; count++)
                {
                    vector[profile] = count;
                    Expand(profile + 1);
                }
                return;
            }
            CardTemplate candidate = new()
            {
                Slots = source.Slots,
                Carriers = vector.ToArray(),
                AvailableTraits = catalog.Data.Profiles.Where(item => vector[item.Id] > 0)
                    .SelectMany(item => item.Options).SelectMany(option => option.Traits).Distinct().Order().ToArray()
            };
            if (sourceSets.SetEquals(AbilitySets(candidate)))
                result.Add(vector.Select(count => (long)count).ToArray());
        }
    }

    /// <summary>建立可变数量的真实连通块，并要求每个连通块至少接触一个安全格。</summary>
    /// <param name="model">接收连通约束的CP-SAT模型。</param>
    /// <param name="occupied">各合法棋盘格是否被占用。</param>
    /// <param name="maximum">净乖离不变时允许的最大连通块数。</param>
    /// <returns>占用格形成的连通块数量。</returns>
    private IntVar ModelComponents(CpModel model, BoolVar[] occupied, int maximum)
    {
        if (maximum < 1 || maximum > cells.Length)
            throw new InvalidDataException($"配方{recipe.Id}的连通块上限无效：{maximum}。");
        int capacity = cells.Length;
        BoolVar[] enabled = Enumerable.Range(0, maximum).Select(component => model.NewBoolVar($"component{component}_enabled")).ToArray();
        IntVar componentCount = model.NewIntVar(1, maximum, "component_count");
        model.Add(componentCount == LinearExpr.Sum(enabled));
        for (int component = 1; component < maximum; component++)
            model.Add(enabled[component - 1] >= enabled[component]);

        BoolVar[,] labels = new BoolVar[maximum, cells.Length];
        BoolVar[,] roots = new BoolVar[maximum, cells.Length];
        for (int cell = 0; cell < cells.Length; cell++)
        {
            BoolVar[] at = new BoolVar[maximum];
            for (int component = 0; component < maximum; component++)
            {
                BoolVar label = model.NewBoolVar($"component{component}_cell{cell}");
                BoolVar root = model.NewBoolVar($"component{component}_root{cell}");
                labels[component, cell] = label;
                roots[component, cell] = root;
                model.Add(label <= enabled[component]);
                model.Add(root <= label);
                if (!safe.Contains(cells[cell]))
                    model.Add(root == 0);
                at[component] = label;
            }
            model.Add(LinearExpr.Sum(at) == occupied[cell]);
        }

        HashSet<(int A, int B)> edges = [];
        for (int cell = 0; cell < cells.Length; cell++)
            foreach (Hex direction in Hex.Directions.Take(3))
                if (cellIndex.TryGetValue(cells[cell].Add(direction), out int neighbor))
                    edges.Add((Math.Min(cell, neighbor), Math.Max(cell, neighbor)));

        IntVar? previousRootPosition = null;
        for (int component = 0; component < maximum; component++)
        {
            BoolVar[] componentRoots = Enumerable.Range(0, cells.Length).Select(cell => roots[component, cell]).ToArray();
            model.Add(LinearExpr.Sum(componentRoots) == enabled[component]);
            IntVar rootPosition = model.NewIntVar(0, cells.Length, $"component{component}_root_position");
            model.Add(rootPosition == LinearExpr.WeightedSum(componentRoots,
                Enumerable.Range(0, cells.Length).Select(index => (long)index).ToArray())
                + cells.Length - cells.Length * enabled[component]);
            if (previousRootPosition is not null)
                model.Add(previousRootPosition < rootPosition).OnlyEnforceIf(enabled[component]);
            previousRootPosition = rootPosition;
            List<LinearExpr>[] incoming = cells.Select(_ => new List<LinearExpr>()).ToArray();
            List<LinearExpr>[] outgoing = cells.Select(_ => new List<LinearExpr>()).ToArray();
            foreach ((int a, int b) in edges)
            {
                model.Add(labels[component, a] == labels[component, b]).OnlyEnforceIf([occupied[a], occupied[b]]);
                IntVar ab = model.NewIntVar(0, capacity, $"component{component}_flow{a}_{b}");
                IntVar ba = model.NewIntVar(0, capacity, $"component{component}_flow{b}_{a}");
                model.Add(ab <= capacity * labels[component, a]);
                model.Add(ab <= capacity * labels[component, b]);
                model.Add(ba <= capacity * labels[component, a]);
                model.Add(ba <= capacity * labels[component, b]);
                outgoing[a].Add(ab);
                incoming[b].Add(ab);
                outgoing[b].Add(ba);
                incoming[a].Add(ba);
            }
            for (int cell = 0; cell < cells.Length; cell++)
            {
                IntVar supply = model.NewIntVar(0, capacity, $"component{component}_supply{cell}");
                model.Add(supply <= capacity * roots[component, cell]);
                model.Add(supply + LinearExpr.Sum(incoming[cell]) - LinearExpr.Sum(outgoing[cell]) == labels[component, cell]);
            }
        }
        return componentCount;
    }

    /// <summary>把原始数量和对应容忍转换为指定净惩罚的线性边界。</summary>
    private static void PreservePenalty(CpModel model, LinearExpr raw, int allowance, int penalty)
    {
        if (penalty == 0)
            model.Add(raw <= allowance);
        else
            model.Add(raw == allowance + penalty);
    }

    /// <summary>求当前激活区域为一种容忍效果提供的总增量。</summary>
    private int ActiveEffect(CardTemplate card, int kind) => card.Active.Sum(area => recipe.Areas[area].Effects
        .Where(effect => effect.Kind == kind).Sum(effect => effect.Arguments[0].Value));

    /// <summary>粒子数固定时，取得大于全部材料阶级组合的原始总惩罚权重。</summary>
    /// <param name="count">固定的粒子总数。</param>
    /// <returns>原始总惩罚每增加一点对应的目标成本。</returns>
    private static long MaterialScale(int count) => (long)count * (count + 1) + 1;

    /// <summary>以三阶优先、二阶其次编码材料阶级。</summary>
    /// <param name="card">待编码的完整卡牌结果。</param>
    /// <returns>整张卡的材料阶级成本。</returns>
    private static long TierCost(CardTemplate card) => (long)card.TierCounts[2] * (card.Count + 1) + card.TierCounts[1];

    /// <summary>依次编码原始总惩罚、三阶数和二阶数。</summary>
    /// <param name="card">待编码的完整卡牌结果。</param>
    /// <returns>可直接比较的字典序目标成本。</returns>
    private static long RefinementCost(CardTemplate card) => card.RawPenalties.Sum() * MaterialScale(card.Count) + TierCost(card);

    /// <summary>比较除原始惩罚、材料阶级、放置和可增加能力外的全部卡牌结果。</summary>
    /// <param name="source">必须保持行为的原始卡牌。</param>
    /// <param name="candidate">完整规则复算后的候选卡牌。</param>
    /// <returns>候选是否保持所有固定卡牌结果。</returns>
    private static bool SameCardResult(CardTemplate source, CardTemplate candidate) =>
        source.Recipe == candidate.Recipe && source.BaseId == candidate.BaseId && source.Name == candidate.Name
        && source.Power == candidate.Power && source.Fortitude == candidate.Fortitude
        && source.BasePower == candidate.BasePower && source.BaseFortitude == candidate.BaseFortitude
        && source.Slots == candidate.Slots && source.Left == candidate.Left && source.Right == candidate.Right
        && source.InnerStructure == candidate.InnerStructure && source.OuterStructure == candidate.OuterStructure
        && source.RetainedPercent == candidate.RetainedPercent && source.Count == candidate.Count
        && source.Strikes == candidate.Strikes && source.Falsehood == candidate.Falsehood && source.Valid == candidate.Valid
        && source.Colors.SequenceEqual(candidate.Colors) && source.Active.SequenceEqual(candidate.Active)
        && source.Penalties.SequenceEqual(candidate.Penalties) && source.Group.SequenceEqual(candidate.Group);

    /// <summary>取得材料能力缓存使用的完整结构键。</summary>
    /// <param name="card">提供槽数、可用技能和截断载体数量的卡牌。</param>
    /// <returns>仅由技能承载能力决定的稳定键。</returns>
    private static string AbilityKey(CardTemplate card) =>
        $"{card.Slots}:{string.Join(',', card.AvailableTraits)}:{string.Join(',', card.Carriers)}";

    /// <summary>取得一张卡能够同时装备的全部未排序技能集合。</summary>
    /// <param name="card">待枚举材料能力的卡牌。</param>
    /// <returns>包括空集和未满槽配置的合法集合。</returns>
    private HashSet<string> AbilitySets(CardTemplate card)
    {
        string key = AbilityKey(card);
        if (!abilitySets.TryGetValue(key, out HashSet<string>? sets))
            abilitySets[key] = sets = ConfidenceAnalysis.LegalSkillSets(card, catalog.Data.Profiles);
        return sets;
    }

    /// <summary>确保新旧布局可以同时装备的全部未排序技能集合完全相同。</summary>
    /// <param name="source">定义原始技能集合的卡牌。</param>
    /// <param name="candidate">待核验技能集合的候选卡牌。</param>
    /// <returns>两张卡的全部合法集合是否相同。</returns>
    private bool PreservesAbilities(CardTemplate source, CardTemplate candidate) =>
        AbilitySets(source).SetEquals(AbilitySets(candidate));
}
