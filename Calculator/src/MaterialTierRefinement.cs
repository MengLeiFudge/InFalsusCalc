using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>在卡牌结果、粒子数和原始乖离不变时，依次减少三阶与二阶粒子。</summary>
internal sealed class MaterialTierRefinement
{
    /// <summary>参与卡库指纹并标记检查点已完成材料阶级精化的策略版本。</summary>
    public const string Policy = "material-tiers-v1";
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

    /// <summary>一次精化的合法结果及本次是否闭合了更低阶搜索空间。</summary>
    /// <param name="Card">已由完整规则复算的卡牌；未改善时为原卡。</param>
    /// <param name="Complete">不存在更低阶合法布局，或已经找到全局最低阶布局。</param>
    /// <param name="Seconds">本次求解墙钟秒数。</param>
    internal sealed record Outcome(CardTemplate Card, bool Complete, double Seconds);

    /// <summary>一个奖励格及其要求的原生颜色。</summary>
    private readonly record struct Target(Hex Cell, int Color);

    /// <summary>一项完整合法放置及其求解模型索引。</summary>
    private sealed record Candidate(Placement Placement, int[] Cells, int[] Matches, bool Outside, int Tier);

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
                    occupied.Any(cell => !safe.Contains(cell)), shape.Tier));
            }
        }
        candidates = placements.OrderBy(item => item.Placement.Id).ThenBy(item => item.Placement.Q).ThenBy(item => item.Placement.R).ToArray();
        candidateIndex = candidates.Select((item, index) => (item, index))
            .ToDictionary(pair => (pair.item.Placement.Id, pair.item.Placement.Q, pair.item.Placement.R), pair => pair.index);
    }

    /// <summary>在给定墙钟预算内寻找严格更低阶的等价布局。</summary>
    /// <param name="source">完整复算且属于当前配方的基准卡。</param>
    /// <param name="seconds">本张卡的求解时间片；实际墙钟另含CP模型构建。</param>
    /// <param name="threads">CP-SAT可使用的原生线程数。</param>
    /// <param name="cancellation">用户或上层阶段取消信号。</param>
    /// <returns>最优性状态和预算内找到的最低阶合法结果。</returns>
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
        double heuristicSeconds = Math.Min(seconds, Math.Max(.25, seconds * .2));
        CardTemplate best = ImproveSingleReplacements(source, heuristicSeconds, timer, cancellation);
        int bestCost = TierCost(best);
        if (bestCost == 0)
            return new(best, true, timer.Elapsed.TotalSeconds);

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
        PreserveComponents(model, occupied, source.RawPenalties[3] + 1);

        long[] costs = candidates.Select(candidate => candidate.Tier switch
        {
            3 => (long)source.Count + 1,
            2 => 1L,
            _ => 0L
        }).ToArray();
        LinearExpr tierCost = LinearExpr.WeightedSum(take, costs);
        model.Add(tierCost < bestCost);
        model.Minimize(tierCost);
        foreach (Placement placement in best.Placements)
        {
            if (!candidateIndex.TryGetValue((placement.Id, placement.Q, placement.R), out int index))
                throw new InvalidDataException($"配方{recipe.Id}的基准放置不在完整合法域中：{placement.Id}/{placement.Q}/{placement.R}。");
            model.AddHint(take[index], 1);
        }

        while (timer.Elapsed.TotalSeconds < seconds)
        {
            cancellation.ThrowIfCancellationRequested();
            double remaining = seconds - timer.Elapsed.TotalSeconds;
            CpSolver solver = new()
            {
                StringParameters = FormattableString.Invariant(
                    $"max_time_in_seconds:{Math.Max(.001, remaining)} num_search_workers:{threads} random_seed:1 linearization_level:2 cp_model_probing_level:2")
            };
            CpSolverStatus status;
            using (cancellation.Register(solver.StopSearch))
                status = solver.Solve(model);
            cancellation.ThrowIfCancellationRequested();
            if (status == CpSolverStatus.ModelInvalid)
                throw new InvalidDataException(solver.ResponseStats());
            if (status == CpSolverStatus.Infeasible)
                return new(best, true, timer.Elapsed.TotalSeconds);
            if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible))
                return new(best, false, timer.Elapsed.TotalSeconds);

            int[] chosen = Enumerable.Range(0, take.Length).Where(index => solver.Value(take[index]) != 0).ToArray();
            CardTemplate candidate = Craft.Evaluate(catalog, recipe, chosen.Select(index => candidates[index].Placement));
            if (SameCardResult(source, candidate) && PreservesAbilities(source, candidate))
            {
                if (TierCost(candidate) >= TierCost(best))
                    throw new InvalidDataException($"配方{recipe.Id}的材料目标返回了非改善布局：{source.Id}。");
                best = candidate;
                if (status == CpSolverStatus.Optimal)
                    return new(best, true, timer.Elapsed.TotalSeconds);
                return new(best, false, timer.Elapsed.TotalSeconds);
            }
            // 粒子数固定，因此少选一颗已选粒子即可排除这一整组放置。
            model.Add(LinearExpr.Sum(chosen.Select(index => take[index])) <= source.Count - 1);
        }
        return new(best, false, timer.Elapsed.TotalSeconds);
    }

    /// <summary>先尝试单颗低阶粒子的直接替代，为完整组合模型提供已验证的更好上界。</summary>
    private CardTemplate ImproveSingleReplacements(CardTemplate source, double seconds, Stopwatch timer, CancellationToken cancellation)
    {
        CardTemplate best = source;
        HashSet<int> activeTargets = source.Active.SelectMany(area => areaTargets[area]).ToHashSet();
        bool improved;
        int scanned = 0;
        do
        {
            improved = false;
            CardTemplate next = best;
            int[] order = Enumerable.Range(0, best.Placements.Length)
                .OrderByDescending(index => catalog.Shapes[best.Placements[index].Id].Tier)
                .ThenByDescending(index => best.Placements[index].Id).ToArray();
            foreach (int piece in order)
            {
                Placement removed = best.Placements[piece];
                int removedTier = catalog.Shapes[removed.Id].Tier;
                if (removedTier == 1)
                    continue;
                HashSet<(int Id, int Q, int R)> retained = best.Placements.Where((_, index) => index != piece)
                    .Select(placement => (placement.Id, placement.Q, placement.R)).ToHashSet();
                HashSet<int> retainedMatches = retained.SelectMany(placement => candidates[candidateIndex[placement]].Matches).ToHashSet();
                Candidate removedCandidate = candidates[candidateIndex[(removed.Id, removed.Q, removed.R)]];
                int[] required = removedCandidate.Matches
                    .Where(target => activeTargets.Contains(target) && !retainedMatches.Contains(target)).ToArray();
                IEnumerable<Candidate> replacements = candidates.Where(candidate => candidate.Tier < removedTier
                    && required.All(candidate.Matches.Contains)).OrderBy(candidate => candidate.Tier)
                    .ThenByDescending(candidate => catalog.Shapes[candidate.Placement.Id].Color == catalog.Shapes[removed.Id].Color)
                    .ThenByDescending(candidate => candidate.Cells.Intersect(removedCandidate.Cells).Count())
                    .ThenBy(candidate => candidate.Placement.Id).ThenBy(candidate => candidate.Placement.Q).ThenBy(candidate => candidate.Placement.R);
                bool lowestTierFound = false;
                foreach (Candidate replacement in replacements)
                {
                    if ((scanned++ & 255) == 0)
                    {
                        cancellation.ThrowIfCancellationRequested();
                        if (timer.Elapsed.TotalSeconds >= seconds)
                            return next;
                    }
                    if (!retained.Add((replacement.Placement.Id, replacement.Placement.Q, replacement.Placement.R)))
                        continue;
                    Placement[] layout = best.Placements.Where((_, index) => index != piece).Append(replacement.Placement).ToArray();
                    CardTemplate candidate = Craft.Evaluate(catalog, recipe, layout);
                    retained.Remove((replacement.Placement.Id, replacement.Placement.Q, replacement.Placement.R));
                    if (TierCost(candidate) < TierCost(next) && SameCardResult(source, candidate) && PreservesAbilities(source, candidate))
                    {
                        next = candidate;
                        if (replacement.Tier == 1)
                        {
                            lowestTierFound = true;
                            break;
                        }
                    }
                }
                if (lowestTierFound)
                    break;
            }
            if (TierCost(next) < TierCost(best))
            {
                best = next;
                improved = true;
            }
        }
        while (improved && timer.Elapsed.TotalSeconds < seconds);
        return best;
    }

    /// <summary>将一个完整配方检查点的全部代表替换为低阶等价布局，并重映射组引用。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="recipe">检查点所属配方。</param>
    /// <param name="result">待原位更新的完整检查点。</param>
    /// <param name="secondsPerCard">每张含高阶材料卡的求解时间片。</param>
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
            Console.WriteLine($"[{recipe.Id}] 材料阶级精化已应用预算内验证结果，完整组合空间尚未闭合。");
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

    /// <summary>固定占用格的真实连通块数量，并要求每个连通块至少接触一个安全格。</summary>
    private void PreserveComponents(CpModel model, BoolVar[] occupied, int componentCount)
    {
        if (componentCount < 1 || componentCount > cells.Length)
            throw new InvalidDataException($"配方{recipe.Id}的原始乖离无法建立连通模型：{componentCount - 1}。");
        int capacity = cells.Length;
        BoolVar[,] labels = new BoolVar[componentCount, cells.Length];
        BoolVar[,] roots = new BoolVar[componentCount, cells.Length];
        for (int cell = 0; cell < cells.Length; cell++)
        {
            BoolVar[] at = new BoolVar[componentCount];
            for (int component = 0; component < componentCount; component++)
            {
                BoolVar label = model.NewBoolVar($"component{component}_cell{cell}");
                BoolVar root = model.NewBoolVar($"component{component}_root{cell}");
                labels[component, cell] = label;
                roots[component, cell] = root;
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
        for (int component = 0; component < componentCount; component++)
        {
            BoolVar[] componentRoots = Enumerable.Range(0, cells.Length).Select(cell => roots[component, cell]).ToArray();
            model.AddExactlyOne(componentRoots);
            IntVar rootPosition = model.NewIntVar(0, cells.Length - 1, $"component{component}_root_position");
            model.Add(rootPosition == LinearExpr.WeightedSum(componentRoots,
                Enumerable.Range(0, cells.Length).Select(index => (long)index).ToArray()));
            if (previousRootPosition is not null)
                model.Add(previousRootPosition < rootPosition);
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

    /// <summary>粒子数固定时，以大于全部二阶数量的权重表达三阶优先的字典序目标。</summary>
    private static int TierCost(CardTemplate card) => card.TierCounts[2] * (card.Count + 1) + card.TierCounts[1];

    /// <summary>比较除材料阶级、放置和可增加能力外的全部卡牌结果。</summary>
    private static bool SameCardResult(CardTemplate source, CardTemplate candidate) =>
        source.Recipe == candidate.Recipe && source.BaseId == candidate.BaseId && source.Name == candidate.Name
        && source.Power == candidate.Power && source.Fortitude == candidate.Fortitude
        && source.BasePower == candidate.BasePower && source.BaseFortitude == candidate.BaseFortitude
        && source.Slots == candidate.Slots && source.Left == candidate.Left && source.Right == candidate.Right
        && source.InnerStructure == candidate.InnerStructure && source.OuterStructure == candidate.OuterStructure
        && source.RetainedPercent == candidate.RetainedPercent && source.Count == candidate.Count
        && source.Strikes == candidate.Strikes && source.Falsehood == candidate.Falsehood && source.Valid == candidate.Valid
        && source.Colors.SequenceEqual(candidate.Colors) && source.Active.SequenceEqual(candidate.Active)
        && source.Penalties.SequenceEqual(candidate.Penalties) && source.Group.SequenceEqual(candidate.Group)
        && source.RawPenalties[0] == candidate.RawPenalties[0]
        && source.RawPenalties[3] == candidate.RawPenalties[3];

    /// <summary>确保新旧布局可以同时装备的全部未排序技能集合完全相同。</summary>
    private bool PreservesAbilities(CardTemplate source, CardTemplate candidate)
    {
        HashSet<string> Sets(CardTemplate card)
        {
            string key = $"{card.Slots}:{string.Join(',', card.AvailableTraits)}:{string.Join(',', card.Carriers)}";
            if (!abilitySets.TryGetValue(key, out HashSet<string>? sets))
                abilitySets[key] = sets = ConfidenceAnalysis.LegalSkillSets(card, catalog.Data.Profiles);
            return sets;
        }
        return Sets(source).SetEquals(Sets(candidate));
    }
}
