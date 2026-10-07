using System.Collections.Concurrent;
using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>
/// 一阶段展示层：配队层之外，每个配方、净惩罚与槽位范围只保留攻最高、防最高和最大总和的全部攻防拆分，
/// 供网页展示低价值卡的代表拼法，不进入配队卡库。
/// 以检查点旧布局与面板求解新布局为已知端点，对高于已知端点的理论面板按棋盘基础时限短时补求；
/// 同一目标连续MissLimit个面板时限内未找到即停止（经验剪枝，可能漏掉更高端点）。
/// 结果做v3材料精化后输出results/panel-display.json，结构与面板求解输出相同。
/// </summary>
internal static class PanelDisplay
{
    /// <summary>输出策略版本，改变端点规则或补求规则时递增。</summary>
    public const string Policy = "panel-display-v1";
    /// <summary>同一目标连续多少个面板在基础时限内未找到（且未证不可行）后停止补求。</summary>
    private const int MissLimit = 8;

    /// <summary>展示层输出文件。</summary>
    internal static string OutputPath => Path.Combine(Storage.Root, "results", "panel-display.json");

    /// <summary>展示层分组：同配方、同净惩罚、同槽位与左右范围。</summary>
    private readonly record struct Group(int Recipe, int Strikes, int Slots, int Left, int Right);

    private static Group GroupOf(PanelSurvey.Panel p) => new(p.Recipe, p.Strikes, p.Slots, p.Left, p.Right);

    /// <summary>目标排序键：0攻优先再防，1防优先再攻，2只比攻防总和（同总和的拆分都是端点）。</summary>
    private static (int Primary, int Secondary) Objective(PanelSurvey.Panel p, int goal) => goal switch
    {
        0 => (p.Power, p.Fortitude),
        1 => (p.Fortitude, p.Power),
        _ => (p.Power + p.Fortitude, 0)
    };

    /// <summary>运行展示层补求并输出。</summary>
    /// <param name="args">可选--threads与--seconds（补求阶段总墙钟预算，秒）。</param>
    /// <returns>完成0；用户停止2。预算用完时以已找到的端点输出并返回0。</returns>
    public static int Run(string[] args)
    {
        int threads = Environment.ProcessorCount, seconds = 7200;
        if (args.Length % 2 != 0)
            throw new ArgumentException("solve-display的参数应成对给出。");
        for (int i = 0; i < args.Length; i += 2)
            switch (args[i])
            {
                case "--threads": threads = int.Parse(args[i + 1]); break;
                case "--seconds": seconds = int.Parse(args[i + 1]); break;
                default: throw new ArgumentException($"solve-display不支持参数{args[i]}。");
            }
        if (threads < 1 || threads > Environment.ProcessorCount || seconds < 60)
            throw new ArgumentException("线程数应在本机逻辑CPU数量内，预算至少60秒。");
        using FileStream lease = Storage.AcquireLock();
        string cancelFile = Path.Combine(Storage.State, "cancel");
        if (File.Exists(cancelFile))
            File.Move(cancelFile, cancelFile + ".previous", true);
        // stop为用户停止；budget另含补求预算，到期只结束补求，仍执行材料精化与输出。
        using CancellationTokenSource stop = new();
        using CancellationTokenSource budget = CancellationTokenSource.CreateLinkedTokenSource(stop.Token);
        budget.CancelAfter(TimeSpan.FromSeconds(seconds));
        using Timer watcher = new(_ => { if (File.Exists(cancelFile)) stop.Cancel(); }, null, 250, 250);
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; stop.Cancel(); };
        Console.CancelKeyPress += handler;
        try
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            Stopwatch clock = Stopwatch.StartNew();
            Catalog catalog = new();
            PanelSurvey.Layers[] layers = PanelSurvey.SurveyAll(catalog, threads);
            HashSet<PanelSurvey.Panel> deck = PanelSolve.LoadCards(catalog, PanelSolve.OutputPath, PanelSolve.Policy, "solve-panels")
                .Select(PanelSolve.Of).ToHashSet();
            PanelSolve.Progress progress = PanelSolve.LoadProgress(catalog, PanelSolve.ProgressPath);

            // 已知布局：检查点优先，保证与现有卡库的编号连续；只取合法且净惩罚在保留层内的布局。
            Dictionary<PanelSurvey.Panel, (CardTemplate Card, bool Fresh)> known = [];
            foreach (Recipe recipe in catalog.Data.Recipes)
                foreach (CardTemplate card in KeyRecipeSolver.Load(catalog, recipe)?.Cards.Values ?? Enumerable.Empty<CardTemplate>())
                    if (card.Valid && ConfidenceAnalysis.RetainedStrikes.Contains(card.Strikes))
                        known.TryAdd(PanelSolve.Of(card), (card, false));
            foreach (CardTemplate card in progress.Found.Values)
                known.TryAdd(PanelSolve.Of(card), (card, true));

            var tasks = Plan(layers, known.Keys, progress.Impossible);
            Console.WriteLine($"展示层：已知{known.Count}个面板，补求{tasks.Length}个端点目标、"
                + $"候选{tasks.Sum(t => t.Candidates.Length)}个面板，{threads}线程，预算{seconds / 3600.0:F1}小时。");
            Dictionary<PanelSurvey.Panel, CardTemplate> found = Solve(catalog, layers, progress, tasks, threads, budget.Token);
            stop.Token.ThrowIfCancellationRequested();
            foreach ((PanelSurvey.Panel panel, CardTemplate card) in found)
                known.TryAdd(panel, (card, true));

            List<(CardTemplate Card, bool Fresh)> display = [];
            foreach (var group in known.GroupBy(pair => GroupOf(pair.Key)))
            {
                var members = group.Select(pair => pair.Key).ToArray();
                int maxTotal = members.Max(p => p.Power + p.Fortitude);
                PanelSurvey.Panel[] ends =
                [
                    members.MaxBy(p => Objective(p, 0)),
                    members.MaxBy(p => Objective(p, 1)),
                    .. members.Where(p => p.Power + p.Fortitude == maxTotal)
                ];
                display.AddRange(ends.Distinct().Where(end => !deck.Contains(end)).Select(end => known[end]));
            }
            Console.WriteLine($"展示层{display.Count}个面板（补求新找到{found.Count}）；开始材料精化。");
            var (refined, errors) = PanelSolve.RefineAll(catalog, display.Select(item => item.Card), threads, stop.Token);
            Storage.Write(OutputPath, new
            {
                policy = Policy,
                material_policy = MaterialTierRefinement.Policy,
                catalog = catalog.Data.Id,
                hours = clock.Elapsed.TotalHours,
                found = found.Count,
                cards = display.Select(item => new
                {
                    fresh = item.Fresh,
                    source = item.Card.Id,
                    refine_error = errors.GetValueOrDefault(item.Card.Id),
                    card = refined[item.Card.Id]
                }).OrderBy(x => x.card.Recipe).ThenBy(x => x.card.Id, StringComparer.Ordinal).ToArray()
            });
            Console.WriteLine($"展示层输出：{OutputPath}，总计{clock.Elapsed.TotalHours:F2}小时。");
            return 0;
        }
        catch (Exception error) when (stop.IsCancellationRequested && PanelSolve.IsCancellation(error))
        {
            Console.WriteLine("展示层补求已停止，输出未写入。");
            return 2;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }

    /// <summary>一个端点补求目标。</summary>
    /// <param name="Recipe">配方编号。</param>
    /// <param name="Goal">0攻、1防、2总和。</param>
    /// <param name="Candidates">同分组内目标值高于已知端点、未被证明不可行的理论面板，按目标从高到低。</param>
    private sealed record Target(int Recipe, int Goal, PanelSurvey.Panel[] Candidates);

    /// <summary>
    /// 只对已有已知布局的分组补求：从未求出过的槽位范围多为置信筛选淘汰的结构，逐一补求成本过高。
    /// </summary>
    private static Target[] Plan(PanelSurvey.Layers[] layers, IEnumerable<PanelSurvey.Panel> known, HashSet<string> impossible)
    {
        var byGroup = known.GroupBy(GroupOf).ToDictionary(g => g.Key, g => g.ToArray());
        List<Target> result = [];
        foreach (PanelSurvey.Layers layer in layers)
            foreach (var group in layer.Masks.Where(pair => pair.Value.Count > 0).Select(pair => pair.Key)
                .Where(p => byGroup.ContainsKey(GroupOf(p)) && !impossible.Contains(PanelSolve.Key(p))).GroupBy(GroupOf))
                for (int goal = 0; goal < 3; goal++)
                {
                    var best = byGroup[group.Key].Max(p => Objective(p, goal));
                    PanelSurvey.Panel[] candidates = [.. group.Where(p => Objective(p, goal).CompareTo(best) > 0)
                        .OrderByDescending(p => Objective(p, goal))];
                    if (candidates.Length > 0)
                        result.Add(new(layer.Recipe, goal, candidates));
                }
        return [.. result.OrderBy(t => t.Recipe)];
    }
    /// <summary>
    /// 按目标并行补求：每个目标从高到低逐个面板求解，找到即为新端点（总和目标再补齐同总和的其余拆分）后停止；
    /// 证明不可行不计入连续未找到次数。同配方求解器实例放入池中复用几何预编译。
    /// </summary>
    /// <returns>新找到的面板与布局；预算用完时返回已找到的部分。</returns>
    private static Dictionary<PanelSurvey.Panel, CardTemplate> Solve(Catalog catalog, PanelSurvey.Layers[] layers,
        PanelSolve.Progress progress, Target[] targets, int threads, CancellationToken budget)
    {
        Dictionary<int, PanelSurvey.Layers> byRecipe = layers.ToDictionary(l => l.Recipe);
        ConcurrentDictionary<PanelSurvey.Panel, CardTemplate> found = [];
        ConcurrentDictionary<int, ConcurrentBag<KeyRecipeSolver>> pools = [];
        int done = 0;
        KeyRecipeSolver Take(Recipe recipe)
        {
            if (pools.GetOrAdd(recipe.Id, _ => []).TryTake(out KeyRecipeSolver? solver))
                return solver;
            solver = new(catalog, recipe, () => 1, 1, budget);
            solver.SeedPatternsFromCheckpoint();
            foreach (CardTemplate card in progress.Found.Values.Where(card => card.Recipe == recipe.Id))
                solver.Seed(card);
            return solver;
        }
        try
        {
            Parallel.ForEach(Partitioner.Create(targets, EnumerablePartitionerOptions.NoBuffering),
                new ParallelOptions { MaxDegreeOfParallelism = threads, CancellationToken = budget }, target =>
                {
                    PanelSurvey.Layers layer = byRecipe[target.Recipe];
                    Recipe recipe = catalog.Data.Recipes.Single(r => r.Id == target.Recipe);
                    KeyRecipeSolver solver = Take(recipe);
                    try
                    {
                        int misses = 0;
                        (int, int)? hit = null;
                        foreach (PanelSurvey.Panel panel in target.Candidates)
                        {
                            // 已有端点后只补同目标值的其余面板（总和目标的其他拆分）。
                            if (hit is not null && Objective(panel, target.Goal) != hit)
                                break;
                            if (found.TryGetValue(panel, out _))
                            {
                                hit ??= Objective(panel, target.Goal);
                                continue;
                            }
                            var answer = solver.SolvePanel(layer.Masks[panel].ToArray(), panel.Strikes,
                                [panel.Slots, panel.Left, panel.Right], PanelSurvey.BaseSeconds(layer.Cells));
                            if (answer.Card is CardTemplate card)
                            {
                                found.TryAdd(panel, card);
                                solver.Seed(card);
                                hit ??= Objective(panel, target.Goal);
                            }
                            else if (answer.Status != CpSolverStatus.Infeasible && hit is null && ++misses >= MissLimit)
                                break;
                        }
                    }
                    finally
                    {
                        pools.GetOrAdd(recipe.Id, _ => []).Add(solver);
                    }
                    int count = Interlocked.Increment(ref done);
                    if (count % 100 == 0)
                        Console.WriteLine($"展示层补求进度：{count}/{targets.Length}，新找到{found.Count}。");
                });
        }
        catch (Exception error) when (budget.IsCancellationRequested && PanelSolve.IsCancellation(error))
        {
            Console.WriteLine($"展示层补求预算用完或已停止：完成{done}/{targets.Length}个目标，新找到{found.Count}。");
        }
        return new(found);
    }
}
