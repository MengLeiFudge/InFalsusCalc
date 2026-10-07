using System.Collections.Concurrent;
using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>
/// 一阶段新跑法的求解阶段：对普查得到的配队层待求解面板按轮次均分算力求布局，
/// 以检查点旧布局与本次新布局的区域拼法为起点；轮次时限逐级延长，预算用完时未找到的面板视为不存在。
/// 找到的面板令同族被支配或低于阈值的待求解面板跳过；结束后对最终面板做v3材料精化并输出。
/// 不写旧检查点；进度保存在state/panel-solve/progress.json，可用stop停止并续算。
/// </summary>
internal static class PanelSolve
{
    /// <summary>进度与输出的策略版本，改变面板分层或求解规则时递增。</summary>
    public const string Policy = "panel-solve-v1";
    /// <summary>
    /// 各轮单面板时限，秒；首轮为0表示按棋盘规模取基础时限。最后一轮结束后仍未找到的面板视为不存在。
    /// 实测第3轮（120秒）在1235个面板中只再找到7个，因此不设更长的轮次。
    /// </summary>
    private static readonly double[] RoundSeconds = [0, 30, 120, 480];

    /// <summary>面板求解输出文件；展示层求解输出沿用同一结构。</summary>
    internal static string OutputPath => Path.Combine(Storage.Root, "results", "panel-solve.json");
    /// <summary>面板求解进度文件。</summary>
    internal static string ProgressPath => Path.Combine(Storage.State, "panel-solve", "progress.json");

    /// <summary>输出中的一张卡。</summary>
    /// <param name="Fresh">本次求解新找到的布局；否则来自检查点。</param>
    /// <param name="Source">材料精化前的布局编号。</param>
    /// <param name="RefineError">材料精化复核失败的原因；此时Card为原布局。</param>
    /// <param name="Card">材料精化后的布局。</param>
    internal sealed record Entry(bool Fresh, string Source, string? RefineError, CardTemplate Card);

    /// <summary>面板求解与展示层求解共用的输出结构，只声明读取时需要核验的字段。</summary>
    internal sealed class Output
    {
        /// <summary>生成输出的策略版本。</summary>
        public string Policy { get; set; } = "";
        /// <summary>材料精化策略。</summary>
        public string MaterialPolicy { get; set; } = "";
        /// <summary>资源快照指纹。</summary>
        public string Catalog { get; set; } = "";
        /// <summary>全部卡牌。</summary>
        public Entry[] Cards { get; set; } = [];
    }

    /// <summary>读取面板求解或展示层输出，核对版本并逐张完整复算。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="path">输出文件。</param>
    /// <param name="policy">期望的策略版本。</param>
    /// <param name="command">生成该文件的命令，用于错误提示。</param>
    /// <returns>材料精化后的卡牌。</returns>
    internal static CardTemplate[] LoadCards(Catalog catalog, string path, string policy, string command)
    {
        Output output = Storage.Read<Output>(path)
            ?? throw new FileNotFoundException($"缺少{path}，请先运行{command}。", path);
        if (output.Policy != policy || output.MaterialPolicy != MaterialTierRefinement.Policy || output.Catalog != catalog.Data.Id)
            throw new InvalidDataException($"{path}的策略、材料精化策略或资源指纹与当前不一致，请重新运行{command}。");
        foreach (Entry entry in output.Cards)
        {
            Recipe recipe = catalog.Data.Recipes.Single(r => r.Id == entry.Card.Recipe);
            if (Craft.Evaluate(catalog, recipe, entry.Card.Placements).Id != entry.Card.Id)
                throw new InvalidDataException($"{path}中的布局{entry.Card.Id}无法复算。");
        }
        return [.. output.Cards.Select(entry => entry.Card)];
    }

    /// <summary>
    /// 逐张做v3材料精化。每8张同配方卡共用一次放置域预编译，避免大配方形成串行尾部；
    /// 单卡复核失败时保留原布局并记录原因。
    /// </summary>
    /// <returns>按原布局编号索引的精化结果与失败原因。</returns>
    internal static (Dictionary<string, CardTemplate> Refined, Dictionary<string, string> Errors) RefineAll(Catalog catalog,
        IEnumerable<CardTemplate> cards, int threads, CancellationToken stop)
    {
        ConcurrentDictionary<string, CardTemplate> refined = [];
        ConcurrentDictionary<string, string> errors = [];
        var jobs = cards.DistinctBy(card => card.Id).GroupBy(card => card.Recipe)
            .SelectMany(g => g.Chunk(8).Select(chunk => (Recipe: g.Key, Cards: chunk))).ToArray();
        Parallel.ForEach(jobs, new ParallelOptions { MaxDegreeOfParallelism = threads, CancellationToken = stop }, job =>
        {
            Recipe recipe = catalog.Data.Recipes.Single(r => r.Id == job.Recipe);
            MaterialTierRefinement refinement = new(catalog, recipe, stop);
            foreach (CardTemplate card in job.Cards)
                try
                {
                    refined[card.Id] = refinement.Refine(card, MaterialTierRefinement.DefaultSecondsPerCard, 1, stop).Card;
                }
                catch (InvalidDataException error)
                {
                    refined[card.Id] = card;
                    errors[card.Id] = error.Message;
                    Console.WriteLine($"[{recipe.Id}] 材料精化跳过{card.Id}：{error.Message}");
                }
        });
        return (new(refined), new(errors));
    }

    /// <summary>可续算的求解进度。</summary>
    internal sealed class Progress
    {
        /// <summary>生成进度时的求解策略。</summary>
        public string Policy { get; set; } = "";
        /// <summary>生成进度时的资源快照指纹。</summary>
        public string Catalog { get; set; } = "";
        /// <summary>本次新找到的布局，按布局编号索引。</summary>
        public Dictionary<string, CardTemplate> Found { get; set; } = [];
        /// <summary>尚未找到布局的面板已完成的轮数。</summary>
        public Dictionary<string, int> Rounds { get; set; } = [];
        /// <summary>全部区域组合均被证明不可行的面板。</summary>
        public HashSet<string> Impossible { get; set; } = [];
    }

    /// <summary>复用同配方几何预编译的求解器，并记录已同步的新布局数。</summary>
    private sealed class Lease(KeyRecipeSolver solver)
    {
        /// <summary>独占使用期间的求解器实例。</summary>
        public KeyRecipeSolver Solver { get; } = solver;
        /// <summary>已作为起点加入的本配方新布局数量。</summary>
        public int Seen { get; set; }
    }

    /// <summary>面板在进度文件中的稳定键。</summary>
    internal static string Key(PanelSurvey.Panel p) => $"{p.Recipe}:{p.Strikes}:{p.Slots},{p.Left},{p.Right}:{p.Power}/{p.Fortitude}";

    /// <summary>布局对应的配队面板；0槽卡的范围不影响配队，统一归零，与普查口径一致。</summary>
    internal static PanelSurvey.Panel Of(CardTemplate card) => card.Slots == 0
        ? new(card.Recipe, card.Strikes, 0, 0, 0, card.Power, card.Fortitude)
        : new(card.Recipe, card.Strikes, card.Slots, card.Left, card.Right, card.Power, card.Fortitude);

    /// <summary>安全支配：攻、防、槽位都不差且至少一项更好。</summary>
    private static bool Dominates(PanelSurvey.Panel a, PanelSurvey.Panel b) =>
        a.Power >= b.Power && a.Fortitude >= b.Fortitude && a.Slots >= b.Slots
        && (a.Power > b.Power || a.Fortitude > b.Fortitude || a.Slots > b.Slots);

    /// <summary>运行面板求解，结束后对最终配队层新布局做材料精化并输出。</summary>
    /// <param name="args">可选--threads与--seconds（求解阶段总墙钟预算，秒）。</param>
    /// <returns>全部面板有结论为0；预算不足或用户停止为2。</returns>
    public static int Run(string[] args)
    {
        int threads = Environment.ProcessorCount, seconds = 43200;
        if (args.Length % 2 != 0)
            throw new ArgumentException("solve-panels的参数应成对给出。");
        for (int i = 0; i < args.Length; i += 2)
            switch (args[i])
            {
                case "--threads": threads = int.Parse(args[i + 1]); break;
                case "--seconds": seconds = int.Parse(args[i + 1]); break;
                default: throw new ArgumentException($"solve-panels不支持参数{args[i]}。");
            }
        if (threads < 1 || threads > Environment.ProcessorCount || seconds < 60)
            throw new ArgumentException("线程数应在本机逻辑CPU数量内，预算至少60秒。");
        using FileStream lease = Storage.AcquireLock();
        string cancelFile = Path.Combine(Storage.State, "cancel");
        if (File.Exists(cancelFile))
            File.Move(cancelFile, cancelFile + ".previous", true);
        // stop为用户停止；budget另含求解预算，到期只结束求解阶段，仍执行材料精化与输出。
        using CancellationTokenSource stop = new();
        using CancellationTokenSource budget = CancellationTokenSource.CreateLinkedTokenSource(stop.Token);
        budget.CancelAfter(TimeSpan.FromSeconds(seconds));
        using Timer watcher = new(_ => { if (File.Exists(cancelFile)) stop.Cancel(); }, null, 250, 250);
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; stop.Cancel(); };
        Console.CancelKeyPress += handler;
        try
        {
            // 长时间满载运行时让出前台响应，与完整计算管线一致。
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            Stopwatch clock = Stopwatch.StartNew();
            Catalog catalog = new();
            PanelSurvey.Layers[] layers = PanelSurvey.SurveyAll(catalog, threads);
            PanelSurvey.Plan plan = PanelSurvey.Classify(layers);
            string progressPath = ProgressPath;
            Progress progress = LoadProgress(catalog, progressPath);
            Console.WriteLine($"面板求解：待求解{plan.Open.Count}个面板，{threads}线程，求解预算{seconds / 3600.0:F1}小时。");
            bool settled = Solve(catalog, layers, plan, progress, progressPath, threads, seconds, clock, budget.Token,
                () => stop.IsCancellationRequested);
            stop.Token.ThrowIfCancellationRequested();
            Finish(catalog, layers, progress, threads, clock, stop.Token);
            return settled ? 0 : 2;
        }
        catch (Exception error) when (stop.IsCancellationRequested && IsCancellation(error))
        {
            Console.WriteLine("面板求解已停止，进度已保存；用相同命令续算。");
            return 2;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }

    /// <summary>判断异常是否只由取消引起，包括并行循环包装的取消。</summary>
    internal static bool IsCancellation(Exception error) => error is OperationCanceledException
        || error is AggregateException aggregate && aggregate.Flatten().InnerExceptions.All(inner => inner is OperationCanceledException);

    /// <summary>读取与当前策略和资源一致的进度，并逐张复算已找到的布局。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="path">进度文件路径。</param>
    /// <returns>可续算进度；版本不一致时为新的空进度。</returns>
    internal static Progress LoadProgress(Catalog catalog, string path)
    {
        Progress? saved = Storage.Read<Progress>(path);
        if (saved is null || saved.Policy != Policy || saved.Catalog != catalog.Data.Id)
            return new() { Policy = Policy, Catalog = catalog.Data.Id };
        foreach (CardTemplate card in saved.Found.Values)
        {
            Recipe recipe = catalog.Data.Recipes.Single(r => r.Id == card.Recipe);
            if (Craft.Evaluate(catalog, recipe, card.Placements).Id != card.Id)
                throw new InvalidDataException($"面板求解进度中的布局{card.Id}无法复算：{path}");
        }
        Console.WriteLine($"面板求解续算：已找到{saved.Found.Count}，已证不可行{saved.Impossible.Count}，未决{saved.Rounds.Count}。");
        return saved;
    }

    /// <summary>
    /// 同族已知可行面板的索引：检查点已有布局的面板加本次新找到的面板。
    /// 族为同名卡、同净惩罚、同左右范围，与普查分层一致；只在调用方持有锁时访问。
    /// </summary>
    private sealed class KnownPanels(Dictionary<int, PanelSurvey.Layers> byRecipe)
    {
        private readonly Dictionary<(int, int, int, int), List<PanelSurvey.Panel>> families = [];

        private (int, int, int, int) Family(PanelSurvey.Panel p) => (byRecipe[p.Recipe].BaseId, p.Strikes, p.Left, p.Right);

        /// <summary>登记一个已确认可行的面板。</summary>
        public void Add(PanelSurvey.Panel panel)
        {
            if (!families.TryGetValue(Family(panel), out var list))
                families[Family(panel)] = list = [];
            list.Add(panel);
        }

        /// <summary>面板已找到、被同族可行面板安全支配，或各目标加权分都低于同族最高分的阈值比例时无需求解。</summary>
        public bool Covers(PanelSurvey.Panel panel)
        {
            if (!families.TryGetValue(Family(panel), out var list) || list.Count == 0)
                return false;
            if (list.Any(known => known.Equals(panel) || Dominates(known, panel)))
                return true;
            return Enumerable.Range(0, 3).All(goal =>
                PanelSurvey.Score(panel, goal) < list.Max(known => PanelSurvey.Score(known, goal)) * PanelSurvey.Threshold);
        }
    }

    /// <summary>
    /// 按轮次求解待求解面板。每轮给所有未决面板相同时限，轮次时限逐级延长；
    /// 同族高分面板优先，找到后令被支配或低于阈值的同族面板直接跳过。
    /// </summary>
    /// <returns>全部待求解面板都已找到、证明不可行或被覆盖时为真。</returns>
    private static bool Solve(Catalog catalog, PanelSurvey.Layers[] layers, PanelSurvey.Plan plan, Progress progress,
        string progressPath, int threads, int seconds, Stopwatch clock, CancellationToken budget, Func<bool> stopRequested)
    {
        Dictionary<int, PanelSurvey.Layers> byRecipe = layers.ToDictionary(l => l.Recipe);
        object gate = new();
        KnownPanels known = new(byRecipe);
        foreach (PanelSurvey.Panel panel in plan.Deck)
            known.Add(panel);
        HashSet<string> foundKeys = [];
        Dictionary<int, List<CardTemplate>> foundByRecipe = [];
        void Register(CardTemplate card)
        {
            known.Add(Of(card));
            foundKeys.Add(Key(Of(card)));
            if (!foundByRecipe.TryGetValue(card.Recipe, out var list))
                foundByRecipe[card.Recipe] = list = [];
            list.Add(card);
        }
        foreach (CardTemplate card in progress.Found.Values)
            Register(card);

        // 每个配方的求解器池：几何预编译只做一次，实例在线程间轮流独占使用。
        ConcurrentDictionary<int, ConcurrentBag<Lease>> pools = [];
        Lease Take(Recipe recipe)
        {
            if (pools.GetOrAdd(recipe.Id, _ => []).TryTake(out Lease? lease))
                return lease;
            KeyRecipeSolver solver = new(catalog, recipe, () => 1, 1, budget);
            solver.SeedPatternsFromCheckpoint();
            return new(solver);
        }

        // 按配方集中处理，限制同时存活的求解器实例；配方内按加权分从高到低，使高面板先被求解并覆盖低面板。
        PanelSurvey.Panel[] ordered = plan.Open.OrderBy(p => p.Recipe)
            .ThenByDescending(p => Enumerable.Range(0, 3).Max(goal => PanelSurvey.Score(p, goal)))
            .ThenBy(p => p.Strikes).ToArray();
        DateTime lastSave = DateTime.UtcNow;
        void SaveProgress(bool force)
        {
            if (!force && DateTime.UtcNow - lastSave < TimeSpan.FromSeconds(30))
                return;
            Storage.Write(progressPath, progress);
            lastSave = DateTime.UtcNow;
        }

        // 去掉已被覆盖面板的未决轮次记录，使未决数只统计真正仍需判断的面板。
        void PruneCovered()
        {
            foreach (PanelSurvey.Panel p in ordered)
                if (progress.Rounds.ContainsKey(Key(p)) && known.Covers(p))
                    progress.Rounds.Remove(Key(p));
        }

        for (int round = 0; round < RoundSeconds.Length; round++)
        {
            PanelSurvey.Panel[] pending;
            lock (gate)
            {
                PruneCovered();
                pending = ordered.Where(p => !foundKeys.Contains(Key(p)) && !progress.Impossible.Contains(Key(p))
                    && progress.Rounds.GetValueOrDefault(Key(p)) <= round && !known.Covers(p)).ToArray();
            }
            if (pending.Length == 0)
            {
                bool settled;
                lock (gate)
                    settled = ordered.All(p => foundKeys.Contains(Key(p)) || progress.Impossible.Contains(Key(p)) || known.Covers(p));
                if (settled)
                {
                    lock (gate)
                        SaveProgress(true);
                    Console.WriteLine($"面板求解完成：新找到{progress.Found.Count}，已证不可行{progress.Impossible.Count}，{clock.Elapsed.TotalHours:F2}小时。");
                    return true;
                }
                continue;
            }
            double limit = RoundSeconds[Math.Min(round, RoundSeconds.Length - 1)];
            Console.WriteLine($"第{round + 1}轮：{pending.Length}个面板，单面板时限{(limit == 0 ? "按棋盘规模" : $"{limit}秒")}，"
                + $"已用{clock.Elapsed.TotalHours:F2}/{seconds / 3600.0:F1}小时。");
            int done = 0, hits = 0;
            // 本轮各配方剩余面板数；归零时释放该配方的求解器池。
            Dictionary<int, int> remaining = pending.GroupBy(p => p.Recipe).ToDictionary(g => g.Key, g => g.Count());
            void Release(int recipeId)
            {
                lock (gate)
                    if (--remaining[recipeId] == 0)
                        pools.TryRemove(recipeId, out _);
            }
            try
            {
                Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(pending, EnumerablePartitionerOptions.NoBuffering),
                    new ParallelOptions { MaxDegreeOfParallelism = threads, CancellationToken = budget }, panel =>
                    {
                        bool covered;
                        lock (gate)
                            covered = known.Covers(panel);
                        if (covered)
                        {
                            Release(panel.Recipe);
                            return;
                        }
                        PanelSurvey.Layers layer = byRecipe[panel.Recipe];
                        Recipe recipe = catalog.Data.Recipes.Single(r => r.Id == panel.Recipe);
                        Lease lease = Take(recipe);
                        try
                        {
                            lock (gate)
                            {
                                // 同步其他线程找到的本配方新布局，作为本次求解的额外起点。
                                List<CardTemplate> news = foundByRecipe.GetValueOrDefault(recipe.Id) ?? [];
                                for (; lease.Seen < news.Count; lease.Seen++)
                                    lease.Solver.Seed(news[lease.Seen]);
                            }
                            double seconds = limit == 0 ? PanelSurvey.BaseSeconds(layer.Cells) : limit;
                            var answer = lease.Solver.SolvePanel(layer.Masks[panel].ToArray(), panel.Strikes,
                                [panel.Slots, panel.Left, panel.Right], seconds);
                            lock (gate)
                            {
                                string key = Key(panel);
                                if (answer.Card is CardTemplate card)
                                {
                                    progress.Found[card.Id] = card;
                                    progress.Rounds.Remove(key);
                                    Register(card);
                                    foundKeys.Add(key);
                                    hits++;
                                }
                                else if (answer.Status == CpSolverStatus.Infeasible)
                                {
                                    progress.Impossible.Add(key);
                                    progress.Rounds.Remove(key);
                                }
                                // 被停止或预算截断的求解没有用满本轮时限，不记为已完成本轮，续算时重做。
                                else if (!budget.IsCancellationRequested)
                                    progress.Rounds[key] = round + 1;
                                if (++done % 200 == 0)
                                    Console.WriteLine($"第{round + 1}轮进度：{done}/{pending.Length}，本轮找到{hits}，累计找到{progress.Found.Count}，"
                                        + $"{clock.Elapsed.TotalHours:F2}小时。");
                                SaveProgress(false);
                            }
                        }
                        finally
                        {
                            pools.GetOrAdd(recipe.Id, _ => []).Add(lease);
                            Release(recipe.Id);
                        }
                    });
            }
            catch (Exception error) when (budget.IsCancellationRequested && IsCancellation(error))
            {
                lock (gate)
                    SaveProgress(true);
                Console.WriteLine($"面板求解{(stopRequested() ? "已停止" : "预算用完")}：新找到{progress.Found.Count}，已证不可行{progress.Impossible.Count}，未决{progress.Rounds.Count}。");
                return false;
            }
            lock (gate)
                SaveProgress(true);
            Console.WriteLine($"第{round + 1}轮结束：本轮找到{hits}，累计找到{progress.Found.Count}，{clock.Elapsed.TotalHours:F2}小时。");
        }
        lock (gate)
        {
            PruneCovered();
            SaveProgress(true);
        }
        Console.WriteLine($"全部{RoundSeconds.Length}轮结束：新找到{progress.Found.Count}，已证不可行{progress.Impossible.Count}，"
            + $"未找到{progress.Rounds.Count}（视为不存在），{clock.Elapsed.TotalHours:F2}小时。");
        return true;
    }

    /// <summary>
    /// 汇总最终配队层：检查点已有布局与本次新布局合并后，按族去掉被安全支配或低于阈值的面板；
    /// 全部布局（含检查点旧布局）逐张做v3材料精化，输出results/panel-solve.json。
    /// 检查点保存的是求解原始布局，多数从未精化，故与新布局按同一策略处理；source记录精化前的布局编号。
    /// </summary>
    private static void Finish(Catalog catalog, PanelSurvey.Layers[] layers, Progress progress, int threads, Stopwatch clock,
        CancellationToken stop)
    {
        Dictionary<int, PanelSurvey.Layers> byRecipe = layers.ToDictionary(l => l.Recipe);
        List<(CardTemplate Card, bool Fresh)> cards = [];
        foreach (Recipe recipe in catalog.Data.Recipes)
            foreach (CardTemplate card in KeyRecipeSolver.Load(catalog, recipe)?.Cards.Values ?? Enumerable.Empty<CardTemplate>())
                cards.Add((card, false));
        cards.AddRange(progress.Found.Values.Select(card => (card, true)));

        // 每个面板保留一张布局，旧布局优先，保证与现有卡库的编号连续。
        var panels = cards.GroupBy(item => Of(item.Card)).Select(g => g.OrderBy(item => item.Fresh).First()).ToArray();
        List<(CardTemplate Card, bool Fresh)> front = [];
        foreach (var family in panels.GroupBy(item => (byRecipe[item.Card.Recipe].BaseId, Of(item.Card).Strikes, Of(item.Card).Left, Of(item.Card).Right)))
        {
            var members = family.ToArray();
            double[] best = [.. Enumerable.Range(0, 3).Select(goal => members.Max(m => PanelSurvey.Score(Of(m.Card), goal)))];
            front.AddRange(members.Where(m => !members.Any(other => Dominates(Of(other.Card), Of(m.Card)))
                && Enumerable.Range(0, 3).Any(goal => PanelSurvey.Score(Of(m.Card), goal) >= best[goal] * PanelSurvey.Threshold)));
        }

        Console.WriteLine($"最终配队层{front.Count}个面板，其中新面板{front.Count(item => item.Fresh)}个；开始材料精化。");
        var (refined, refineErrors) = RefineAll(catalog, front.Select(item => item.Card), threads, stop);

        string output = OutputPath;
        Storage.Write(output, new
        {
            policy = Policy,
            material_policy = MaterialTierRefinement.Policy,
            catalog = catalog.Data.Id,
            hours = clock.Elapsed.TotalHours,
            found = progress.Found.Count,
            impossible = progress.Impossible.Count,
            unresolved = progress.Rounds.Count,
            cards = front.Select(item => new
            {
                fresh = item.Fresh,
                source = item.Card.Id,
                refine_error = refineErrors.GetValueOrDefault(item.Card.Id),
                card = refined[item.Card.Id]
            }).OrderBy(x => x.card.Recipe).ThenBy(x => x.card.Id, StringComparer.Ordinal).ToArray()
        });
        Console.WriteLine($"面板求解输出：{output}，总计{clock.Elapsed.TotalHours:F2}小时。");
    }
}
