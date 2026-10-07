using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>
/// 对指定的未决面板做一次长时限补求：每个面板独占一个求解器，全部面板并行，
/// 本机线程按面板数均分给CP-SAT（多工作线程提供不同搜索策略组合，不保证线性提速）。
/// 找到或证明不可行的结果写回state/panel-solve/progress.json；找到新布局后需重跑solve-panels收尾。
/// </summary>
internal static class PanelRecheck
{
    /// <summary>运行补求。</summary>
    /// <param name="args">--keys 面板键列表JSON文件（必填），可选--seconds单面板时限与--threads总线程数。</param>
    /// <returns>完成0；用户停止2。</returns>
    public static int Run(string[] args)
    {
        string? keysPath = null;
        int threads = Environment.ProcessorCount, seconds = 1800;
        if (args.Length % 2 != 0)
            throw new ArgumentException("recheck-panels的参数应成对给出。");
        for (int i = 0; i < args.Length; i += 2)
            switch (args[i])
            {
                case "--keys": keysPath = Path.GetFullPath(args[i + 1]); break;
                case "--seconds": seconds = int.Parse(args[i + 1]); break;
                case "--threads": threads = int.Parse(args[i + 1]); break;
                default: throw new ArgumentException($"recheck-panels不支持参数{args[i]}。");
            }
        if (keysPath is null || threads < 1 || threads > Environment.ProcessorCount || seconds < 1)
            throw new ArgumentException("需要--keys；线程数应在本机逻辑CPU数量内，时限至少1秒。");
        string[] keys = Storage.Read<string[]>(keysPath) ?? throw new InvalidDataException($"面板键列表为空：{keysPath}");

        using FileStream lease = Storage.AcquireLock();
        string cancelFile = Path.Combine(Storage.State, "cancel");
        if (File.Exists(cancelFile))
            File.Move(cancelFile, cancelFile + ".previous", true);
        using CancellationTokenSource stop = new();
        using Timer watcher = new(_ => { if (File.Exists(cancelFile)) stop.Cancel(); }, null, 250, 250);
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; stop.Cancel(); };
        Console.CancelKeyPress += handler;
        try
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
            Stopwatch clock = Stopwatch.StartNew();
            Catalog catalog = new();
            PanelSurvey.Layers[] layers = PanelSurvey.SurveyAll(catalog, threads);
            PanelSolve.Progress progress = PanelSolve.LoadProgress(catalog, PanelSolve.ProgressPath);
            var targets = keys.Select(key => layers.SelectMany(l => l.Masks.Keys.Select(p => (Layer: l, Panel: p)))
                .SingleOrDefault(x => PanelSolve.Key(x.Panel) == key)).ToArray();
            if (targets.Any(t => t.Layer is null) || keys.Any(key => !progress.Rounds.ContainsKey(key)))
                throw new InvalidDataException("面板键不在普查结果中，或不是当前未决面板。");
            int workers = Math.Max(1, threads / targets.Length);
            Console.WriteLine($"面板补求：{targets.Length}个面板并行，每个{workers}个CP-SAT工作线程，单面板时限{seconds}秒。");

            object gate = new();
            Parallel.ForEach(targets, new ParallelOptions { MaxDegreeOfParallelism = targets.Length, CancellationToken = stop.Token }, target =>
            {
                Recipe recipe = catalog.Data.Recipes.Single(r => r.Id == target.Layer.Recipe);
                KeyRecipeSolver solver = new(catalog, recipe, () => workers, seconds, stop.Token);
                solver.SeedPatternsFromCheckpoint();
                foreach (CardTemplate card in progress.Found.Values.Where(card => card.Recipe == recipe.Id))
                    solver.Seed(card);
                PanelSurvey.Panel panel = target.Panel;
                Stopwatch watch = Stopwatch.StartNew();
                var answer = solver.SolvePanel(target.Layer.Masks[panel].ToArray(), panel.Strikes,
                    [panel.Slots, panel.Left, panel.Right], seconds);
                string key = PanelSolve.Key(panel);
                lock (gate)
                {
                    if (answer.Card is CardTemplate card)
                    {
                        progress.Found[card.Id] = card;
                        progress.Rounds.Remove(key);
                    }
                    else if (answer.Status == CpSolverStatus.Infeasible)
                    {
                        progress.Impossible.Add(key);
                        progress.Rounds.Remove(key);
                    }
                    // 未找到时保留未决记录，仍按约定视为不存在。
                    if (!stop.IsCancellationRequested)
                        Storage.Write(PanelSolve.ProgressPath, progress);
                    string result = answer.Card is not null ? $"找到{answer.Card.Id}" : answer.Status == CpSolverStatus.Infeasible ? "证明不可行" : "未找到";
                    Console.WriteLine($"面板补求结果 {key}：{result}，{watch.Elapsed.TotalSeconds:F0}秒。");
                }
            });
            Console.WriteLine($"面板补求结束，{clock.Elapsed.TotalMinutes:F1}分钟。");
            return 0;
        }
        catch (Exception error) when (stop.IsCancellationRequested && PanelSolve.IsCancellation(error))
        {
            Console.WriteLine("面板补求已停止。");
            return 2;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }
}
