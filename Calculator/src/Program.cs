using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace InFalsusCalc;

/// <summary>Windows原生计算入口，数学与展示分离，不依赖Python或WSL运行。</summary>
internal static class Program
{
    /// <summary>解析命令，计算结束或中断时返回明确状态。</summary>
    /// <param name="args">普通运行参数，或debug、status、stop控制命令。</param>
    /// <returns>完成0、错误1、保存后暂停2。</returns>
    private static int Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        try
        {
            string? command = args.FirstOrDefault();
            if (command == "status")
            {
                if (args.Length != 1)
                    throw new ArgumentException("status不接受参数。");
                string path = Path.Combine(Storage.State, "status.json");
                Console.WriteLine(File.Exists(path) ? File.ReadAllText(path) : "尚未开始计算。");
                return 0;
            }
            if (command == "stop")
            {
                if (args.Length != 1)
                    throw new ArgumentException("stop不接受参数。");
                Directory.CreateDirectory(Storage.State);
                File.WriteAllText(Path.Combine(Storage.State, "cancel"), "user-request");
                Console.WriteLine("已请求停止，程序将在当前求解返回后保存退出。");
                return 0;
            }
            if (command == "debug")
            {
                if (args.Length < 2)
                    throw new ArgumentException("调试命令为debug run或debug confidence。");
                if (args[1] == "confidence")
                {
                    if (args.Length != 2)
                        throw new ArgumentException("debug confidence不接受其他参数。");
                    ConfidenceAnalysis.Run(new Catalog());
                    return 0;
                }
                if (args[1] == "run")
                    return RunPipeline(Parse(args, 2, true));
                throw new ArgumentException("调试命令为debug run或debug confidence。");
            }
            if (command is not null && !command.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("直接运行程序即可完整计算；另有status、stop和debug命令。");
            return RunPipeline(Parse(args, 0, false));
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error.ToString());
            return 1;
        }
    }

    /// <summary>仅提供命令行工程参数；网页没有求解配置界面。</summary>
    /// <param name="args">完整命令行参数。</param>
    /// <param name="start">第一个参数选项的位置。</param>
    /// <param name="advanced">是否允许内部定位参数。</param>
    /// <returns>已检查范围的计算参数。</returns>
    private static Options Parse(string[] args, int start, bool advanced)
    {
        Options options = new();
        for (int i = start; i < args.Length; i++)
        {
            string name = args[i];
            if (name == "--prove-confidence")
            {
                options.ProveConfidence = true;
                continue;
            }
            if (i + 1 >= args.Length)
                throw new ArgumentException($"参数{name}缺少值。");
            string value = args[++i];
            switch (name)
            {
                case "--threads":
                    options.Threads = int.Parse(value);
                    break;
                case "--seconds":
                    options.Seconds = int.Parse(value);
                    break;
                case "--output" when advanced:
                    options.Output = Path.GetFullPath(value);
                    break;
                case "--recipe-seconds" when advanced:
                    options.RecipeSeconds = int.Parse(value);
                    break;
                case "--key" when advanced:
                    options.Key = value.Split(',').Select(int.Parse).ToArray();
                    break;
                case "--goal" when advanced:
                    options.Goal = value;
                    break;
                case "--recipe" when advanced:
                    options.Recipe = int.Parse(value);
                    break;
                case "--encounter" when advanced:
                    options.Encounter = int.Parse(value);
                    break;
                case "--slice-seconds" when advanced:
                    options.DirectionSeconds = int.Parse(value);
                    break;
                default:
                    throw new ArgumentException(advanced
                    ? $"未知调试参数{name}。"
                    : $"普通运行仅支持--prove-confidence、--threads和--seconds；{name}请放在debug run后使用。");
            }
        }
        if (options.Threads < 1 || options.Threads > Environment.ProcessorCount)
            throw new ArgumentException($"线程数应为1～{Environment.ProcessorCount}。");
        if (options.RecipeSeconds < 0 || options.RecipeSeconds > 86400)
            throw new ArgumentException("单卡预算应为0～86400秒，0表示仅使用总预算。");
        if (options.Seconds < 1 || options.Seconds > 86400)
            throw new ArgumentException("每个普通计算阶段的预算应为1～86400秒；prove-confidence制卡阶段不使用该截止时间。");
        if (options.DirectionSeconds < 1 || options.DirectionSeconds > 3600)
            throw new ArgumentException("求解时间片应为1～3600秒。");
        if (options.Key is not null && (options.Key.Length != 3 || options.Key[0] < 0 || options.Key[0] > 3 || options.Key.Skip(1).Any(x => x < 0 || x > 4)))
            throw new ArgumentException("--key格式为槽数,左范围,右范围，槽数0～3，范围0～4。");
        if (options.Goal is not null && options.Goal is not ("power" or "fortitude" or "total"))
            throw new ArgumentException("--goal为power、fortitude或total。");
        return options;
    }

    /// <summary>完整执行制卡、配队和结果导出；阶段参数不改变后续阶段是否执行。</summary>
    /// <param name="options">计算范围和工程预算。</param>
    /// <returns>完成0、保存后暂停2。</returns>
    private static int RunPipeline(Options options)
    {
        int craft = CraftKeys(options);
        return craft == 0 ? Compute(options) : craft;
    }

    /// <summary>运行固定key制卡器并保存检查点；成功后由完整管线继续配队。</summary>
    /// <param name="options">可选配方、key、目标和搜索预算。</param>
    /// <returns>全部目标完成为0，存在未决为2。</returns>
    private static int CraftKeys(Options options)
    {
        using FileStream lease = Storage.AcquireLock();
        using CancellationTokenSource cancel = new();
        if (!options.ProveConfidence)
            cancel.CancelAfter(TimeSpan.FromSeconds(options.Seconds));
        string stopFile = Path.Combine(Storage.State, "cancel");
        if (File.Exists(stopFile))
            File.Move(stopFile, stopFile + ".previous", true);
        using Timer watcher = new(_ => { if (File.Exists(stopFile)) cancel.Cancel(); }, null, 250, 250);
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; cancel.Cancel(); };
        Console.CancelKeyPress += handler;
        Action? publishFinal = null;
        try
        {
            Catalog catalog = new();
            Recipe[] recipes = catalog.Data.Recipes.Where(r => options.Recipe is null || r.Id == options.Recipe).ToArray();
            if (recipes.Length == 0)
                throw new ArgumentException("配方编号不存在。");
            foreach (Recipe recipe in recipes)
            {
                KeyRecipeSolver.MigrateExclusions(catalog, recipe);
                ConfidenceAnalysis.Migrate(catalog, recipe);
                KeyRecipeSolver.MigrateStackLimit(catalog, recipe);
                KeyRecipeSolver.RestoreBounds(catalog, recipe);
            }
            object statusGate = new();
            Stopwatch elapsed = Stopwatch.StartNew();
            ConcurrentDictionary<int, (Stopwatch Clock, string Name)> active = new();
            ConcurrentDictionary<int, KeyRecipeSolver> solvers = new();
            ConcurrentDictionary<int, double> spent = new();
            ConcurrentDictionary<int, string> errors = new();
            string[] goals = options.Goal is null ? ["power", "fortitude", "total"] : [options.Goal];
            string phase = "未尝试目标首轮";
            int[][] Keys(Recipe recipe) => options.Key is null ? ConfidenceAnalysis.RetainedKeys(recipe) : [options.Key];
            bool Needed(Recipe recipe, bool retry)
            {
                RecipeResult? saved = KeyRecipeSolver.Load(catalog, recipe);
                if (options.ProveConfidence)
                {
                    if (!retry || saved is null)
                        return false;
                    return !errors.ContainsKey(recipe.Id) && Keys(recipe).Any(k => ConfidenceAnalysis.RetainedStrikes.Any(strikes => goals.Any(g =>
                        saved.Groups.GetValueOrDefault(KeyRecipeSolver.GroupKey(string.Join(',', k), strikes))?.Goals.GetValueOrDefault(g)?.Status == "CONFIDENCE")));
                }
                return !errors.ContainsKey(recipe.Id) && Keys(recipe).Any(k => ConfidenceAnalysis.RetainedStrikes.Any(strikes => goals.Any(g =>
                    !KeyRecipeSolver.Done(saved, KeyRecipeSolver.GroupKey(string.Join(',', k), strikes), g) &&
                    retry == KeyRecipeSolver.Attempted(saved, KeyRecipeSolver.GroupKey(string.Join(',', k), strikes), g))));
            }
            void Publish()
            {
                lock (statusGate)
                {
                    var rows = recipes.Select(recipe =>
                    {
                        RecipeResult? saved = KeyRecipeSolver.Load(catalog, recipe);
                        int[][] keys = Keys(recipe);
                        string[] states = keys.SelectMany(k => ConfidenceAnalysis.RetainedStrikes.SelectMany(strikes => goals.Select(g =>
                        {
                            string group = KeyRecipeSolver.GroupKey(string.Join(',', k), strikes);
                            return !KeyRecipeSolver.Done(saved, group, g) && !KeyRecipeSolver.Attempted(saved, group, g)
                                ? "MISSING" : saved!.Groups[group].Goals[g].Status;
                        }))).ToArray();
                        bool running = active.TryGetValue(recipe.Id, out var job);
                        return new
                        {
                            recipe = recipe.Id,
                            name = recipe.Name,
                            excluded_areas = saved?.ExcludedAreas ?? [],
                            keys = keys.Length,
                            total = states.Length,
                            optimal = states.Count(s => s == "OPTIMAL"),
                            confidence = states.Count(s => s == "CONFIDENCE"),
                            infeasible = states.Count(s => s == "INFEASIBLE"),
                            unknown = states.Count(s => s == "UNKNOWN"),
                            missing = states.Count(s => s == "MISSING"),
                            complete = keys.All(k => ConfidenceAnalysis.RetainedStrikes.All(strikes => goals.All(g =>
                                options.ProveConfidence
                                    ? saved?.Groups.GetValueOrDefault(KeyRecipeSolver.GroupKey(string.Join(',', k), strikes))?.Goals.GetValueOrDefault(g)?.Status is "OPTIMAL" or "INFEASIBLE"
                                    : KeyRecipeSolver.Done(saved, KeyRecipeSolver.GroupKey(string.Join(',', k), strikes), g)))),
                            running,
                            seconds = spent.GetValueOrDefault(recipe.Id) + (running ? job.Clock.Elapsed.TotalSeconds : 0),
                            error = errors.GetValueOrDefault(recipe.Id)
                        };
                    }).ToArray();
                    var jobs = active.OrderBy(p => p.Key).Select(p =>
                    {
                        KeyRecipeSolver? solver = solvers.GetValueOrDefault(p.Key);
                        return new
                        {
                            recipe = p.Key,
                            name = p.Value.Name,
                            seconds = p.Value.Clock.Elapsed.TotalSeconds,
                            goal_seconds = solver?.GoalSeconds ?? 0,
                            progress = solver?.Progress ?? "几何预编译"
                        };
                    }).ToArray();
                    Console.WriteLine($"[状态 {elapsed.Elapsed:hh\\:mm\\:ss}] {phase}；完整卡{rows.Count(r => r.complete)}/{rows.Length}，已完成{rows.Sum(r => r.optimal + r.confidence + r.infeasible)}/{rows.Sum(r => r.total)}（精确{rows.Sum(r => r.optimal + r.infeasible)}，置信{rows.Sum(r => r.confidence)}），未决{rows.Sum(r => r.unknown)}，未尝试{rows.Sum(r => r.missing)}，活动卡{jobs.Length}。");
                    foreach (var job in jobs)
                        Console.WriteLine($"  [{job.recipe}] {job.name} {job.progress}，当前目标{job.goal_seconds:F0}秒，本阶段{job.seconds:F0}秒。");
                    Storage.Write(Path.Combine(Storage.State, "key-survey.json"), new
                    {
                        updated = DateTimeOffset.UtcNow,
                        policy = KeyRecipeSolver.Policy,
                        snapshot = catalog.Data.Id,
                        pid = Environment.ProcessId,
                        phase,
                        elapsed_seconds = elapsed.Elapsed.TotalSeconds,
                        threads = options.Threads,
                        slice_seconds = options.DirectionSeconds,
                        active = jobs,
                        recipes = rows
                    });
                }
            }
            publishFinal = () => { phase = cancel.IsCancellationRequested ? "已暂停" : "本轮结束"; Publish(); };
            using Timer reporter = new(_ =>
            {
                try
                {
                    Publish();
                }
                catch (Exception ex) { Console.WriteLine($"状态保存失败：{ex.Message}"); cancel.Cancel(); }
            }, null, 5000, 5000);
            Console.WriteLine(options.ProveConfidence
                ? $"置信度公平续算：总线程预算{options.Threads}；每个目标一个{options.DirectionSeconds:0.###}秒完整模型时间片；每5秒输出状态。"
                : $"并行续算：总线程预算{options.Threads}；先全局首轮，再无时限重试；每5秒输出状态。");
            Publish();
            int fairRound = 0;
            do
            {
                fairRound++;
                bool[] passes = options.ProveConfidence ? [true] : [false, true];
                foreach (bool retry in passes)
                {
                    phase = retry ? options.ProveConfidence ? $"置信度公平轮次{fairRound}" : "无时限重试" : "未尝试目标首轮";
                    Recipe[] work = recipes.Where(r => Needed(r, retry)).ToArray();
                    if (retry)
                        work = work.Select(r => (Recipe: r, Saved: KeyRecipeSolver.Load(catalog, r)))
                            .OrderByDescending(p => Keys(p.Recipe).Sum(k => ConfidenceAnalysis.RetainedStrikes.Sum(strikes => goals.Count(g =>
                                !KeyRecipeSolver.Done(p.Saved, KeyRecipeSolver.GroupKey(string.Join(',', k), strikes), g)))))
                            .ThenBy(p => p.Recipe.Id).Select(p => p.Recipe).ToArray();
                    int parallelism = options.ProveConfidence ? options.Threads : retry ? Math.Min(4, options.Threads) : options.Threads;
                    ConcurrentDictionary<int, byte> pending = new(work.Select(r => new KeyValuePair<int, byte>(r.Id, 0)));
                    Console.WriteLine($"进入{phase}：{work.Length}张卡，最多并行{parallelism}张。");
                    Parallel.ForEach(Partitioner.Create(work, EnumerablePartitionerOptions.NoBuffering), new ParallelOptions { MaxDegreeOfParallelism = parallelism, CancellationToken = cancel.Token }, recipe =>
                    {
                        Stopwatch clock = Stopwatch.StartNew();
                        active[recipe.Id] = (clock, recipe.Name);
                        KeyRecipeSolver? current = null;
                        try
                        {
                            int ThreadBudget()
                            {
                                int[] remaining = pending.Keys.Order().ToArray();
                                if (remaining.Length >= parallelism)
                                    return Math.Max(1, options.Threads / parallelism);
                                int count = Math.Max(1, remaining.Length);
                                return options.Threads / count + (Array.IndexOf(remaining, recipe.Id) < options.Threads % count ? 1 : 0);
                            }
                            using CancellationTokenSource cardCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel.Token);
                            if (!retry && options.RecipeSeconds > 0)
                                cardCancel.CancelAfter(TimeSpan.FromSeconds(options.RecipeSeconds));
                            Console.WriteLine($"[{recipe.Id}] {recipe.Name} 开始{phase}，当前线程份额{ThreadBudget()}。");
                            current = new KeyRecipeSolver(catalog, recipe, ThreadBudget, options.DirectionSeconds, cardCancel.Token, options.ProveConfidence);
                            solvers[recipe.Id] = current;
                            current.Run(options.Key, options.Goal, retry);
                        }
                        catch (OperationCanceledException) { current?.Save(); Console.WriteLine($"[{recipe.Id}] 已保存暂停。"); }
                        catch (Exception ex) { current?.Save(); errors[recipe.Id] = ex.Message; Console.WriteLine($"[{recipe.Id}] 失败：{ex.Message}"); }
                        finally
                        {
                            spent.AddOrUpdate(recipe.Id, clock.Elapsed.TotalSeconds, (_, prior) => prior + clock.Elapsed.TotalSeconds);
                            active.TryRemove(recipe.Id, out _);
                            solvers.TryRemove(recipe.Id, out _);
                            pending.TryRemove(recipe.Id, out _);
                        }
                    });
                    Publish();
                    if (!retry && recipes.Any(r => Needed(r, false)))
                    {
                        Console.WriteLine("首轮预算内仍有未尝试目标，已保存；本轮不提前进入重试。");
                        return 2;
                    }
                }
                if (!options.ProveConfidence || !errors.IsEmpty || !recipes.Any(recipe => Needed(recipe, true)))
                    break;
                Console.WriteLine($"置信度公平轮次{fairRound}完成；保存后继续下一轮，直到全部闭合或收到停止请求。");
            } while (!cancel.IsCancellationRequested);
            phase = "本轮结束";
            Publish();
            return errors.IsEmpty && recipes.All(r => !Needed(r, false) && !Needed(r, true)) ? 0 : 2;
        }
        catch (OperationCanceledException) { Console.WriteLine("各并行任务已保存固定key计算进度。"); return 2; }
        finally { Console.CancelKeyPress -= handler; publishFinal?.Invoke(); }
    }

    /// <summary>批量生成真实配方和通关得分成果，完成后才替换结果JSON。</summary>
    /// <param name="options">资源位置、线程和总预算。</param>
    /// <returns>全部完成0，预算或用户停止2。</returns>
    private static int Compute(Options options)
    {
        using FileStream lease = Storage.AcquireLock();
        string cancel = Path.Combine(Storage.State, "cancel");
        if (File.Exists(cancel))
            File.Move(cancel, cancel + ".previous", true);
        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(options.Seconds));
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        Console.CancelKeyPress += handler;
        using Timer watcher = new(_ => { if (File.Exists(cancel)) cancellation.Cancel(); }, null, 250, 250);
        Stopwatch elapsed = Stopwatch.StartNew();
        DateTimeOffset started = DateTimeOffset.UtcNow;
        List<RecipeResult> results = [];
        ConcurrentDictionary<(int Encounter, int MaxStrikes), DeckResult> decks = [];
        string stage = "读取并核验游戏资源";
        object statusGate = new();
        string? library = null;
        // 在一个锁中保存阶段和进度，避免并行回想覆盖状态文件。
        void SaveStatus()
        {
            lock (statusGate)
            {
                Storage.Write(Path.Combine(Storage.State, "status.json"), new
                {
                    policy = KeyRecipeSolver.Policy,
                    selection_scope = ConfidenceAnalysis.Scope,
                    pid = Environment.ProcessId,
                    started,
                    updated = DateTimeOffset.UtcNow,
                    elapsed_seconds = elapsed.Elapsed.TotalSeconds,
                    deadline_seconds = options.Seconds,
                    threads = options.Threads,
                    stage,
                    recipes = results.Count,
                    completed_recipes = results.Count(r => r.Complete),
                    completed_encounters = decks.Keys.Select(k => k.Encounter).Distinct().Count(),
                    completed_decks = decks.Count,
                    library,
                    output = options.Output
                });
            }
        }
        try
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            SaveStatus();
            Console.WriteLine($"C#计算已启动，使用{options.Threads}个求解线程；Ctrl+C可保存后停止。");
            Catalog catalog = new();
            Recipe[] selectedRecipes = catalog.Data.Recipes;
            stage = "读取置信key配方库";
            SaveStatus();
            foreach (Recipe recipe in selectedRecipes)
            {
                cancellation.Token.ThrowIfCancellationRequested();
                KeyRecipeSolver.MigrateExclusions(catalog, recipe);
                ConfidenceAnalysis.Migrate(catalog, recipe);
                KeyRecipeSolver.RestoreBounds(catalog, recipe);
                RecipeResult result = KeyRecipeSolver.Load(catalog, recipe) ?? throw new InvalidDataException($"配方{recipe.Id}缺少兼容的置信key检查点。");
                if (!result.Complete)
                    throw new InvalidDataException($"配方{recipe.Id}的置信key尚未完成。");
                results.Add(result);
            }
            CardTemplate[] templates = results.Zip(selectedRecipes).SelectMany(p => ConfidenceAnalysis.SelectedCards(p.First, p.Second, catalog.Data.Profiles)).DistinctBy(c => c.Id).OrderBy(c => c.Id).ToArray();
            if (templates.Select(c => c.BaseId).Distinct().Count() < 5)
                throw new InvalidDataException("结果不足五种不同名卡。");
            Dictionary<(int Encounter, int MaxStrikes), DeckResult> incumbents = LoadIncumbents();
            Dictionary<string, CardTemplate> byTemplate = templates.ToDictionary(c => c.Id);
            library = Storage.Digest(new object[] { KeyRecipeSolver.Policy, ConfidenceAnalysis.Scope, DeckSearch.Policy, catalog.Data.Id, templates });
            stage = "通关约束下的遭遇得分搜索";
            SaveStatus();
            string directory = Path.Combine(Storage.State, "decks", library);
            ConcurrentBag<string> missing = [];
            Encounter[] selectedEncounters = catalog.Data.Encounters;
            if (selectedEncounters.Length == 0)
                throw new InvalidDataException("资源中没有回想。");
            if (options.Encounter is int requestedEncounter && !selectedEncounters.Any(e => e.Id == requestedEncounter))
                throw new ArgumentException("回想编号不存在。");
            Parallel.ForEach(selectedEncounters, new ParallelOptions { MaxDegreeOfParallelism = options.Threads, CancellationToken = cancellation.Token }, encounter =>
            {
                DeckResult? lowerResult = null;
                foreach (int maxStrikes in ConfidenceAnalysis.RetainedStrikes)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    string file = Path.Combine(directory, $"{encounter.Id:000}-p{maxStrikes}.json");
                    DeckResult? cached = Storage.Read<DeckResult>(file);
                    bool cacheValid = cached is not null && cached.Library == library && cached.Encounter == encounter.Id && cached.MaxStrikes == maxStrikes
                        && cached.Rating == Battle.SearchRating && cached.Battle.Passed && cached.RatingBattles.Count == 20
                        && cached.Cards.All(c => byTemplate.TryGetValue(c.Template, out CardTemplate? card) && card.Strikes <= maxStrikes);
                    bool CachedBeatsLower() => lowerResult is null || cached!.Battle.Score > lowerResult.Battle.Score
                        || cached.Battle.Score == lowerResult.Battle.Score && cached.Battle.RawScore >= lowerResult.Battle.RawScore;
                    if (cacheValid && (cached!.NeighborhoodComplete || cached.Battle.Score >= Battle.MaxScore)
                        && CachedBeatsLower() && options.Encounter != encounter.Id)
                    {
                        decks[(encounter.Id, maxStrikes)] = cached!;
                        lowerResult = cached;
                        Console.WriteLine($"{encounter.Name}（允许罚分{maxStrikes}）：复用已有搜索结果，遭遇分{cached!.Battle.Score:N0}。");
                        SaveStatus();
                        continue;
                    }
                    DeckResult? incumbent = incumbents.GetValueOrDefault((encounter.Id, maxStrikes))
                        ?? incumbents.GetValueOrDefault((encounter.Id, -1));
                    DeckChoice[]? Team(DeckResult? source) => source is not null
                        && source.Cards.All(c => byTemplate.TryGetValue(c.Template, out CardTemplate? card) && card.Strikes <= maxStrikes)
                        ? source.Cards.Select(c => new DeckChoice(byTemplate[c.Template], c.Color, c.Traits)).ToArray() : null;
                    DeckResult[] baselines = new DeckResult?[] { lowerResult, cacheValid ? cached : null, incumbent }
                        .Where(x => Team(x) is not null).Select(x => x!).Distinct().ToArray();
                    DeckChoice[][] seeds = baselines.Select(Team).Select(x => x!).ToArray();
                    DeckResult? result = new DeckSearch(catalog, templates, encounter, library, maxStrikes, cancellation.Token).Run(seeds);
                    foreach (DeckResult baseline in baselines)
                    {
                        DeckChoice[] baselineTeam = Team(baseline)!;
                        BattleCard[] oldCards = baselineTeam.Select(c => c.ToBattleCard()).ToArray();
                        Dictionary<int, BattleResult> oldRatings = Enumerable.Range(1, 20)
                            .ToDictionary(rating => rating, rating => new Battle(catalog, oldCards, encounter, rating).Run(true, true));
                        BattleResult oldBattle = oldRatings[Battle.SearchRating];
                        if (oldBattle.Passed && (result is null || oldBattle.Score > result.Battle.Score
                            || oldBattle.Score == result.Battle.Score && oldBattle.RawScore > result.Battle.RawScore))
                            result = new DeckResult
                            {
                                Library = library,
                                Encounter = encounter.Id,
                                MaxStrikes = maxStrikes,
                                Cards = baseline.Cards,
                                Battle = oldBattle,
                                RatingBattles = oldRatings,
                                Evaluated = result?.Evaluated ?? 1,
                                Seconds = result?.Seconds ?? 0,
                                Method = $"允许罚分{maxStrikes}保留已有更高分配队；{DeckSearch.Policy}未超过基准"
                            };
                    }
                    if (result is null)
                    {
                        missing.Add($"{encounter.Name}（允许罚分{maxStrikes}）");
                        Console.WriteLine($"{encounter.Name}（允许罚分{maxStrikes}）：当前搜索尚未找到通关配置。");
                    }
                    else
                    {
                        Storage.Write(file, result);
                        decks[(encounter.Id, maxStrikes)] = result;
                        lowerResult = result;
                        Console.WriteLine($"{encounter.Name}（允许罚分{maxStrikes}）：通关，遭遇分{result.Battle.Score:N0}，比较{result.Evaluated}组，耗时{result.Seconds:F2}秒。");
                    }
                    SaveStatus();
                }
            });
            if (!missing.IsEmpty)
            {
                stage = "仍有回想未找到通关配置，已保留其余成果";
                SaveStatus();
                Console.WriteLine(string.Join("、", missing));
                return 2;
            }
            cancellation.Token.ThrowIfCancellationRequested();
            stage = "导出完整计算结果";
            SaveStatus();
            Reporting.Export(catalog, results.ToArray(), decks.Values.OrderBy(d => d.Encounter).ThenBy(d => d.MaxStrikes).ToArray(), library, options.Output);
            stage = "completed";
            SaveStatus();
            Console.WriteLine($"全部成果已生成：{options.Output}");
            return 0;
        }
        catch (OperationCanceledException)
        {
            stage = "paused";
            SaveStatus();
            Console.WriteLine("已保存配队进度并停止，完整结果JSON未覆盖。");
            return 2;
        }
        catch
        {
            stage = "failed";
            SaveStatus();
            throw;
        }
        finally { Console.CancelKeyPress -= handler; }
    }

    /// <summary>合并本地结果与已发布报告的高分队伍，避免旧运行文件遮住更新的发布基准。</summary>
    /// <returns>按回想与允许罚分索引的已有配队。</returns>
    private static Dictionary<(int Encounter, int MaxStrikes), DeckResult> LoadIncumbents()
    {
        List<DeckResult> results = [];
        foreach (string directory in new[] { "results", "reports" })
        {
            string path = Path.Combine(Storage.Root, directory, "report.json");
            if (!File.Exists(path))
                continue;
            using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(path));
            if (!document.RootElement.TryGetProperty("encounters", out JsonElement encounters))
                continue;
            foreach (JsonProperty encounter in encounters.EnumerateObject())
            {
                if (encounter.Value.TryGetProperty("cards", out _))
                {
                    DeckResult? result = encounter.Value.Deserialize<DeckResult>(Storage.Json);
                    if (result is not null)
                        results.Add(result);
                }
                else
                    foreach (JsonProperty tier in encounter.Value.EnumerateObject())
                    {
                        DeckResult? result = tier.Value.Deserialize<DeckResult>(Storage.Json);
                        if (result is not null)
                            results.Add(result);
                    }
            }
        }
        return results.GroupBy(result => (result.Encounter, result.MaxStrikes))
            .ToDictionary(group => group.Key, group => group.MaxBy(result => result.Battle.Score)!);
    }

    /// <summary>Windows原生入口的工程参数，战斗等级等条件不可调整。</summary>
    private sealed class Options
    {
        /// <summary>完整计算结果JSON的本地输出位置。</summary>
        public string Output { get; set; } = Path.Combine(Storage.Root, "results", "report.json");
        /// <summary>默认使用本机可用的全部逻辑CPU。</summary>
        public int Threads { get; set; } = Environment.ProcessorCount;
        /// <summary>仅计算指定配方；为空时生成完整成果。</summary>
        public int? Recipe
        {
            get; set;
        }
        /// <summary>强制重算指定回想；为空时复用缓存或补齐全部31回想。</summary>
        public int? Encounter
        {
            get; set;
        }
        /// <summary>只计算指定的槽位、左右范围三元组。</summary>
        public int[]? Key
        {
            get; set;
        }
        /// <summary>只计算指定面板目标，空值计算三个目标。</summary>
        public string? Goal
        {
            get; set;
        }
        /// <summary>每个内外方向的求解时间片。</summary>
        public int DirectionSeconds { get; set; } = 23;
        /// <summary>单卡首轮总预算，0表示仅使用阶段总预算。</summary>
        public int RecipeSeconds
        {
            get; set;
        }
        /// <summary>不限时继续证明CONFIDENCE上界。</summary>
        public bool ProveConfidence
        {
            get; set;
        }
        /// <summary>整次运行预算；craft默认0不设上限，compute默认8小时。</summary>
        public int Seconds { get; set; } = 28800;
    }
}
