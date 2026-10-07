using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>在卡牌结果与粒子数不变时，依次减少原始总惩罚、三阶与二阶粒子。</summary>
internal sealed class MaterialTierRefinement
{
    /// <summary>参与卡库指纹并标记检查点已完成材料精化的策略版本。</summary>
    public const string Policy = "material-tiers-v3";
    /// <summary>正常生成管线给每张卡的求解时间片参考值，单位秒。</summary>
    public const double DefaultSecondsPerCard = 30;
    /// <summary>
    /// 候选放置的最远格到安全区的六边距离上限。经验剪枝：material-tiers-v1报告847张模板的13117颗粒子中只有1颗超过2；
    /// 超出安全区的粒子本身计入原始超界，远离安全区还需更多超界粒子连通，不利于降低原始总惩罚。
    /// 原布局的全部放置始终保留，实际上限取本值与原布局最远距离的较大者，保证基准可行。
    /// </summary>
    public const int DistanceLimit = 2;
    /// <summary>空间窗口的六边半径，按从小到大依次尝试；最大半径已覆盖常见卡牌的大部分占用区域。</summary>
    private static readonly int[] WindowRadii = [2, 4, 7];
    /// <summary>单个窗口子问题的墙钟上限，单位秒；小窗口通常在远低于该值时证明或找到改善。</summary>
    private const double WindowSeconds = 4;

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
    /// <param name="Complete">全部窗口均已证明不存在严格改善，即局部收敛；不代表完整合法域内的全局最低。</param>
    /// <param name="Seconds">本次求解墙钟秒数。</param>
    internal sealed record Outcome(CardTemplate Card, bool Complete, double Seconds);

    /// <summary>一个奖励格及其要求的原生颜色。</summary>
    private readonly record struct Target(Hex Cell, int Color);

    /// <summary>一项完整合法放置及其求解模型索引。</summary>
    /// <param name="Placement">形状编号与平移坐标。</param>
    /// <param name="Cells">占用的配方格索引，已去重升序。</param>
    /// <param name="Matches">同色覆盖的奖励目标索引。</param>
    /// <param name="Outside">是否有格位于安全区外，计入原始超界。</param>
    /// <param name="Tier">材料阶级1至3。</param>
    /// <param name="Profile">材料能力组编号。</param>
    /// <param name="Distance">放置最远格到最近安全格的六边步数；全部在安全区时为0。</param>
    private sealed record Candidate(Placement Placement, int[] Cells, int[] Matches, bool Outside, int Tier, int Profile, int Distance);

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

        // 从全部安全格出发做广度优先搜索，得到每个棋盘格到安全区的步数；与安全区不连通的格保持int.MaxValue。
        int[] distance = Enumerable.Repeat(int.MaxValue, cells.Length).ToArray();
        Queue<int> frontier = new();
        for (int cell = 0; cell < cells.Length; cell++)
            if (safe.Contains(cells[cell]))
            {
                distance[cell] = 0;
                frontier.Enqueue(cell);
            }
        while (frontier.TryDequeue(out int cell))
            foreach (Hex direction in Hex.Directions)
                if (cellIndex.TryGetValue(cells[cell].Add(direction), out int next) && distance[next] == int.MaxValue)
                {
                    distance[next] = distance[cell] + 1;
                    frontier.Enqueue(next);
                }

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
                    occupied.Any(cell => !safe.Contains(cell)), shape.Tier, shape.Profile,
                    occupiedIndices.Max(index => distance[index])));
            }
        }
        candidates = placements.OrderBy(item => item.Placement.Id).ThenBy(item => item.Placement.Q).ThenBy(item => item.Placement.R).ToArray();
        candidateIndex = candidates.Select((item, index) => (item, index))
            .ToDictionary(pair => (pair.item.Placement.Id, pair.item.Placement.Q, pair.item.Placement.R), pair => pair.index);
    }

    /// <summary>
    /// 按原始总惩罚、三阶数、二阶数寻找严格改善的等价布局。
    /// 采用空间窗口局部搜索：每次释放一个六边窗口内的全部现有粒子（数量不限），用小型CP-SAT在窗口内重排；
    /// 窗口半径逐级扩大，任一窗口改善后从最小半径重新开始，全部窗口均证明无改善时收敛。
    /// </summary>
    /// <param name="source">完整复算且属于当前配方的基准卡。</param>
    /// <param name="seconds">本张卡的墙钟预算。</param>
    /// <param name="threads">CP-SAT可使用的原生线程数。</param>
    /// <param name="cancellation">用户或上层阶段取消信号。</param>
    /// <returns>预算内找到的最低目标值合法结果及是否已收敛。</returns>
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
        // 安全剪枝：材料类型不出现在任何等价载体向量中的放置必然改变合法技能集合。
        // 经验剪枝：离安全区超过DistanceLimit的放置不进入候选，原布局始终保持可行。
        long[][] vectors = EquivalentCarrierVectors(source);
        bool[] usefulProfile = new bool[catalog.Data.Profiles.Length];
        foreach (long[] vector in vectors)
            for (int profile = 0; profile < vector.Length; profile++)
                usefulProfile[profile] |= vector[profile] > 0;
        int reach = Math.Max(DistanceLimit, source.Placements.Max(placement => candidates[Locate(placement)].Distance));
        int[] allowed = Enumerable.Range(0, candidates.Length)
            .Where(index => candidates[index].Distance <= reach && usefulProfile[candidates[index].Profile]).ToArray();

        CardTemplate current = source;
        while (true)
        {
            int[] layout = current.Placements.Select(Locate).ToArray();
            CardTemplate? better = null;
            bool undecided = false;
            foreach (int radius in WindowRadii)
            {
                HashSet<string> tried = [];
                foreach (int center in layout.Select(index => candidates[index].Cells[0]).Distinct())
                {
                    double remaining = seconds - timer.Elapsed.TotalSeconds;
                    if (remaining <= 0)
                        return new(current, false, timer.Elapsed.TotalSeconds);
                    bool Inside(int cell) => Distance(cells[cell], cells[center]) <= radius;
                    int[] freed = layout.Where(index => candidates[index].Cells.Any(Inside)).ToArray();
                    if (!tried.Add(string.Join(',', freed.Order())))
                        continue;
                    int[] domain = allowed.Where(index => candidates[index].Cells.All(Inside)).Union(layout).ToArray();
                    (better, bool decided) = SolveWindow(source, current, domain, layout.Except(freed).ToHashSet(), vectors,
                        Math.Min(WindowSeconds, remaining), threads, cancellation);
                    undecided |= !decided;
                    if (better is not null)
                        break;
                }
                if (better is not null)
                    break;
            }
            if (better is null)
                return new(current, !undecided, timer.Elapsed.TotalSeconds);
            current = better;
        }
    }

    /// <summary>取得一项布局放置在完整候选表中的索引。</summary>
    /// <param name="placement">已属于当前配方合法外框的放置。</param>
    /// <returns>candidates中的索引。</returns>
    private int Locate(Placement placement) => candidateIndex.TryGetValue((placement.Id, placement.Q, placement.R), out int index)
        ? index : throw new InvalidDataException($"配方{recipe.Id}的放置不在合法域中：{placement.Id}/{placement.Q}/{placement.R}。");

    /// <summary>轴向坐标下两格之间的六边步数。</summary>
    private static int Distance(Hex a, Hex b)
    {
        int dq = a.Q - b.Q, dr = a.R - b.R;
        return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(dq + dr)) / 2;
    }

    /// <summary>
    /// 在一个窗口内求解严格改善：fixedSet中的放置保持选中，domain中的其余放置可自由增删，全局约束与完整模型相同。
    /// </summary>
    /// <param name="source">定义面板、激活区域、净惩罚和能力等价类的原始卡牌。</param>
    /// <param name="current">当前最优布局，作为提示与改善基准。</param>
    /// <param name="domain">窗口内可选放置与当前布局放置的候选索引并集。</param>
    /// <param name="fixedSet">窗口外必须保留的当前放置。</param>
    /// <param name="vectors">与原卡能力等价的截断载体向量。</param>
    /// <param name="seconds">本窗口的墙钟预算。</param>
    /// <param name="threads">CP-SAT原生线程数。</param>
    /// <param name="cancellation">用户或上层阶段取消信号。</param>
    /// <returns>严格改善的完整复算布局，或空引用；Decided表示求解器已证明本窗口的结论。</returns>
    private (CardTemplate? Better, bool Decided) SolveWindow(CardTemplate source, CardTemplate current, int[] domain,
        HashSet<int> fixedSet, long[][] vectors, double seconds, int threads, CancellationToken cancellation)
    {
        long currentCost = RefinementCost(current);
        HashSet<int> selected = current.Placements.Select(Locate).ToHashSet();
        // local把配方格索引映射为模型格索引；模型只为候选实际可覆盖的格建立占用、重叠和连通变量。
        int[] usable = domain.SelectMany(index => candidates[index].Cells).Distinct().Order().ToArray();
        Dictionary<int, int> local = usable.Select((cell, index) => (cell, index)).ToDictionary(pair => pair.cell, pair => pair.index);

        CpModel model = new();
        BoolVar[] take = domain.Select(index => model.NewBoolVar($"p{index}")).ToArray();
        List<BoolVar>[] cover = usable.Select(_ => new List<BoolVar>()).ToArray();
        List<BoolVar>[] match = targets.Select(_ => new List<BoolVar>()).ToArray();
        for (int k = 0; k < domain.Length; k++)
        {
            if (fixedSet.Contains(domain[k]))
                model.Add(take[k] == 1);
            foreach (int cell in candidates[domain[k]].Cells)
                cover[local[cell]].Add(take[k]);
            foreach (int target in candidates[domain[k]].Matches)
                match[target].Add(take[k]);
        }
        model.Add(LinearExpr.Sum(take) == source.Count);
        foreach (List<BoolVar> at in cover)
            model.Add(LinearExpr.Sum(at) <= Craft.MaxStack);

        HashSet<int> active = source.Active.ToHashSet();
        for (int area = 0; area < areaTargets.Length; area++)
        {
            // 窗口内没有任何候选覆盖的目标恒为未填充，未激活区域因此自然保持未激活。
            BoolVar?[] filled = areaTargets[area].Select(target =>
            {
                if (match[target].Count == 0)
                    return null;
                BoolVar value = model.NewBoolVar($"filled{target}");
                model.AddMaxEquality(value, match[target]);
                return value;
            }).ToArray();
            if (active.Contains(area))
            {
                if (filled.Any(literal => literal is null))
                    return (null, true);
                foreach (BoolVar? literal in filled)
                    model.Add(literal! == 1);
            }
            else if (filled.All(literal => literal is not null))
                model.AddBoolOr(filled.Select(literal => literal!.Not()));
        }

        int outsideAllowance = Craft.Skills[recipe.Character].Outside + ActiveEffect(source, 9);
        LinearExpr outside = LinearExpr.Sum(Enumerable.Range(0, domain.Length)
            .Where(k => candidates[domain[k]].Outside).Select(k => take[k]));
        PreservePenalty(model, outside, outsideAllowance, source.Penalties[1]);

        BoolVar[] overlap = new BoolVar[usable.Length];
        BoolVar[] occupied = new BoolVar[usable.Length];
        for (int cell = 0; cell < usable.Length; cell++)
        {
            BoolVar used = model.NewBoolVar($"occupied{cell}");
            model.AddMaxEquality(used, cover[cell]);
            occupied[cell] = used;
            BoolVar over = model.NewBoolVar($"overlap{cell}");
            model.Add(LinearExpr.Sum(cover[cell]) >= 2).OnlyEnforceIf(over);
            model.Add(LinearExpr.Sum(cover[cell]) <= 1).OnlyEnforceIf(over.Not());
            overlap[cell] = over;
        }
        int overlapAllowance = Craft.Skills[recipe.Character].Overlap + ActiveEffect(source, 10);
        PreservePenalty(model, LinearExpr.Sum(overlap), overlapAllowance, source.Penalties[2]);

        // 净乖离为0时只需连通块数的上界：根数不少于真实块数，最小化会把根数压到真实块数，因此目标最优值不变。
        // 净乖离为正时必须精确保持块数，只能使用带标签的多商品流模型。
        int splitAllowance = Craft.Skills[recipe.Character].Split + ActiveEffect(source, 11);
        (int A, int B)[] edges = Edges(usable, local);
        LinearExpr componentCount = source.Penalties[3] == 0
            ? ModelRootedFlow(model, occupied, usable, edges, splitAllowance + 1)
            : ModelComponents(model, occupied, usable, edges, splitAllowance + source.Penalties[3] + 1);
        LinearExpr split = componentCount - 1;
        PreservePenalty(model, split, splitAllowance, source.Penalties[3]);
        ConstrainAbilities(model, take, domain, vectors, source.Count);

        long[] tierCosts = domain.Select(index => candidates[index].Tier switch
        {
            3 => (long)source.Count + 1,
            2 => 1L,
            _ => 0L
        }).ToArray();
        LinearExpr rawTotal = outside + LinearExpr.Sum(overlap) + split + source.RawPenalties[0];
        LinearExpr refinementCost = rawTotal * MaterialScale(source.Count) + LinearExpr.WeightedSum(take, tierCosts);
        model.Add(refinementCost < currentCost);
        model.Minimize(refinementCost);
        for (int k = 0; k < domain.Length; k++)
            model.AddHint(take[k], selected.Contains(domain[k]));

        CpSolver solver = new()
        {
            StringParameters = FormattableString.Invariant(
                $"max_time_in_seconds:{Math.Max(.001, seconds)} num_search_workers:{threads} random_seed:1")
        };
        CpSolverStatus status;
        using (cancellation.Register(solver.StopSearch))
            status = solver.Solve(model);
        cancellation.ThrowIfCancellationRequested();
        if (status == CpSolverStatus.ModelInvalid)
            throw new InvalidDataException($"配方{recipe.Id}的材料窗口模型无效：{source.Id}。{solver.ResponseStats()}");
        if (status == CpSolverStatus.Infeasible)
            return (null, true);
        if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible))
            return (null, false);

        CardTemplate candidate = Craft.Evaluate(catalog, recipe, Enumerable.Range(0, domain.Length)
            .Where(k => solver.Value(take[k]) != 0).Select(k => candidates[domain[k]].Placement));
        if (!SameCardResult(source, candidate) || !PreservesAbilities(source, candidate) || RefinementCost(candidate) >= currentCost)
            throw new InvalidDataException($"配方{recipe.Id}的材料窗口返回了非等价或非改善布局：{source.Id}。");
        return (candidate, true);
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

    /// <summary>把与原卡完全相同的合法技能集合编码为材料载体表约束。</summary>
    /// <param name="model">接收材料约束的CP-SAT模型。</param>
    /// <param name="take">剪枝域内放置的选择变量，与domain逐项对应。</param>
    /// <param name="domain">take对应的候选放置索引。</param>
    /// <param name="vectors">与原卡能力等价的全部截断载体向量。</param>
    /// <param name="count">固定的粒子总数。</param>
    private void ConstrainAbilities(CpModel model, BoolVar[] take, int[] domain, long[][] vectors, int count)
    {
        IntVar[] carriers = new IntVar[catalog.Data.Profiles.Length];
        for (int profile = 0; profile < carriers.Length; profile++)
        {
            IntVar total = model.NewIntVar(0, count, $"profile{profile}_count");
            model.Add(total == LinearExpr.Sum(Enumerable.Range(0, domain.Length)
                .Where(k => candidates[domain[k]].Profile == profile).Select(k => take[k])));
            IntVar carrier = model.NewIntVar(0, 3, $"profile{profile}_carrier");
            model.AddMinEquality(carrier, [total, LinearExpr.Constant(3)]);
            carriers[profile] = carrier;
        }
        TableConstraint table = model.AddAllowedAssignments(carriers);
        foreach (long[] vector in vectors)
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

    /// <summary>列出模型格之间共享边的无向邻接，每条边只出现一次。</summary>
    /// <param name="usable">模型格对应的配方格索引。</param>
    /// <param name="local">配方格索引到模型格索引的映射。</param>
    /// <returns>按模型格索引表示的无向边。</returns>
    private (int A, int B)[] Edges(int[] usable, Dictionary<int, int> local)
    {
        List<(int A, int B)> edges = [];
        for (int cell = 0; cell < usable.Length; cell++)
            foreach (Hex direction in Hex.Directions.Take(3))
                if (cellIndex.TryGetValue(cells[usable[cell]].Add(direction), out int neighbor) && local.TryGetValue(neighbor, out int other))
                    edges.Add((Math.Min(cell, other), Math.Max(cell, other)));
        return edges.ToArray();
    }

    /// <summary>
    /// 用单商品流约束占用格：每个占用格从某个安全区根格获得一单位流量，因此每个连通块都接触安全格。
    /// 返回的根数是真实连通块数的上界；目标最小化乖离时根数会收缩到真实块数。
    /// </summary>
    /// <param name="model">接收连通约束的CP-SAT模型。</param>
    /// <param name="occupied">各模型格是否被占用。</param>
    /// <param name="usable">模型格对应的配方格索引。</param>
    /// <param name="edges">模型格之间的无向边。</param>
    /// <param name="maximum">净乖离为0时允许的最大连通块数。</param>
    /// <returns>根格数量。</returns>
    private LinearExpr ModelRootedFlow(CpModel model, BoolVar[] occupied, int[] usable, (int A, int B)[] edges, int maximum)
    {
        int capacity = usable.Length;
        List<BoolVar> roots = [];
        List<LinearExpr>[] balance = usable.Select(_ => new List<LinearExpr>()).ToArray();
        for (int cell = 0; cell < usable.Length; cell++)
        {
            if (!safe.Contains(cells[usable[cell]]))
                continue;
            BoolVar root = model.NewBoolVar($"root{cell}");
            model.Add(root <= occupied[cell]);
            IntVar supply = model.NewIntVar(0, capacity, $"supply{cell}");
            model.Add(supply <= capacity * root);
            balance[cell].Add(supply);
            roots.Add(root);
        }
        foreach ((int a, int b) in edges)
        {
            IntVar ab = model.NewIntVar(0, capacity, $"flow{a}_{b}");
            IntVar ba = model.NewIntVar(0, capacity, $"flow{b}_{a}");
            foreach (IntVar flow in new[] { ab, ba })
            {
                model.Add(flow <= capacity * occupied[a]);
                model.Add(flow <= capacity * occupied[b]);
            }
            balance[b].Add(ab);
            balance[b].Add(-ba);
            balance[a].Add(ba);
            balance[a].Add(-ab);
        }
        for (int cell = 0; cell < usable.Length; cell++)
            model.Add(LinearExpr.Sum(balance[cell]) == occupied[cell]);
        LinearExpr count = LinearExpr.Sum(roots);
        model.Add(count >= 1);
        model.Add(count <= maximum);
        return count;
    }

    /// <summary>建立精确数量的真实连通块，并要求每个连通块至少接触一个安全格；只用于净乖离为正的卡。</summary>
    /// <param name="model">接收连通约束的CP-SAT模型。</param>
    /// <param name="occupied">各模型格是否被占用。</param>
    /// <param name="usable">模型格对应的配方格索引。</param>
    /// <param name="edges">模型格之间的无向边。</param>
    /// <param name="maximum">允许的最大连通块数。</param>
    /// <returns>占用格形成的连通块数量。</returns>
    private IntVar ModelComponents(CpModel model, BoolVar[] occupied, int[] usable, (int A, int B)[] edges, int maximum)
    {
        int size = usable.Length;
        if (maximum < 1 || maximum > size)
            throw new InvalidDataException($"配方{recipe.Id}的连通块上限无效：{maximum}。");
        BoolVar[] enabled = Enumerable.Range(0, maximum).Select(component => model.NewBoolVar($"component{component}_enabled")).ToArray();
        IntVar componentCount = model.NewIntVar(1, maximum, "component_count");
        model.Add(componentCount == LinearExpr.Sum(enabled));
        for (int component = 1; component < maximum; component++)
            model.Add(enabled[component - 1] >= enabled[component]);

        BoolVar[,] labels = new BoolVar[maximum, size];
        BoolVar[,] roots = new BoolVar[maximum, size];
        for (int cell = 0; cell < size; cell++)
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
                if (!safe.Contains(cells[usable[cell]]))
                    model.Add(root == 0);
                at[component] = label;
            }
            model.Add(LinearExpr.Sum(at) == occupied[cell]);
        }

        IntVar? previousRootPosition = null;
        for (int component = 0; component < maximum; component++)
        {
            BoolVar[] componentRoots = Enumerable.Range(0, size).Select(cell => roots[component, cell]).ToArray();
            model.Add(LinearExpr.Sum(componentRoots) == enabled[component]);
            // 以根格序号给连通块排序，消除标签对称。
            IntVar rootPosition = model.NewIntVar(0, size, $"component{component}_root_position");
            model.Add(rootPosition == LinearExpr.WeightedSum(componentRoots,
                Enumerable.Range(0, size).Select(index => (long)index).ToArray()) + size - size * enabled[component]);
            if (previousRootPosition is not null)
                model.Add(previousRootPosition < rootPosition).OnlyEnforceIf(enabled[component]);
            previousRootPosition = rootPosition;
            List<LinearExpr>[] balance = Enumerable.Range(0, size).Select(_ => new List<LinearExpr>()).ToArray();
            foreach ((int a, int b) in edges)
            {
                model.Add(labels[component, a] == labels[component, b]).OnlyEnforceIf([occupied[a], occupied[b]]);
                IntVar ab = model.NewIntVar(0, size, $"component{component}_flow{a}_{b}");
                IntVar ba = model.NewIntVar(0, size, $"component{component}_flow{b}_{a}");
                foreach (IntVar flow in new[] { ab, ba })
                {
                    model.Add(flow <= size * labels[component, a]);
                    model.Add(flow <= size * labels[component, b]);
                }
                balance[b].Add(ab);
                balance[b].Add(-ba);
                balance[a].Add(ba);
                balance[a].Add(-ab);
            }
            for (int cell = 0; cell < size; cell++)
            {
                IntVar supply = model.NewIntVar(0, size, $"component{component}_supply{cell}");
                model.Add(supply <= size * roots[component, cell]);
                balance[cell].Add(supply);
                model.Add(LinearExpr.Sum(balance[cell]) == labels[component, cell]);
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
