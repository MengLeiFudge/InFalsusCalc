using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>固定槽位与左右范围的区域分支搜索；同一激活集合只求一次粒子可行性。</summary>
internal sealed partial class KeyRecipeSolver
{
    /// <summary>当前资源中的材料、技能与效果定义。</summary>
    private readonly Catalog catalog;
    /// <summary>目标卡的棋盘、奖励区域与角色。</summary>
    private readonly Recipe recipe;
    /// <summary>批量调度器按尚未结束的卡动态分配原生线程。</summary>
    private readonly Func<int> threadBudget;
    /// <summary>本次原生调用可获得的线程数，运行中的调用保持原分配。</summary>
    private int threads => threadBudget();
    /// <summary>用户停止和整个命令预算共用的取消信号。</summary>
    private readonly CancellationToken cancellation;
    /// <summary>从几何预处理开始计时，不扣除建模与保存成本。</summary>
    private readonly Stopwatch elapsed = Stopwatch.StartNew();
    /// <summary>坐标到紧凑格子编号的映射。</summary>
    private readonly Dictionary<Hex, int> cellIndex;
    /// <summary>按格子编号排列的合法外框坐标。</summary>
    private readonly Hex[] cells;
    /// <summary>完全探索时可作为合法连通根的安全格。</summary>
    private readonly HashSet<Hex> safe;
    /// <summary>真实放置域；仅合并面板等价放置并删去越界预算下不可达的装饰放置。</summary>
    private readonly List<Piece> pieces = [];
    /// <summary>区域位掩码与精确惩罚层对应的合法布局或已证不可行空值。</summary>
    private readonly Dictionary<(uint Mask, int Strikes), CardTemplate?> solved = [];
    /// <summary>各区域的多种候选拼法，可在组合时复用共享粒子；候选缺失不代表不可行。</summary>
    private readonly Dictionary<int, List<int[]>> localPatterns = [];
    /// <summary>把真实布局中的粒子坐标还原为预编译放置编号。</summary>
    private readonly Dictionary<(int Id, int Q, int R), int> placementIndex;
    /// <summary>每个区域必须使用的粒子数安全下界。</summary>
    private readonly int[] regionMinimum;
    /// <summary>完全不使用越界粒子覆盖本区域时的粒子数下界。</summary>
    private readonly int[] regionSafeMinimum;
    /// <summary>单颗粒子最多能匹配每个区域的格子数。</summary>
    private readonly int[] regionMaxCover;
    /// <summary>区域奖励，16和17列分别存放左右范围贡献。</summary>
    private readonly int[,] effects;
    /// <summary>每个激活集合仅覆盖奖励所需粒子数的安全下界。</summary>
    private byte[] coverageLower = [];
    /// <summary>能被同一真实粒子匹配的区域关系位集。</summary>
    private readonly uint[] shared;
    /// <summary>小共享区域组在不同越界额度下的覆盖粒子数下界。</summary>
    private readonly List<(uint Mask, Dictionary<uint, int[]> Costs)> clusterCosts = [];
    /// <summary>包含全部正向区域奖励时的越界容忍上界。</summary>
    private readonly int outsideBudget;
    /// <summary>每个格子相邻的安全区连通分量，用于越界额度的连接下界。</summary>
    private readonly ulong[] safeContacts;
    /// <summary>每个区域实际落入的安全区连通分量。</summary>
    private readonly ulong[] regionSafeParts;
    /// <summary>单颗越界粒子最多能合并的安全区分量数。</summary>
    private readonly int maxSafeMerge;
    /// <summary>是否不存在跨区域匹配粒子，决定可否累加局部粒子数。</summary>
    private readonly bool independent;
    /// <summary>角色和配方共同允许的基础粒子数量。</summary>
    private readonly int limit;
    /// <summary>每次原生几何求解的时间片，单位秒。</summary>
    private readonly double slice;
    /// <summary>实际进入几何判断的收益层次数。</summary>
    private long checks;

    /// <summary>新算法与旧内外极值检查点分离，防止误恢复未优化的结果。</summary>
    public const string Policy = "key-regions-v2";
    /// <summary>是否处于取消完整模型时间限制的重试阶段。</summary>
    private bool retry;
    /// <summary>供状态线程读取的当前求解阶段。</summary>
    private volatile string progress = "几何预编译";
    /// <summary>当前目标要求的精确净惩罚次数。</summary>
    private int currentStrikes;
    /// <summary>当前目标的可保存状态，停止时补齐真实用时。</summary>
    private KeyGoalState? runningGoal;
    /// <summary>当前目标的墙钟计时。</summary>
    private Stopwatch? goalClock;
    /// <summary>当前key与目标，作为阶段进度的前缀。</summary>
    private string target = "";
    /// <summary>当前不可变进度文本，可由监控线程安全读取。</summary>
    public string Progress => progress;
    /// <summary>当前目标已经运行的墙钟秒数。</summary>
    public double GoalSeconds => Volatile.Read(ref goalClock)?.Elapsed.TotalSeconds ?? 0;
    /// <summary>首轮各子步骤共享目标预算；重试没有目标截止时间。</summary>
    private double SearchBudget => retry ? double.PositiveInfinity : Math.Max(0, slice - GoalSeconds);
    /// <summary>当前配方全部已计算key及其三个面板代表。</summary>
    public RecipeResult Result { get; }

    /// <summary>预编译配方几何，并复核恢复兼容检查点中的布局。</summary>
    /// <param name="catalog">当前资源快照。</param>
    /// <param name="recipe">目标配方。</param>
    /// <param name="threadBudget">共享总预算中的当前线程份额。</param>
    /// <param name="slice">单次几何搜索时间片，秒。</param>
    /// <param name="cancellation">整个任务的取消信号。</param>
    public KeyRecipeSolver(Catalog catalog, Recipe recipe, Func<int> threadBudget, double slice, CancellationToken cancellation)
    {
        this.catalog = catalog; this.recipe = recipe; this.threadBudget = threadBudget; this.slice = slice; this.cancellation = cancellation;
        cells = recipe.Board.Select(c => c.Position).Distinct().ToArray();
        cellIndex = cells.Select((c, i) => (c, i)).ToDictionary(p => p.c, p => p.i);
        safe = recipe.Safe.Select(c => c.Position).ToHashSet();
        if (recipe.Areas.SelectMany(a => a.Cells).Any(c => !safe.Contains(c.Position)) || catalog.Data.Shapes.Any(s => Craft.Components(s.Cells).Count != 1))
            throw new InvalidDataException("区域锚点压缩要求奖励格位于安全区且粒子形状连通，当前资源不满足。");
        limit = Math.Min(Craft.Skills[recipe.Character].Count, recipe.MaxIota);
        excluded = Excluded(recipe);
        Result = Load(catalog, recipe) ?? new RecipeResult { Recipe = recipe.Id, Snapshot = catalog.Data.Id, Policy = RecipePolicy(recipe),
            SelectionScope = ConfidenceAnalysis.Scope,
            ExcludedAreas = Enumerable.Range(0, recipe.Areas.Length).Where(i => (excluded & (1u << i)) != 0).ToArray() };
        foreach (uint mask in Result.InfeasibleRegions) solved[(mask, 0)] = null;
        foreach (CardTemplate stored in Result.Cards.Values)
        {
            CardTemplate card = Craft.Evaluate(catalog, recipe, stored.Placements);
            if (!card.Valid || card.Strikes is < 0 or > 2 || card.Power != stored.Power || card.Fortitude != stored.Fortitude || !card.Active.Order().SequenceEqual(stored.Active.Order()))
                throw new InvalidDataException($"配方{recipe.Id}的续算布局复核失败：{stored.Id}。");
            uint mask = card.Active.Aggregate(0u, (m, i) => m | (1u << i));
            var solvedKey = (mask, card.Strikes);
            if (solved.TryGetValue(solvedKey, out CardTemplate? existing) && existing is null) throw new InvalidDataException($"配方{recipe.Id}的布局与不可行区域证明冲突：{mask}/p{card.Strikes}。");
            solved[solvedKey] = card;
        }
        effects = new int[recipe.Areas.Length, 18];
        Dictionary<(Hex, int), int> target = [];
        int bit = 0;
        foreach (var (area, index) in recipe.Areas.Select((a, i) => (a, i)))
        {
            foreach (BoardCell c in area.Cells) target[(c.Position, c.Color)] = bit++;
            foreach (RegionEffect e in area.Effects)
            {
                if (e.Kind == 8) { effects[index, 16] += e.Arguments[0].Value; effects[index, 17] += e.Arguments[1].Value; }
                else if (e.Kind != 1) effects[index, e.Kind] += e.Arguments[0].Value;
            }
        }
        int[] bitRegion = recipe.Areas.SelectMany((a, i) => a.Cells.Select(_ => i)).ToArray();
        regionMaxCover = new int[recipe.Areas.Length];
        HashSet<string> seen = [];
        foreach (Shape shape in catalog.Data.Shapes)
        {
            // 任选一个形状格作为锚点，遍历外框格即可得到全部合法平移。
            Hex origin = shape.Cells[0];
            foreach (Hex anchor in cells)
            {
                cancellation.ThrowIfCancellationRequested();
                Hex at = new(anchor.Q - origin.Q, anchor.R - origin.R);
                Hex[] occupied = shape.Cells.Select(c => c.Add(at)).ToArray();
                if (occupied.Any(c => !cellIndex.ContainsKey(c))) continue;
                int[] matches = occupied.Where(c => target.ContainsKey((c, shape.Color))).Select(c => target[(c, shape.Color)]).Order().ToArray();
                int[] ids = occupied.Select(c => cellIndex[c]).Order().ToArray();
                // 相同占用和奖励覆盖对当前三个面板目标完全等价；保留一种真实材料。
                string identity = string.Join(',', ids) + "/" + string.Join(',', matches);
                if (!seen.Add(identity)) continue;
                uint regions = 0;
                foreach (var group in matches.GroupBy(b => bitRegion[b]))
                { regions |= 1u << group.Key; regionMaxCover[group.Key] = Math.Max(regionMaxCover[group.Key], group.Count()); }
                pieces.Add(new Piece(new Placement { Id = shape.Id, Q = at.Q, R = at.R }, ids, matches, regions,
                    occupied.Any(c => !safe.Contains(c))));
            }
        }
        independent = pieces.All(p => System.Numerics.BitOperations.PopCount(p.Regions) <= 1);
        shared = new uint[recipe.Areas.Length];
        foreach (Piece p in pieces)
            for (int i = 0; i < shared.Length; i++)
                if ((p.Regions & (1u << i)) != 0) shared[i] |= p.Regions & ~(1u << i);
        regionMinimum = recipe.Areas.Select((a, i) => regionMaxCover[i] == 0 ? limit + 1 : (a.Cells.Length + regionMaxCover[i] - 1) / regionMaxCover[i]).ToArray();
        int offset = 0;
        for (int i = 0; i < regionMinimum.Length; i++)
        {
            regionMinimum[i] = CoverMinimum(i, offset, regionMinimum[i]); offset += recipe.Areas[i].Cells.Length;
        }
        outsideBudget = Craft.Skills[recipe.Character].Outside + ConfidenceAnalysis.RetainedStrikes.Max() + Enumerable.Range(0, recipe.Areas.Length)
            .Where(i => (excluded & (1u << i)) == 0).Sum(i => Math.Max(0, effects[i, 9]));
        HashSet<int> reachable = safe.Select(c => cellIndex[c]).ToHashSet();
        HashSet<int> border = [];
        for (int step = 0; step < outsideBudget; step++)
        {
            border = reachable.SelectMany(c => Hex.Directions.Select(cells[c].Add).Append(cells[c])).Where(cellIndex.ContainsKey).Select(c => cellIndex[c]).ToHashSet();
            int[] extension = pieces.Where(p => p.Outside && p.Cells.Any(border.Contains)).SelectMany(p => p.Cells).ToArray();
            reachable.UnionWith(extension);
        }
        pieces.RemoveAll(p => p.Outside && (outsideBudget == 0 || !p.Cells.Any(border.Contains)));
        for (int region = 0; region < recipe.Areas.Length; region++)
            if (recipe.Areas[region].Cells.Length > 1) regionMinimum[region] = Math.Max(regionMinimum[region], GeometryMinimum(region));
        regionSafeMinimum = (int[])regionMinimum.Clone();
        if (outsideBudget > 0)
            for (int region = 0; region < recipe.Areas.Length; region++)
                if ((excluded & (1u << region)) == 0 && recipe.Areas[region].Cells.Length > 1)
                    regionSafeMinimum[region] = Math.Max(regionMinimum[region], GeometryMinimum(region, 0));
        if (independent)
            for (int region = 0; region < recipe.Areas.Length; region++)
            {
                if (recipe.Areas[region].Cells.Length == 1) continue;
                int[] costs = Enumerable.Range(0, outsideBudget + 1).Select(b => b == outsideBudget ? regionMinimum[region] : b == 0 ? regionSafeMinimum[region] : Math.Max(regionMinimum[region], GeometryMinimum(region, b))).ToArray();
                if (costs.Any(c => c != regionMinimum[region]))
                {
                    uint mask = 1u << region;
                    clusterCosts.Add((mask, new Dictionary<uint, int[]> { [0] = new int[outsideBudget + 1], [mask] = costs }));
                }
            }
        if (!independent)
        {
            uint pending = (1u << recipe.Areas.Length) - 1;
            while (pending != 0)
            {
                uint cluster = pending & (uint)-(int)pending;
                uint prior;
                do
                {
                    prior = cluster;
                    for (int i = 0; i < shared.Length; i++) if ((cluster & (1u << i)) != 0) cluster |= shared[i];
                } while (cluster != prior);
                pending &= ~cluster;
                int[] areas = Enumerable.Range(0, shared.Length).Where(i => (cluster & (1u << i)) != 0).ToArray();
                if (areas.Sum(i => recipe.Areas[i].Cells.Length) > 63 || areas.Length > 10) continue;
                Dictionary<uint, int[]> costs = new() { [0] = new int[outsideBudget + 1] };
                for (uint subset = cluster; subset != 0; subset = (subset - 1) & cluster)
                {
                    int[] bits = bitRegion.Select((r, b) => (r, b)).Where(p => (subset & (1u << p.r)) != 0).Select(p => p.b).ToArray();
                    Dictionary<int, int> remap = bits.Select((b, i) => (b, i)).ToDictionary(p => p.b, p => p.i);
                    var patterns = pieces.Where(p => (p.Regions & subset) != 0).Select(p => (Mask: p.Matches.Where(remap.ContainsKey)
                        .Aggregate(0UL, (mask, b) => mask | (1UL << remap[b])), p.Outside)).Distinct().ToArray();
                    costs[subset] = Enumerable.Range(0, outsideBudget + 1).Select(budget => MinimumCover(patterns, bits.Length, budget)).ToArray();
                }
                clusterCosts.Add((cluster, costs));
            }
        }
        List<HashSet<Hex>> safeParts = Craft.Components(safe);
        if (safeParts.Count > 64) throw new InvalidDataException("安全区连通分量超过当前位集容量。");
        safeContacts = new ulong[cells.Length]; regionSafeParts = new ulong[recipe.Areas.Length];
        for (int part = 0; part < safeParts.Count; part++)
        {
            foreach (Hex c in safeParts[part].SelectMany(c => Hex.Directions.Select(c.Add).Append(c)))
                if (cellIndex.TryGetValue(c, out int index)) safeContacts[index] |= 1UL << part;
            for (int i = 0; i < recipe.Areas.Length; i++)
                if (recipe.Areas[i].Cells.Any(c => safeParts[part].Contains(c.Position))) regionSafeParts[i] |= 1UL << part;
        }
        maxSafeMerge = pieces.Where(p => p.Outside).Select(p => Math.Max(0, System.Numerics.BitOperations.PopCount(p.Cells.Aggregate(0UL, (mask, c) => mask | safeContacts[c])) - 1)).DefaultIfEmpty(0).Max();
        placementIndex = pieces.Select((p, i) => (p, i)).ToDictionary(x => (x.p.Placement.Id, x.p.Placement.Q, x.p.Placement.R), x => x.i);
        Console.WriteLine($"[{recipe.Id}] 几何预编译：{pieces.Count}个放置，区域共享={(independent ? "无" : "有")}，{elapsed.Elapsed.TotalSeconds:F3}秒。");
    }

    /// <summary>按指定key计算三个面板代表；未指定时枚举资源能产生的全部key。</summary>
    /// <param name="requested">可选的槽位、左范围、右范围。</param>
    /// <param name="goal">可选power、fortitude、total；为空时全部求解。</param>
    /// <param name="retry">false只处理未尝试目标；true不限时重试已有未决目标。</param>
    public void Run(int[]? requested, string? goal, bool retry)
    {
        this.retry = retry;
        Dictionary<string, List<RegionSet>> baseGroups = [];
        List<RegionSet>?[] indexedGroups = new List<RegionSet>?[100];
        foreach (int[] k in ConfidenceAnalysis.RetainedKeys(recipe))
            if (requested is null || requested.SequenceEqual(k))
                baseGroups[string.Join(',', k)] = indexedGroups[k[0] * 25 + k[1] * 5 + k[2]] = [];
        byte[] lower = coverageLower = new byte[1 << recipe.Areas.Length];
        for (uint mask = 1; mask < lower.Length; mask++)
        {
            int i = System.Numerics.BitOperations.TrailingZeroCount(mask);
            uint rest = mask & (mask - 1);
            int bound = Math.Max(lower[rest], Math.Min(255, regionMinimum[i] + lower[rest & ~shared[i]]));
            if (clusterCosts.Count > 0)
            {
                uint clustered = clusterCosts.Aggregate(0u, (m, c) => m | c.Mask);
                int constant = independent ? Enumerable.Range(0, regionMinimum.Length).Where(i => (mask & ~clustered & (1u << i)) != 0).Sum(i => regionMinimum[i]) : 0;
                int[] costs = Enumerable.Repeat(constant, outsideBudget + 1).ToArray();
                foreach (var cluster in clusterCosts)
                {
                    int[] local = cluster.Costs[mask & cluster.Mask];
                    costs = Enumerable.Range(0, outsideBudget + 1).Select(b => Enumerable.Range(0, b + 1).Min(n => costs[b - n] + local[n])).ToArray();
                }
                bound = Math.Max(bound, costs[outsideBudget]);
            }
            lower[mask] = (byte)Math.Min(limit + 1, bound);
        }
        // 最大22区；只保存通过安全粒子数下界及精确key的区域集合。
        for (uint mask = 1; mask < (1u << recipe.Areas.Length); mask++)
        {
            if ((mask & 4095) == 0) cancellation.ThrowIfCancellationRequested();
            if ((mask & excluded) != 0) continue;
            int slots = 0, left = 0, right = 0, power = 0, fortitude = 0, extra = 0;
            for (int i = 0; i < recipe.Areas.Length; i++)
            {
                if ((mask & (1u << i)) == 0) continue;
                slots += effects[i, 7]; left += effects[i, 16]; right += effects[i, 17];
                power += effects[i, 3]; fortitude += effects[i, 4]; extra += effects[i, 15];
            }
            int cost = lower[mask];
            if (cost + extra > limit + ConfidenceAnalysis.RetainedStrikes.Max() || power <= 0 || fortitude <= 0) continue;
            slots = Math.Min(3, slots); left = Math.Min(4, left); right = Math.Min(4, right);
            if (slots == 0) left = right = 0;
            if (requested is not null && (slots != requested[0] || left != requested[1] || right != requested[2])) continue;
            List<RegionSet>? group = indexedGroups[slots * 25 + left * 5 + right];
            if (group is null) continue;
            group.Add(new RegionSet(mask, power, fortitude, cost));
        }
        if (requested is not null && baseGroups.Count == 0)
            baseGroups[string.Join(',', requested)] = [];
        var groups = baseGroups.SelectMany(pair => ConfidenceAnalysis.RetainedStrikes.Select(strikes => new
        {
            Id = GroupKey(pair.Key, strikes),
            Key = pair.Key,
            Strikes = strikes,
            Sets = pair.Value
        })).ToArray();
        string[] requestedGoals = goal is null ? ["power", "fortitude", "total"] : [goal];
        foreach (var entry in groups
            .OrderBy(g => requestedGoals.Any(n => !Done(Result, g.Id, n) && !Attempted(Result, g.Id, n)) ? 0
                : requestedGoals.Any(n => !Done(Result, g.Id, n)) ? 1 : 2)
            .ThenBy(g => g.Strikes)
            .ThenByDescending(g => ConfidenceAnalysis.KeyConfidence(recipe, g.Key))
            .ThenBy(g => g.Key))
        {
            string key = entry.Key, groupId = entry.Id;
            List<RegionSet> sets = entry.Sets;
            currentStrikes = entry.Strikes;
            cancellation.ThrowIfCancellationRequested();
            int[] tuple = key.Split(',').Select(int.Parse).ToArray();
            if (!Result.Groups.TryGetValue(groupId, out GroupState? group)) Result.Groups[groupId] = group = new GroupState { Key = tuple, Strikes = currentStrikes };
            else if (group.Strikes != currentStrikes) throw new InvalidDataException($"配方{recipe.Id}/{groupId}惩罚层不一致。");
            foreach (string name in requestedGoals.OrderBy(n => Attempted(Result, groupId, n) ? 1 : 0).ToArray())
            {
                if (Done(Result, groupId, name)) continue;
                if (retry != Attempted(Result, groupId, name)) continue;
                if (group.Goals.Values.Any(s => s.Status == "INFEASIBLE"))
                {
                    group.Goals[name] = new KeyGoalState { Status = "INFEASIBLE" };
                    Console.WriteLine($"[{recipe.Id}] key={key} p{currentStrikes} {name} 复用同key不可行证明。");
                    Save(); continue;
                }
                Stopwatch watch = Stopwatch.StartNew(); long before = checks;
                goalClock = watch; target = $"key={key} p{currentStrikes} {name}"; progress = target + " 区域收益排序";
                int g = name == "power" ? 0 : name == "fortitude" ? 1 : 2;
                // 基础数值排序与统一999效能下的单项排序一致；总和按原生取整后的值排序。
                int? savedUpper = group.Goals.GetValueOrDefault(name)?.UpperBound;
                RegionSet[] ordered = sets.Where(s => savedUpper is null || Value(s, g, currentStrikes) <= savedUpper.Value).OrderByDescending(s => Value(s, g, currentStrikes)).ThenBy(s => s.Cost).ThenBy(s => s.Mask).ToArray();
                bool unresolved = false; int upper = 0;
                runningGoal = new KeyGoalState { Status = "UNKNOWN", SearchPolicy = Result.Policy, UpperBound = ordered.Length == 0 ? 0 : Value(ordered[0], g, currentStrikes) };
                group.Goals[name] = runningGoal;
                Save();
                if (Done(Result, groupId, name)) { runningGoal = null; progress = target + " 已复用证明"; continue; }
                progress = target + " 构造合法候选";
                CardTemplate? initial = ordered.Select(s => solved.GetValueOrDefault((s.Mask, currentStrikes))).FirstOrDefault(c => c is not null);
                if (initial is null && recipe.Areas.Any(a => a.Cells.Length > 1))
                    initial = FindLayout(ordered.OrderBy(s => s.Cost).Take(64).ToArray(), .35);
                if (initial is not null)
                {
                    solved[(initial.Active.Aggregate(0u, (mask, i) => mask | (1u << i)), currentStrikes)] = initial;
                    Result.Cards[initial.Id] = initial; group.Best[name] = initial.Id;
                    Save();
                }
                if (Done(Result, groupId, name)) { runningGoal = null; progress = target + " 已复用证明"; continue; }
                foreach (var batch in ordered.GroupBy(s => Value(s, g, currentStrikes)))
                {
                    if (SearchBudget <= 0) { unresolved = true; upper = runningGoal.UpperBound; break; }
                    cancellation.ThrowIfCancellationRequested();
                    if (!unresolved) runningGoal.UpperBound = upper = batch.Key;
                    RegionSet[] alternatives = batch.ToArray();
                    CardTemplate? card = alternatives.Select(s => solved.GetValueOrDefault((s.Mask, currentStrikes))).FirstOrDefault(c => c is not null);
                    if (card is null)
                    {
                        alternatives = alternatives.Where(s => !solved.ContainsKey((s.Mask, currentStrikes))).ToArray();
                        if (alternatives.Length == 0) continue;
                        if (unresolved)
                        {
                            card = recipe.Areas.Any(a => a.Cells.Length > 1) ? FindPatterns(alternatives, .15) : null;
                            card ??= FindLayout(alternatives, .15);
                            if (card is null) continue;
                        }
                        else
                        {
                            var answer = Feasible(alternatives, tuple);
                            if (answer.Status == CpSolverStatus.Unknown) { unresolved = true; continue; }
                            card = answer.Card;
                        }
                        if (card is not null) solved[(card.Active.Aggregate(0u, (mask, i) => mask | (1u << i)), currentStrikes)] = card;
                        else foreach (RegionSet impossible in alternatives) solved[(impossible.Mask, currentStrikes)] = null;
                    }
                    if (card is null) continue;
                    Result.Cards[card.Id] = card; group.Best[name] = card.Id;
                    if (recipe.Areas.Any(a => a.Cells.Length > 1))
                    {
                        int[] indices = card.Placements.Select(p => placementIndex[(p.Id, p.Q, p.R)]).ToArray();
                        foreach (int region in card.Active)
                        {
                            int[] pattern = indices.Where(i => (pieces[i].Regions & (1u << region)) != 0).Order().ToArray();
                            List<int[]> patterns = Patterns(region);
                            if (!patterns.Any(p => p.SequenceEqual(pattern))) patterns.Insert(0, pattern);
                        }
                    }
                    break;
                }
                string status = unresolved ? group.Best.ContainsKey(name) ? "CONFIDENCE" : "UNKNOWN" : group.Best.ContainsKey(name) ? "OPTIMAL" : "INFEASIBLE";
                group.Goals[name] = new KeyGoalState { SearchPolicy = Result.Policy, Status = status,
                    UpperBound = !unresolved && !group.Best.ContainsKey(name) ? 0 : upper, Seconds = watch.Elapsed.TotalSeconds, GeometryQueries = checks - before };
                runningGoal = null;
                progress = target + " " + group.Goals[name].Status;
                string panel = group.Best.TryGetValue(name, out string? id) ? $"基础{Result.Cards[id].BasePower}/{Result.Cards[id].BaseFortitude}" : "无布局";
                Console.WriteLine($"[{recipe.Id}] key={key} p{currentStrikes} {name} {group.Goals[name].Status} {panel}，{watch.Elapsed.TotalSeconds:F3}秒，几何查询{checks - before}。");
                Save();
            }
            group.Complete = group.Goals.Count == 3 && group.Goals.Values.All(c => c.Status != "UNKNOWN");
            group.Infeasible = group.Complete && group.Best.Count == 0;
        }
        Result.Complete = Complete(Result, recipe);
        Result.BoundedFinalized = Result.Complete; Result.SolveSeconds = elapsed.Elapsed.TotalSeconds; Save();
    }

    /// <summary>单区域考虑重叠和越界后的粒子数下界；有限时间只采用安全目标界。</summary>
    /// <param name="region">奖励区域编号。</param>
    /// <param name="budget">可用越界粒子数，空值使用角色与区域奖励上界。</param>
    /// <returns>未超过真实最少用量的安全下界。</returns>
    private int GeometryMinimum(int region, int? budget = null)
    {
        Piece[] options = pieces.Where(p => (p.Regions & (1u << region)) != 0).ToArray();
        CpModel model = new(); BoolVar[] take = options.Select((_, i) => model.NewBoolVar($"local{i}")).ToArray();
        int offset = recipe.Areas.Take(region).Sum(a => a.Cells.Length);
        foreach (int bit in Enumerable.Range(offset, recipe.Areas[region].Cells.Length))
            model.Add(LinearExpr.Sum(take.Where((_, i) => options[i].Matches.Contains(bit))) >= 1);
        model.Add(LinearExpr.Sum(take) <= limit);
        model.Add(LinearExpr.Sum(take.Where((_, i) => options[i].Outside)) <= (budget ?? outsideBudget));
        List<BoolVar> repeated = [];
        foreach (var group in options.SelectMany((p, i) => p.Cells.Select(c => (c, i))).GroupBy(p => p.c))
        {
            if (group.Count() < 2) continue;
            BoolVar over = model.NewBoolVar($"repeat{group.Key}");
            model.Add(LinearExpr.Sum(group.Select(p => take[p.i])) <= 1).OnlyEnforceIf(over.Not()); repeated.Add(over);
        }
        model.Add(LinearExpr.Sum(repeated) <= Craft.Skills[recipe.Character].Overlap + Enumerable.Range(0, recipe.Areas.Length).Sum(i => Math.Max(0, effects[i, 10])));
        model.Minimize(LinearExpr.Sum(take));
        CpSolver solver = new() { StringParameters = "max_time_in_seconds:0.3 num_search_workers:1 cp_model_probing_level:0" };
        using CancellationTokenRegistration registration = cancellation.Register(solver.StopSearch);
        CpSolverStatus status = solver.Solve(model); cancellation.ThrowIfCancellationRequested();
        if (status == CpSolverStatus.Infeasible) return limit + 1;
        return status == CpSolverStatus.Optimal ? (int)Math.Round(solver.ObjectiveValue) : Math.Max(0, (int)Math.Floor(solver.BestObjectiveBound));
    }

    /// <summary>忽略外部冲突求单区域覆盖下界；节点预算耗尽只保留已证明的下界。</summary>
    /// <param name="region">区域编号。</param>
    /// <param name="offset">该区域在奖励格索引中的起点。</param>
    /// <param name="lower">格数推导出的初始下界。</param>
    /// <returns>覆盖搜索已证明的粒子数下界。</returns>
    private int CoverMinimum(int region, int offset, int lower)
    {
        int count = recipe.Areas[region].Cells.Length;
        ulong full = (1UL << count) - 1;
        ulong[] covers = pieces.Where(p => (p.Regions & (1u << region)) != 0)
            .Select(p => p.Matches.Where(b => b >= offset && b < offset + count).Aggregate(0UL, (mask, b) => mask | (1UL << (b - offset))))
            .Distinct().OrderByDescending(System.Numerics.BitOperations.PopCount).ToArray();
        return MinimumCover(covers, count, lower);
    }

    /// <summary>覆盖模式的有界精确下界；共享粒子在一个模式中只计一次。</summary>
    /// <param name="covers">每种放置覆盖的局部奖励格位集。</param>
    /// <param name="count">需要覆盖的格子数。</param>
    /// <param name="lower">开始检查的粒子数量。</param>
    /// <returns>未超出真实最小覆盖数的下界。</returns>
    private int MinimumCover(ulong[] covers, int count, int lower)
    {
        ulong full = (1UL << count) - 1;
        covers = covers.OrderByDescending(System.Numerics.BitOperations.PopCount).ToArray();
        covers = covers.Where((mask, i) => !covers.Take(i).Any(other => (mask & other) == mask)).ToArray();
        int maxCover = covers.Length == 0 ? 0 : covers.Max(System.Numerics.BitOperations.PopCount);
        long nodes = 0; bool exhausted = false;
        bool Search(ulong remaining, int depth, Dictionary<ulong, int> failed)
        {
            if (remaining == 0) return true;
            if (++nodes > 100000) { exhausted = true; return false; }
            if (depth == 0 || System.Numerics.BitOperations.PopCount(remaining) > depth * maxCover) return false;
            if (failed.TryGetValue(remaining, out int old) && old >= depth) return false;
            ulong[]? next = null;
            for (int bit = 0; bit < count; bit++)
                if ((remaining & (1UL << bit)) != 0)
                {
                    ulong[] candidates = covers.Where(c => (c & (1UL << bit)) != 0).ToArray();
                    if (next is null || candidates.Length < next.Length) next = candidates;
                }
            foreach (ulong cover in next!.OrderByDescending(c => System.Numerics.BitOperations.PopCount(c & remaining)))
                if (Search(remaining & ~cover, depth - 1, failed)) return true;
                else if (exhausted) return false;
            failed[remaining] = depth; return false;
        }
        for (int depth = lower; depth <= limit; depth++)
        {
            if (Search(full, depth, [])) return depth;
            if (exhausted) return depth;
        }
        return limit + 1;
    }

    /// <summary>共享区域在有限越界额度下的覆盖下界；每个越界粒子只扣一次额度。</summary>
    /// <param name="patterns">覆盖位集及该粒子是否越界。</param>
    /// <param name="count">联合区域所需的奖励格数。</param>
    /// <param name="budget">可分配给这些区域的越界额度。</param>
    /// <returns>预算约束下已经证明的粒子数下界。</returns>
    private int MinimumCover((ulong Mask, bool Outside)[] patterns, int count, int budget)
    {
        patterns = patterns.Where(p => !p.Outside || budget > 0).OrderByDescending(p => System.Numerics.BitOperations.PopCount(p.Mask)).ThenBy(p => p.Outside).ToArray();
        patterns = patterns.Where((p, i) => !patterns.Take(i).Any(q => (p.Mask & q.Mask) == p.Mask && (!q.Outside || p.Outside))).ToArray();
        ulong full = (1UL << count) - 1;
        int maximum = patterns.Length == 0 ? 0 : patterns.Max(p => System.Numerics.BitOperations.PopCount(p.Mask));
        int visits = 0; bool exhausted = false;
        bool Search(ulong remaining, int depth, int outside, Dictionary<(ulong, int), int> failed)
        {
            if (remaining == 0) return true;
            if (++visits > 100000) { exhausted = true; return false; }
            if (depth == 0 || System.Numerics.BitOperations.PopCount(remaining) > depth * maximum) return false;
            if (failed.TryGetValue((remaining, outside), out int old) && old >= depth) return false;
            (ulong Mask, bool Outside)[]? options = null;
            for (int bit = 0; bit < count; bit++)
                if ((remaining & (1UL << bit)) != 0)
                {
                    var found = patterns.Where(p => (p.Mask & (1UL << bit)) != 0 && (!p.Outside || outside > 0)).ToArray();
                    if (found.Length == 0) return false;
                    if (options is null || found.Length < options.Length) options = found;
                }
            foreach (var p in options!.OrderByDescending(p => System.Numerics.BitOperations.PopCount(p.Mask & remaining)))
            {
                if (Search(remaining & ~p.Mask, depth - 1, outside - (p.Outside ? 1 : 0), failed)) return true;
                if (exhausted) return false;
            }
            failed[(remaining, outside)] = depth; return false;
        }
        for (int depth = maximum == 0 ? limit + 1 : (count + maximum - 1) / maximum; depth <= limit; depth++)
        {
            if (Search(full, depth, budget, [])) return depth;
            if (exhausted) return depth;
        }
        return limit + 1;
    }

    /// <summary>将已完成和未决状态写入新策略目录，不改旧成果。</summary>
    public void Save()
    {
        if (runningGoal is not null && goalClock is not null) runningGoal.Seconds = goalClock.Elapsed.TotalSeconds;
        ReuseBounds(Result);
        Result.InfeasibleRegions = solved.Where(p => p.Key.Strikes == 0 && p.Value is null).Select(p => p.Key.Mask).Order().ToArray();
        Result.Updated = DateTimeOffset.UtcNow;
        string path = Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json");
        RecipeResult? previous = Storage.Read<RecipeResult>(path);
        if (previous is null || previous.Policy != Result.Policy || previous.Snapshot != catalog.Data.Id)
        { Storage.Write(path, Result); return; }
        previous.InfeasibleRegions = previous.InfeasibleRegions.Union(Result.InfeasibleRegions).Order().ToArray();
        foreach (var pair in Result.Cards) previous.Cards[pair.Key] = pair.Value;
        foreach (var pair in Result.Groups)
        {
            if (!previous.Groups.TryGetValue(pair.Key, out GroupState? group)) previous.Groups[pair.Key] = group = new GroupState { Key = pair.Value.Key, Strikes = pair.Value.Strikes };
            foreach (var goal in pair.Value.Goals)
            {
                KeyGoalState? oldGoal = group.Goals.GetValueOrDefault(goal.Key);
                if (oldGoal?.Status == "OPTIMAL" && goal.Value.Status != "OPTIMAL" || oldGoal?.Status == "CONFIDENCE" && goal.Value.Status == "UNKNOWN") continue;
                group.Goals[goal.Key] = goal.Value;
                if (pair.Value.Best.TryGetValue(goal.Key, out string? card))
                {
                    CardTemplate next = previous.Cards[card];
                    CardTemplate? old = group.Best.TryGetValue(goal.Key, out string? oldId) ? previous.Cards.GetValueOrDefault(oldId) : null;
                    int nextValue = goal.Key == "power" ? next.Power : goal.Key == "fortitude" ? next.Fortitude : next.Total;
                    int oldValue = old is null ? -1 : goal.Key == "power" ? old.Power : goal.Key == "fortitude" ? old.Fortitude : old.Total;
                    if (nextValue >= oldValue) group.Best[goal.Key] = card;
                }
                else if (goal.Value.Status == "INFEASIBLE") group.Best.Remove(goal.Key);
            }
            group.Complete = group.Goals.Count == 3 && group.Goals.Values.All(g => g.Status != "UNKNOWN");
            group.Infeasible = group.Complete && group.Best.Count == 0;
        }
        previous.Updated = Result.Updated; previous.SolveSeconds = Result.SolveSeconds;
        previous.Complete = Complete(previous, recipe);
        previous.BoundedFinalized = previous.Complete;
        HashSet<string> used = previous.Groups.Values.SelectMany(g => g.Best.Values).ToHashSet();
        previous.Cards = previous.Cards.Where(p => used.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value);
        Dictionary<string, string> representatives = [];
        foreach (var cards in previous.Cards.Values.GroupBy(c => $"{c.Strikes}:{c.Power}:{c.Fortitude}:" + string.Join(',', c.Active.Order())))
        {
            string id = cards.First().Id;
            foreach (CardTemplate card in cards) representatives[card.Id] = id;
        }
        foreach (GroupState group in previous.Groups.Values)
            foreach (string name in group.Best.Keys.ToArray()) group.Best[name] = representatives[group.Best[name]];
        previous.Cards = representatives.Values.Distinct().ToDictionary(id => id, id => previous.Cards[id]);
        Storage.Write(path, previous);
    }

    /// <summary>区域收益按真实效能取整，保证总和目标与最终卡牌一致。</summary>
    /// <param name="set">区域选择对应的基础数值。</param>
    /// <param name="goal">0攻击、1防御、2攻防和。</param>
    /// <param name="strikes">要求的精确净惩罚次数。</param>
    /// <returns>999效能下该惩罚层的目标面板值。</returns>
    private static int Value(RegionSet set, int goal, int strikes) => goal == 0 ? Craft.FinalStat(set.Power, strikes) : goal == 1 ? Craft.FinalStat(set.Fortitude, strikes)
        : Craft.FinalStat(set.Power, strikes) + Craft.FinalStat(set.Fortitude, strikes);

    /// <summary>先用奖励附近的紧域找合法解，只有完整域明确不可行才排除区域集合。</summary>
    /// <param name="sets">同一key与面板值的替代区域集合。</param>
    /// <param name="key">精确的槽数和左右范围。</param>
    /// <returns>合法布局，或明确不可行、未决状态。</returns>
    private (CardTemplate? Card, CpSolverStatus Status) Feasible(RegionSet[] sets, int[] key)
    {
        if (SearchBudget <= 0) return (null, CpSolverStatus.Unknown);
        checks++;
        Console.WriteLine($"[{recipe.Id}] 收益层：{sets.Length}组区域，粒子数下界{sets.Min(s => s.Cost)}/{limit}。");
        CardTemplate? seed = recipe.Areas.Any(a => a.Cells.Length > 1) ? FindPatterns(sets, Math.Min(2, SearchBudget)) : null;
        seed ??= FindLayout(sets, Math.Min(2, SearchBudget));
        if (seed is not null) return (seed, CpSolverStatus.Feasible);
        var cover = Geometry(sets, key, GeometryMode.Cover);
        if (cover.Card is not null || cover.Status == CpSolverStatus.Infeasible) return cover;
        if (recipe.Areas.Any(a => a.Cells.Length > 1) && sets.Length > 1)
            foreach (RegionSet candidate in sets.OrderBy(s => s.Cost).Take(8))
            {
                if (SearchBudget <= 0) return (null, CpSolverStatus.Unknown);
                var direct = currentStrikes == 0 && independent ? PatternGraph(candidate, Math.Min(5, Math.Min(slice, SearchBudget))) : Geometry([candidate], key, GeometryMode.Compact, .75);
                if (direct.Card is not null) return direct;
            }
        var small = Geometry(sets, key, GeometryMode.Compact);
        if (small.Card is not null) return small;
        return Geometry(sets, key, GeometryMode.Full);
    }

    /// <summary>生成有限的区域完整拼法作为构造候选；不据候选库缺失作不可行证明。</summary>
    /// <param name="region">无跨区域匹配的目标区域编号。</param>
    /// <returns>粒子放置编号数组构成的候选拼法库。</returns>
    private List<int[]> Patterns(int region)
    {
        if (localPatterns.TryGetValue(region, out var cached)) return cached;
        int offset = recipe.Areas.Take(region).Sum(a => a.Cells.Length), size = recipe.Areas[region].Cells.Length;
        uint full = (1u << size) - 1;
        int[] indices = Enumerable.Range(0, pieces.Count).Where(i => (pieces[i].Regions & (1u << region)) != 0).ToArray();
        Dictionary<int, uint> masks = indices.ToDictionary(i => i, i => pieces[i].Matches.Where(b => b >= offset && b < offset + size).Aggregate(0u, (mask, b) => mask | (1u << (b - offset))));
        int[][] choices = Enumerable.Range(0, size).Select(b => indices.Where(i => (masks[i] & (1u << b)) != 0)
            .OrderBy(i => pieces[i].Outside).ThenByDescending(i => System.Numerics.BitOperations.PopCount(masks[i])).ToArray()).ToArray();
        List<int[]> result = []; HashSet<string> seen = [];
        int[] occupied = new int[cells.Length]; List<int> selected = []; int outside = 0, overlaps = 0;
        Stopwatch clock = Stopwatch.StartNew(); int outsideTarget = 0, quota = 0; double deadline = 0;
        bool Fits(int i) => (!pieces[i].Outside || outside < outsideTarget) && overlaps + pieces[i].Cells.Count(c => occupied[c] == 1) <= Craft.Skills[recipe.Character].Overlap;
        void Search(uint covered, int depth)
        {
            if (clock.Elapsed.TotalSeconds > deadline || result.Count >= quota || SearchBudget <= 0 || cancellation.IsCancellationRequested) return;
            if (covered == full)
            {
                if (outside != outsideTarget) return;
                int[] ids = selected.Order().ToArray();
                if (seen.Add(string.Join(',', ids))) result.Add(ids);
                return;
            }
            if (depth == 0) return;
            int[]? next = null;
            for (int b = 0; b < size; b++)
                if ((covered & (1u << b)) == 0)
                {
                    int[] candidates = choices[b].Where(Fits).ToArray();
                    if (next is null || candidates.Length < next.Length) next = candidates;
                }
            foreach (int i in next!)
            {
                Piece p = pieces[i]; selected.Add(i); if (p.Outside) outside++;
                foreach (int c in p.Cells) if (occupied[c]++ == 1) overlaps++;
                Search(covered | masks[i], depth - 1);
                foreach (int c in p.Cells) if (--occupied[c] == 1) overlaps--;
                if (p.Outside) outside--; selected.RemoveAt(selected.Count - 1);
                if (clock.Elapsed.TotalSeconds > deadline || result.Count >= quota) break;
            }
        }
        for (outsideTarget = 0; outsideTarget <= outsideBudget; outsideTarget++)
        {
            quota = result.Count + 64; deadline = clock.Elapsed.TotalSeconds + .15;
            for (int count = regionMinimum[region]; count <= Math.Min(limit, regionMinimum[region] + 2) && result.Count < quota; count++) Search(0, count);
        }
        foreach (int desiredOutside in Enumerable.Range(0, outsideBudget + 1).Where(b => !result.Any(p => p.Count(i => pieces[i].Outside) == b)).ToArray())
        {
            CpModel model = new(); BoolVar[] take = indices.Select(i => model.NewBoolVar($"local{i}")).ToArray();
            for (int b = 0; b < size; b++) model.Add(LinearExpr.Sum(take.Where((_, j) => (masks[indices[j]] & (1u << b)) != 0)) >= 1);
            model.Add(LinearExpr.Sum(take) <= limit);
            model.Add(LinearExpr.Sum(take.Where((_, j) => pieces[indices[j]].Outside)) == desiredOutside);
            List<BoolVar> repeated = [];
            foreach (var group in indices.SelectMany((i, j) => pieces[i].Cells.Select(c => (c, j))).GroupBy(p => p.c))
            {
                if (group.Count() < 2) continue;
                BoolVar over = model.NewBoolVar($"localOver{group.Key}");
                model.Add(LinearExpr.Sum(group.Select(p => take[p.j])) <= 1).OnlyEnforceIf(over.Not());
                repeated.Add(over);
            }
            model.Add(LinearExpr.Sum(repeated) <= Craft.Skills[recipe.Character].Overlap);
            model.Minimize(LinearExpr.Sum(take) * 100 + LinearExpr.Sum(take.Where((_, j) => pieces[indices[j]].Outside)) * 10 + LinearExpr.Sum(repeated));
            for (int attempt = 0; attempt < 2; attempt++)
            {
                if (SearchBudget <= 0) break;
                CpSolver solver = new() { StringParameters = FormattableString.Invariant($"max_time_in_seconds:{Math.Min(1, SearchBudget)} num_search_workers:{threads} cp_model_probing_level:0 linearization_level:0") };
                using CancellationTokenRegistration registration = cancellation.Register(solver.StopSearch);
                CpSolverStatus status = solver.Solve(model);
                cancellation.ThrowIfCancellationRequested();
                if (status is not (CpSolverStatus.Feasible or CpSolverStatus.Optimal)) break;
                int[] chosen = Enumerable.Range(0, take.Length).Where(j => solver.Value(take[j]) != 0).ToArray();
                result.Add(chosen.Select(j => indices[j]).ToArray());
                model.Add(LinearExpr.Sum(chosen.Select(j => take[j])) <= chosen.Length - 1);
            }
        }
        result = PrunePatterns(region, result);
        localPatterns[region] = result; return result;
    }

    /// <summary>组合区域拼法；直接比较共享边界与预算，避开逐格重复搜索。</summary>
    /// <param name="sets">当前收益层的候选区域集合。</param>
    /// <param name="seconds">构造式搜索预算，秒。</param>
    /// <returns>找到的合法布局；空值只表示候选库没有找到。</returns>
    private CardTemplate? FindPatterns(RegionSet[] sets, double seconds)
    {
        Stopwatch clock = Stopwatch.StartNew();
        foreach (RegionSet set in sets.OrderBy(s => s.Cost))
        {
            if (clock.Elapsed.TotalSeconds >= seconds || SearchBudget <= 0) break;
            double preparation = clock.Elapsed.TotalSeconds;
            int[] regions = Enumerable.Range(0, recipe.Areas.Length).Where(i => (set.Mask & (1u << i)) != 0).OrderBy(i => Patterns(i).Count).ToArray();
            if (regions.Any(i => Patterns(i).Count == 0)) continue;
            Dictionary<int, uint> near = [];
            foreach (int region in regions)
                foreach (Hex c in recipe.Areas[region].Cells.SelectMany(c => Hex.Directions.Select(c.Position.Add).Append(c.Position)))
                    if (cellIndex.TryGetValue(c, out int index)) near[index] = near.GetValueOrDefault(index) | (1u << region);
            var orderedPatterns = regions.ToDictionary(i => i, i => Patterns(i)
                .OrderByDescending(p => System.Numerics.BitOperations.PopCount(p.SelectMany(j => pieces[j].Cells).Aggregate(0u, (mask, c) => mask | near.GetValueOrDefault(c))))
                .ThenBy(p => p.Length).ToArray());
            seconds += clock.Elapsed.TotalSeconds - preparation;
            int[] occupied = new int[cells.Length], covered = new int[recipe.Areas.Sum(a => a.Cells.Length)];
            int[] offsets = Enumerable.Range(0, recipe.Areas.Length).Select(r => recipe.Areas.Take(r).Sum(a => a.Cells.Length)).ToArray();
            List<int> selected = []; HashSet<int> selectedSet = []; int outside = 0, overlaps = 0;
            int maxOutside = Craft.Skills[recipe.Character].Outside + regions.Sum(i => effects[i, 9]) + currentStrikes;
            int maxOverlap = Craft.Skills[recipe.Character].Overlap + regions.Sum(i => effects[i, 10]) + currentStrikes;
            int countLimit = limit - regions.Sum(i => effects[i, 15]) + currentStrikes;
            double until = Math.Min(seconds, clock.Elapsed.TotalSeconds + .3);
            CardTemplate? answer = null;
            bool Search(int at)
            {
                if (clock.Elapsed.TotalSeconds >= until || SearchBudget <= 0 || cancellation.IsCancellationRequested) return false;
                if (at == regions.Length)
                {
                    CardTemplate card = Craft.Evaluate(catalog, recipe, selected.Select(i => pieces[i].Placement));
                    if (card.Valid && card.Strikes == currentStrikes && card.Active.Aggregate(0u, (mask, i) => mask | (1u << i)) == set.Mask)
                    { answer = card; return true; }
                    return false;
                }
                int region = regions[at];
                if (Enumerable.Range(offsets[region], recipe.Areas[region].Cells.Length).All(b => covered[b] > 0)) return Search(at + 1);
                int after = independent ? regions.Skip(at + 1).Sum(i => regionMinimum[i]) : 0;
                foreach (int[] pattern in orderedPatterns[region])
                {
                    int[] added = pattern.Where(i => !selectedSet.Contains(i)).ToArray();
                    if (selected.Count + added.Length + after > countLimit || outside + added.Count(i => pieces[i].Outside) > maxOutside) continue;
                    foreach (int i in added)
                    {
                        if (pieces[i].Outside) outside++;
                        foreach (int c in pieces[i].Cells) if (occupied[c]++ == 1) overlaps++;
                        foreach (int b in pieces[i].Matches) covered[b]++;
                        selected.Add(i); selectedSet.Add(i);
                    }
                    if (overlaps <= maxOverlap && Search(at + 1)) return true;
                    foreach (int i in added)
                    {
                        if (pieces[i].Outside) outside--;
                        foreach (int c in pieces[i].Cells) if (--occupied[c] == 1) overlaps--;
                        foreach (int b in pieces[i].Matches) covered[b]--;
                        selected.RemoveAt(selected.Count - 1); selectedSet.Remove(i);
                    }
                    if (clock.Elapsed.TotalSeconds >= until) break;
                }
                return false;
            }
            if (Search(0)) return answer;
        }
        return null;
    }

    /// <summary>优先按最受约束奖励格回溯构造；只提供合法解，不以紧域失败声明不可行。</summary>
    /// <param name="sets">同收益的替代区域集合。</param>
    /// <param name="seconds">从零构造的时间预算，秒。</param>
    /// <returns>经真实规则复算的合法布局，或未找到时为空。</returns>
    private CardTemplate? FindLayout(RegionSet[] sets, double seconds)
    {
        Stopwatch clock = Stopwatch.StartNew();
        int[] bitArea = recipe.Areas.SelectMany((a, i) => a.Cells.Select(_ => i)).ToArray();
        int[] bitCell = recipe.Areas.SelectMany(a => a.Cells.Select(c => cellIndex[c.Position])).ToArray();
        foreach (RegionSet set in sets.OrderBy(s => s.Cost).ThenByDescending(s => s.Power + s.Fortitude))
        {
            if (clock.Elapsed.TotalSeconds >= seconds) break;
            double setStart = clock.Elapsed.TotalSeconds;
            double until = Math.Min(seconds, setStart + .2);
            bool[] required = bitArea.Select(i => (set.Mask & (1u << i)) != 0).ToArray();
            HashSet<Hex> forced = Enumerable.Range(0, required.Length).Where(b => required[b]).Select(b => cells[bitCell[b]]).ToHashSet();
            var components = Craft.Components(forced);
            Dictionary<int, ulong> near = [];
            for (int i = 0; i < components.Count; i++)
                foreach (Hex c in components[i].SelectMany(c => Hex.Directions.Select(c.Add).Append(c)))
                    if (cellIndex.TryGetValue(c, out int index)) near[index] = near.GetValueOrDefault(index) | (1UL << i);
            int maxCount = limit - Enumerable.Range(0, recipe.Areas.Length).Where(i => (set.Mask & (1u << i)) != 0).Sum(i => effects[i, 15]) + currentStrikes;
            int maxOutside = Craft.Skills[recipe.Character].Outside + Enumerable.Range(0, recipe.Areas.Length).Where(i => (set.Mask & (1u << i)) != 0).Sum(i => effects[i, 9]) + currentStrikes;
            int maxOverlap = Craft.Skills[recipe.Character].Overlap + Enumerable.Range(0, recipe.Areas.Length).Where(i => (set.Mask & (1u << i)) != 0).Sum(i => effects[i, 10]) + currentStrikes;
            int[] gains = pieces.Select(p => System.Numerics.BitOperations.PopCount(p.Cells.Aggregate(0UL, (m, c) => m | near.GetValueOrDefault(c)))).ToArray();
            int[][] byBit = Enumerable.Range(0, required.Length).Select(b => required[b]
                ? Enumerable.Range(0, pieces.Count).Where(i => pieces[i].Matches.Contains(b)).OrderByDescending(i => gains[i] * 10 - (pieces[i].Outside ? 8 : 0) +
                    pieces[i].Matches.Count(b => required[b]) * (recipe.Areas.All(a => a.Cells.Length == 1) ? 1 : 100)).ToArray() : []).ToArray();
            int[] occupied = new int[cells.Length], covered = new int[required.Length], areaCovered = new int[recipe.Areas.Length];
            List<int> chosen = []; int outside = 0, overlaps = 0, visited = 0;
            CardTemplate? answer = null;
            bool Fits(int index)
            {
                Piece p = pieces[index];
                if (p.Outside && outside >= maxOutside || chosen.Contains(index)) return false;
                if (overlaps + p.Cells.Count(c => occupied[c] == 1) > maxOverlap) return false;
                foreach (var g in p.Matches.Where(b => covered[b] == 0).GroupBy(b => bitArea[b]))
                    if ((set.Mask & (1u << g.Key)) == 0 && areaCovered[g.Key] + g.Count() == recipe.Areas[g.Key].Cells.Length) return false;
                return true;
            }
            void Change(int index, bool add)
            {
                Piece p = pieces[index]; int d = add ? 1 : -1;
                if (p.Outside) outside += d;
                foreach (int c in p.Cells)
                {
                    if (add) { if (occupied[c]++ == 1) overlaps++; }
                    else { if (--occupied[c] == 1) overlaps--; }
                }
                foreach (int b in p.Matches)
                {
                    if (add) { if (covered[b]++ == 0) areaCovered[bitArea[b]]++; }
                    else { if (--covered[b] == 0) areaCovered[bitArea[b]]--; }
                }
                if (add) chosen.Add(index); else chosen.RemoveAt(chosen.Count - 1);
            }
            bool Search()
            {
                if (++visited % 128 == 0 && (clock.Elapsed.TotalSeconds >= until || cancellation.IsCancellationRequested)) return false;
                int[]? next = null;
                for (int b = 0; b < required.Length; b++)
                {
                    if (!required[b] || covered[b] != 0) continue;
                    int[] possible = byBit[b].Where(Fits).ToArray();
                    if (possible.Length == 0) return false;
                    if (next is null || possible.Length < next.Length) next = possible;
                }
                if (next is null)
                {
                    CardTemplate card = Craft.Evaluate(catalog, recipe, chosen.Select(i => pieces[i].Placement));
                    if (card.Valid && card.Strikes == currentStrikes && card.Active.Aggregate(0u, (mask, i) => mask | (1u << i)) == set.Mask)
                    { answer = card; return true; }
                    if (chosen.Count >= maxCount) return false;
                    var groups = Craft.Components(Enumerable.Range(0, occupied.Length).Where(c => occupied[c] > 0).Select(c => cells[c]));
                    Dictionary<int, ulong> adjacent = [];
                    for (int i = 0; i < groups.Count; i++)
                        foreach (Hex c in groups[i].SelectMany(c => Hex.Directions.Select(c.Add).Append(c)))
                            if (cellIndex.TryGetValue(c, out int index)) adjacent[index] = adjacent.GetValueOrDefault(index) | (1UL << i);
                    next = Enumerable.Range(0, pieces.Count).Where(Fits).Select(i => (i, gain: System.Numerics.BitOperations.PopCount(pieces[i].Cells.Aggregate(0UL, (m, c) => m | adjacent.GetValueOrDefault(c)))))
                        .Where(p => p.gain >= 2).OrderByDescending(p => p.gain).ThenBy(p => pieces[p.i].Outside).Select(p => p.i).ToArray();
                }
                else if (chosen.Count >= maxCount || independent && chosen.Count + Enumerable.Range(0, recipe.Areas.Length)
                    .Where(i => (set.Mask & (1u << i)) != 0).Sum(i => (recipe.Areas[i].Cells.Length - areaCovered[i] + regionMaxCover[i] - 1) / regionMaxCover[i]) > maxCount) return false;
                foreach (int index in next)
                {
                    if (clock.Elapsed.TotalSeconds >= until) return false;
                    Change(index, true);
                    if (Search()) return true;
                    Change(index, false);
                }
                return false;
            }
            if (Search()) return answer;
            // 先放能同时接通多个必选区域的桥，再补奖励粒子，避免单格先占满数量预算。
            int[] bridges = Enumerable.Range(0, pieces.Count).Where(i => gains[i] >= 3)
                .OrderByDescending(i => gains[i]).ThenBy(i => pieces[i].Outside).ThenByDescending(i => pieces[i].Matches.Count(b => required[b])).Take(80).ToArray();
            foreach (int bridge in bridges)
            {
                if (clock.Elapsed.TotalSeconds >= seconds || clock.Elapsed.TotalSeconds - setStart > .8) break;
                if (!Fits(bridge)) continue;
                until = Math.Min(seconds, clock.Elapsed.TotalSeconds + .05);
                Change(bridge, true);
                if (Search()) return answer;
                Change(bridge, false);
            }
        }
        return null;
    }

    /// <summary>固定激活集合后仅求覆盖与连接；必选区域占用格收缩为连通节点。</summary>
    /// <param name="sets">需要考虑的替代区域集合。</param>
    /// <param name="key">目标槽位与左右范围。</param>
    /// <param name="mode">覆盖松弛、紧域、完整域或候选拼法域。</param>
    /// <param name="seconds">可选的短搜索时间片。</param>
    /// <returns>布局与原生求解状态；紧域或候选域无解不能直接当全域无解。</returns>
    private (CardTemplate? Card, CpSolverStatus Status) Geometry(RegionSet[] sets, int[] key, GeometryMode mode, double? seconds = null)
    {
        bool compact = mode != GeometryMode.Full, connect = mode != GeometryMode.Cover;
        if (mode == GeometryMode.Full && sets.Length > 1 && Environment.GetEnvironmentVariable("INFALSUS_SPLIT") == "1")
        {
            var ordered = sets.Select(s => (Set: s, Count: PruneChoices([s]).Length)).OrderBy(p => p.Count).ToArray();
            Console.WriteLine($"[{recipe.Id}] 分开连接证明：{string.Join(',', ordered.Select(p => p.Count))}个候选，先小域。");
            bool unknown = false;
            foreach (var item in ordered)
            {
                cancellation.ThrowIfCancellationRequested();
                var answer = Geometry([item.Set], key, mode, seconds);
                if (answer.Card is not null) return answer;
                if (answer.Status == CpSolverStatus.Infeasible) { solved[(item.Set.Mask, currentStrikes)] = null; Save(); }
                else unknown = true;
            }
            return (null, unknown ? CpSolverStatus.Unknown : CpSolverStatus.Infeasible);
        }
        double defaultBudget = retry ? mode switch { GeometryMode.Full => 120, GeometryMode.Cover => 30, _ => slice } : slice;
        double budget = Math.Min(SearchBudget, seconds ?? defaultBudget);
        if (budget <= 0) return (null, CpSolverStatus.Unknown);
        progress = $"{target} {(connect ? "连接" : "覆盖")}/{(compact ? "紧域" : "全域")} 建模";
        cancellation.ThrowIfCancellationRequested();
        uint common = sets.Aggregate(uint.MaxValue, (mask, s) => mask & s.Mask);
        uint union = sets.Aggregate(0u, (mask, s) => mask | s.Mask);
        bool[] required = recipe.Areas.Select((_, i) => (common & (1u << i)) != 0).ToArray();
        int outsideLimit = Craft.Skills[recipe.Character].Outside + Enumerable.Range(0, required.Length).Sum(i => Math.Max(0, effects[i, 9])) + currentStrikes;
        bool noSpare = sets.All(s => s.Cost + Enumerable.Range(0, required.Length).Where(i => (s.Mask & (1u << i)) != 0).Sum(i => effects[i, 15]) == limit);
        HashSet<Hex> forced = recipe.Areas.Where((_, i) => required[i]).SelectMany(a => a.Cells).Select(c => c.Position).ToHashSet();
        List<HashSet<Hex>> forcedComponents = Craft.Components(forced);
        Dictionary<int, ulong> near = [];
        for (int i = 0; i < forcedComponents.Count; i++)
            foreach (Hex cell in forcedComponents[i].SelectMany(c => Hex.Directions.Select(c.Add).Append(c)))
                if (cellIndex.TryGetValue(cell, out int index)) near[index] = near.GetValueOrDefault(index) | (1UL << i);
        bool Bridges(Piece p) => System.Numerics.BitOperations.PopCount(p.Cells.Aggregate(0UL, (mask, c) => mask | near.GetValueOrDefault(c))) >= 2;
        Option[] choices = PruneChoices(sets).Where(p => (!p.Outside || outsideLimit > 0) &&
            (!noSpare || (p.Regions & union) != 0) && (!compact || (p.Regions & union) != 0 || connect && (!p.Outside && p.Cells.Length == 1 || Bridges(p))))
            .Select(p => new Option([p.Placement], p.Cells, p.Matches, p.Regions, p.Outside ? 1 : 0)).ToArray();
        CpModel model = new();
        BoolVar[] selected = choices.Select((_, i) => model.NewBoolVar($"p{i}")).ToArray();
        List<BoolVar>[] cover = cells.Select(_ => new List<BoolVar>()).ToArray();
        List<BoolVar>[] forcedOverlap = cells.Select(_ => new List<BoolVar>()).ToArray();
        Dictionary<int, int> forcedColors = recipe.Areas.Where((_, i) => required[i]).SelectMany(a => a.Cells)
            .GroupBy(c => cellIndex[c.Position]).ToDictionary(g => g.Key, g => g.First().Color);
        int bits = recipe.Areas.Sum(a => a.Cells.Length);
        List<BoolVar>[] matches = Enumerable.Range(0, bits).Select(_ => new List<BoolVar>()).ToArray();
        for (int i = 0; i < choices.Length; i++)
        {
            foreach (int c in choices[i].Cells)
            {
                cover[c].Add(selected[i]);
                if (forcedColors.TryGetValue(c, out int color) && color != catalog.Shapes[choices[i].Layout[0].Id].Color)
                    forcedOverlap[c].Add(selected[i]);
            }
            foreach (int b in choices[i].Matches) matches[b].Add(selected[i]);
        }
        BoolVar[] active = recipe.Areas.Select((_, i) => model.NewBoolVar($"area{i}")).ToArray();
        if (sets.Length is > 1 and <= 32)
        {
            BoolVar[] cases = sets.Select((_, i) => model.NewBoolVar($"case{i}")).ToArray();
            model.AddExactlyOne(cases);
            for (int r = 0; r < active.Length; r++)
                model.Add(active[r] == LinearExpr.Sum(cases.Where((_, s) => (sets[s].Mask & (1u << r)) != 0)));
            int[] indices = choices.Select(p => placementIndex[(p.Layout[0].Id, p.Layout[0].Q, p.Layout[0].R)]).ToArray();
            int disabled = 0;
            for (int s = 0; s < sets.Length; s++)
            {
                RegionSet set = sets[s];
                if (connect && currentStrikes == 0) AddSafeConnectionBound(model, selected, choices, set, cases[s]);
                HashSet<int> domain = prunedDomains[set.Mask].ToHashSet();
                for (int i = 0; i < selected.Length; i++)
                    if (!domain.Contains(indices[i])) { model.AddImplication(cases[s], selected[i].Not()); disabled++; }
                model.Add(LinearExpr.Sum(selected.Where((_, i) => (choices[i].Regions & set.Mask) != 0)) >= set.Cost).OnlyEnforceIf(cases[s]);
                int remaining = limit - set.Cost - Enumerable.Range(0, active.Length).Where(r => (set.Mask & (1u << r)) != 0).Sum(r => effects[r, 15]) + currentStrikes;
                model.Add(LinearExpr.Sum(selected.Where((_, i) => (choices[i].Regions & set.Mask) == 0)) <= remaining).OnlyEnforceIf(cases[s]);
            }
            Console.WriteLine($"[{recipe.Id}] 组合条件：{sets.Length}组，{disabled}条放置禁用关系。");
        }
        else
        {
            long[,] activation = new long[sets.Length, active.Length];
            for (int s = 0; s < sets.Length; s++)
                for (int i = 0; i < active.Length; i++) activation[s, i] = (sets[s].Mask & (1u << i)) != 0 ? 1 : 0;
            model.AddAllowedAssignments(active).AddTuples(activation);
        }
        if (connect && sets.Length == 1 && currentStrikes == 0) AddSafeConnectionBound(model, selected, choices, sets[0]);
        model.Add(LinearExpr.Sum(selected.Where((_, i) => (choices[i].Regions & union) != 0)) >= sets.Min(s => s.Cost));
        int spare = sets.Max(s => limit - s.Cost - Enumerable.Range(0, active.Length).Where(i => (s.Mask & (1u << i)) != 0).Sum(i => effects[i, 15])) + currentStrikes;
        model.Add(LinearExpr.Sum(selected.Where((_, i) => (choices[i].Regions & union) == 0)) <= spare);
        LinearExpr Effect(int kind) => LinearExpr.WeightedSum(active, Enumerable.Range(0, active.Length).Select(i => (long)effects[i, kind]).ToArray());
        LinearExpr selectedCount = LinearExpr.WeightedSum(selected, choices.Select(p => (long)p.Layout.Length).ToArray());
        LinearExpr outside = LinearExpr.WeightedSum(selected, choices.Select(p => (long)p.Outside).ToArray());
        IntVar countPenalty = model.NewIntVar(0, currentStrikes, "countPenalty");
        IntVar outsidePenalty = model.NewIntVar(0, currentStrikes, "outsidePenalty");
        IntVar overlapPenalty = model.NewIntVar(0, currentStrikes, "overlapPenalty");
        IntVar splitPenalty = model.NewIntVar(0, currentStrikes, "splitPenalty");
        model.AddMaxEquality(countPenalty, [selectedCount + Effect(15) - limit, model.NewConstant(0)]);
        model.AddMaxEquality(outsidePenalty, [outside - Craft.Skills[recipe.Character].Outside - Effect(9), model.NewConstant(0)]);
        foreach (var (kind, value, cap) in new[] { (7, key[0], 3), (16, key[1], 4), (17, key[2], 4) })
        {
            if (key[0] == 0 && kind is 16 or 17) continue;
            if (value < cap) model.Add(Effect(kind) == value); else model.Add(Effect(kind) >= value);
        }
        IntVar power = model.NewIntVar(1, 200000, "power"); IntVar fortitude = model.NewIntVar(1, 200000, "fortitude");
        model.Add(power == Effect(3)); model.Add(fortitude == Effect(4));
        var panels = sets.Select(s => (s.Power, s.Fortitude)).Distinct().ToArray();
        long[,] tuples = new long[panels.Length, 2];
        for (int i = 0; i < panels.Length; i++) { tuples[i, 0] = panels[i].Power; tuples[i, 1] = panels[i].Fortitude; }
        model.AddAllowedAssignments([power, fortitude]).AddTuples(tuples);
        int bit = 0;
        for (int i = 0; i < required.Length; i++)
        {
            if (required[i]) model.Add(active[i] == 1);
            if ((union & (1u << i)) == 0) model.Add(active[i] == 0);
            List<ILiteral> missing = [];
            foreach (BoardCell cell in recipe.Areas[i].Cells)
            {
                var m = matches[bit++];
                BoolVar filled = model.NewBoolVar($"fill{bit}");
                if (m.Count == 0) model.Add(filled == 0); else model.AddMaxEquality(filled, m);
                model.AddImplication(active[i], filled); missing.Add(filled.Not());
            }
            model.AddBoolOr(missing.Append(active[i]));
        }
        if (currentStrikes == 0 && outsideBudget <= 1)
        {
            int activeParts = sets.Min(s => System.Numerics.BitOperations.PopCount(Enumerable.Range(0, regionSafeParts.Length)
                .Where(i => (s.Mask & (1u << i)) != 0).Aggregate(0UL, (mask, i) => mask | regionSafeParts[i])));
            int allowedParts = Craft.Skills[recipe.Character].Split + 1 + sets.Max(s => Enumerable.Range(0, recipe.Areas.Length).Where(i => (s.Mask & (1u << i)) != 0).Sum(i => effects[i, 11]));
            long[] spent = choices.Select(p => (long)maxSafeMerge * p.Outside - (p.Outside == 0 ? 0 : Math.Max(0,
                System.Numerics.BitOperations.PopCount(p.Cells.Aggregate(0UL, (mask, c) => mask | safeContacts[c])) - 1))).ToArray();
            // 未用于覆盖的越界额度按最大可能连接收益计入，仍是完整问题的必要条件。
            model.Add(LinearExpr.WeightedSum(selected, spent) <= allowedParts + maxSafeMerge * outsideBudget - activeParts);
        }
        for (int i = 0; i < required.Length; i++)
        {
            var relevant = Enumerable.Range(0, selected.Length).Where(j => (choices[j].Regions & (1u << i)) != 0).ToArray();
            LinearExpr count = LinearExpr.WeightedSum(relevant.Select(j => selected[j]), relevant.Select(j => (long)choices[j].Layout.Length));
            model.Add(count >= regionMinimum[i] * active[i]);
            int discount = regionSafeMinimum[i] - regionMinimum[i];
            if (discount > 0)
                model.Add(count + discount * LinearExpr.Sum(relevant.Where(j => choices[j].Outside > 0).Select(j => selected[j])) >= regionSafeMinimum[i] * active[i]);
        }
        List<BoolVar> overlap = [];
        for (int c = 0; c < cells.Length; c++)
        {
            if (cover[c].Count < 2) continue;
            BoolVar over = model.NewBoolVar($"over{c}");
            model.Add(LinearExpr.Sum(cover[c]) <= 1).OnlyEnforceIf(over.Not());
            model.Add(LinearExpr.Sum(cover[c]) >= 2).OnlyEnforceIf(over);
            foreach (BoolVar particle in forcedOverlap[c]) model.AddImplication(particle, over);
            overlap.Add(over);
        }
        model.AddMaxEquality(overlapPenalty, [LinearExpr.Sum(overlap) - Craft.Skills[recipe.Character].Overlap - Effect(10), model.NewConstant(0)]);
        if (connect) AddConnection(model, active, cover, forced, sets, splitPenalty);
        model.Add(countPenalty + outsidePenalty + overlapPenalty + splitPenalty <= currentStrikes);
        bool bridgeFirst = currentStrikes == 0 && connect && sets.All(s =>
        {
            int[] regions = Enumerable.Range(0, recipe.Areas.Length).Where(r => (s.Mask & (1u << r)) != 0).ToArray();
            int outside = Craft.Skills[recipe.Character].Outside + regions.Sum(r => effects[r, 9]);
            int roots = Craft.Skills[recipe.Character].Split + 1 + regions.Sum(r => effects[r, 11]);
            ulong parts = regions.Aggregate(0UL, (bits, r) => bits | regionSafeParts[r]);
            return outside == 1 && System.Numerics.BitOperations.PopCount(parts) > roots;
        });
        if (bridgeFirst)
        {
            BoolVar[] bridges = Enumerable.Range(0, selected.Length).Where(i => choices[i].Outside > 0)
                .OrderByDescending(i => choices[i].Matches.Length).Select(i => selected[i]).ToArray();
            model.AddExactlyOne(bridges);
            model.AddDecisionStrategy(bridges, DecisionStrategyProto.Types.VariableSelectionStrategy.ChooseFirst,
                DecisionStrategyProto.Types.DomainReductionStrategy.SelectMaxValue);
            Console.WriteLine($"[{recipe.Id}] 连接优先：先决定{bridges.Length}种必需越界放置之一，再自动搜索覆盖。");
        }
        if (retry) model.Minimize(LinearExpr.Sum(selected));
        budget = Math.Min(budget, SearchBudget);
        CpSolver solver = new();
        using CancellationTokenRegistration registration = cancellation.Register(solver.StopSearch);
        using Timer stopGuard = double.IsPositiveInfinity(budget)
            ? new Timer(_ => { if (cancellation.IsCancellationRequested) solver.StopSearch(); }, null, 250, 250)
            : new Timer(_ => solver.StopSearch(), null, Math.Max(1, (int)Math.Ceiling(budget * 1000)), Timeout.Infinite);
        Stopwatch watch = Stopwatch.StartNew();
        while (watch.Elapsed.TotalSeconds < budget)
        {
            int workers = threads;
            string duration = double.IsPositiveInfinity(budget) ? "" : FormattableString.Invariant($"max_time_in_seconds:{Math.Max(.001, budget - watch.Elapsed.TotalSeconds)} ");
            solver.StringParameters = duration + FormattableString.Invariant($"num_search_workers:{workers} random_seed:1 stop_after_first_solution:true linearization_level:{(connect || retry ? 2 : 0)} cp_model_probing_level:{(retry ? 2 : 0)}")
                + (connect && workers > 1 ? FormattableString.Invariant($" num_full_subsolvers:{workers - 1} extra_subsolvers:\"max_lp\"") : "")
                + (bridgeFirst ? " search_branching:PARTIAL_FIXED_SEARCH" : "");
            bool linear = mode == GeometryMode.Full && Environment.GetEnvironmentVariable("INFALSUS_LINEAR") == "1";
            string budgetLabel = double.IsPositiveInfinity(budget) ? "无时限" : retry ? "置信度预算" : "首轮/快速检查";
            progress = $"{target} {(linear ? "连续流" : connect ? "连接" : "覆盖")}/{(compact ? "紧域" : "全域")} {workers}线程 {budgetLabel}";
            cancellation.ThrowIfCancellationRequested();
            int[] chosen;
            CpSolverStatus status;
            if (linear) (chosen, status) = SolveLinear(model, selected, budget - watch.Elapsed.TotalSeconds);
            else
            {
                status = solver.Solve(model);
                chosen = status is CpSolverStatus.Optimal or CpSolverStatus.Feasible
                    ? Enumerable.Range(0, selected.Length).Where(i => solver.Value(selected[i]) != 0).ToArray() : [];
            }
            cancellation.ThrowIfCancellationRequested();
            if (status == CpSolverStatus.ModelInvalid) throw new InvalidDataException(solver.ResponseStats());
            if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible))
            {
                Console.WriteLine($"[{recipe.Id}] {(connect ? "连接" : "覆盖")}/{(compact ? "紧域" : "全域")} {choices.Length}放置 {status} {watch.Elapsed.TotalSeconds:F3}秒。");
                return (null, status);
            }
            CardTemplate card = Craft.Evaluate(catalog, recipe, chosen.SelectMany(i => choices[i].Layout));
            HashSet<Hex> rewards = card.Active.SelectMany(i => recipe.Areas[i].Cells.Select(c => c.Position)).ToHashSet();
            var decoration = Craft.Components(card.Placements.SelectMany(p => p.Cells)).Where(c => !c.Overlaps(rewards)).SelectMany(c => c).ToHashSet();
            if (decoration.Count > 0)
            {
                chosen = chosen.Where(i => !choices[i].Cells.All(c => decoration.Contains(cells[c]))).ToArray();
                card = Craft.Evaluate(catalog, recipe, chosen.SelectMany(i => choices[i].Layout));
            }
            uint actual = card.Active.Aggregate(0u, (mask, i) => mask | (1u << i));
            if (!sets.Any(s => s.Mask == actual)) throw new InvalidDataException($"区域集合约束不一致：{recipe.Id}/{actual}");
            if (card.Valid && card.Strikes == currentStrikes)
            {
                if (watch.Elapsed.TotalSeconds > 1) Console.WriteLine($"[{recipe.Id}] {(connect ? "连接" : "覆盖")}/{(compact ? "紧域" : "全域")} {choices.Length}放置 可行 {watch.Elapsed.TotalSeconds:F3}秒。");
                return (card, status);
            }
            int[] rejected = chosen;
            LinearExpr noGood = LinearExpr.Sum(rejected.Select(i => selected[i])) -
                LinearExpr.Sum(Enumerable.Range(0, selected.Length).Except(rejected).Select(i => selected[i]));
            model.Add(noGood <= rejected.Length - 1);
            if (connect && currentStrikes == 0) throw new InvalidDataException($"完整连接模型复核不一致：{recipe.Id}，惩罚{card.Strikes}。");
        }
        return (null, CpSolverStatus.Unknown);
    }

    /// <summary>覆盖松弛、启发式紧域和可承担不可行证明的完整粒子域。</summary>
    private enum GeometryMode { Cover, Compact, Full }
    /// <summary>模型中的一个选择，可为单粒子或已经完整覆盖区域的一组粒子。</summary>
    private sealed record Option(Placement[] Layout, int[] Cells, int[] Matches, uint Regions, int Outside);
    /// <summary>一个完整区域选择的确定面板与安全粒子数下界。</summary>
    private readonly record struct RegionSet(uint Mask, int Power, int Fortitude, int Cost);
    /// <summary>真实粒子平移及其几何和奖励覆盖索引。</summary>
    private sealed record Piece(Placement Placement, int[] Cells, int[] Matches, uint Regions, bool Outside);
}
