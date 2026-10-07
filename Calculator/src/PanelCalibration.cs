using System.Collections.Concurrent;
using System.Diagnostics;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>
/// 面板求解时限校准：从检查点已有布局的配队层面板中按棋盘规模抽样，记录找到合法布局的用时。
/// --seeded 时用检查点旧布局作起点，但排除实现样本面板本身的布局（留一法），模拟求解新面板的真实条件。
/// 只读检查点，输出results/panel-calibration[-seeded].json。
/// </summary>
internal static class PanelCalibration
{
    /// <summary>每个棋盘规模档抽样的面板数。</summary>
    private const int PerBucket = 15;
    /// <summary>单个样本的求解上限，秒；远大于预期找到用时，超时样本单独计数。</summary>
    private const double SampleSeconds = 60;

    /// <summary>运行校准并打印各档用时分布。</summary>
    /// <param name="args">可选--threads与--seeded。</param>
    /// <returns>完成0。</returns>
    public static int Run(string[] args)
    {
        int threads = Environment.ProcessorCount;
        bool seeded = false;
        for (int i = 0; i < args.Length; i++)
            if (args[i] == "--seeded")
                seeded = true;
            else if (args[i] == "--threads" && i + 1 < args.Length)
                threads = int.Parse(args[++i]);
            else
                throw new ArgumentException($"calibrate-panels不支持参数{args[i]}。");
        Catalog catalog = new();
        PanelSurvey.Layers[] layers = PanelSurvey.SurveyAll(catalog, threads);
        PanelSurvey.Plan plan = PanelSurvey.Classify(layers);
        Dictionary<int, PanelSurvey.Layers> byRecipe = layers.ToDictionary(l => l.Recipe);

        // 固定种子抽样，保证重复运行可比；按基础时限档分组即按棋盘规模分组。
        Random random = new(20260929);
        var samples = plan.Deck.GroupBy(p => PanelSurvey.BaseSeconds(byRecipe[p.Recipe].Cells))
            .SelectMany(bucket => bucket.OrderBy(p => (p.Recipe, p.Strikes, p.Slots, p.Left, p.Right, p.Power, p.Fortitude))
                .OrderBy(_ => random.Next()).Take(PerBucket).Select(p => (Bucket: bucket.Key, Panel: p)))
            .ToArray();
        Console.WriteLine($"时限校准：{samples.Length}个样本，每个上限{SampleSeconds}秒，并行{threads}。");

        ConcurrentBag<(double Bucket, int Recipe, int Cells, double Seconds, string Status)> rows = [];
        Parallel.ForEach(samples, new ParallelOptions { MaxDegreeOfParallelism = threads }, sample =>
        {
            Recipe recipe = catalog.Data.Recipes.Single(r => r.Id == sample.Panel.Recipe);
            // 留一法：起点排除面板相同的布局，其余旧布局的区域拼法都可复用。
            KeyRecipeSolver solver = new(catalog, recipe, () => 1, SampleSeconds, CancellationToken.None);
            PanelSurvey.Panel panel = sample.Panel;
            if (seeded)
                solver.SeedPatternsFromCheckpoint(card => card.Strikes == panel.Strikes && card.Power == panel.Power
                    && card.Fortitude == panel.Fortitude && card.Slots == panel.Slots);
            uint[] masks = byRecipe[recipe.Id].Masks[sample.Panel].ToArray();
            Stopwatch clock = Stopwatch.StartNew();
            var answer = solver.SolvePanel(masks, sample.Panel.Strikes, [sample.Panel.Slots, sample.Panel.Left, sample.Panel.Right]);
            string status = answer.Card is not null ? "found" : answer.Status == CpSolverStatus.Infeasible ? "infeasible" : "timeout";
            rows.Add((sample.Bucket, recipe.Id, byRecipe[recipe.Id].Cells, clock.Elapsed.TotalSeconds, status));
            Console.WriteLine($"[{recipe.Id}] 样本{status}，{clock.Elapsed.TotalSeconds:F2}秒。");
        });

        var buckets = rows.GroupBy(r => r.Bucket).OrderBy(g => g.Key).Select(g =>
        {
            double[] found = g.Where(r => r.Status == "found").Select(r => r.Seconds).Order().ToArray();
            return new
            {
                base_seconds = g.Key,
                samples = g.Count(),
                found = found.Length,
                timeout = g.Count(r => r.Status == "timeout"),
                infeasible = g.Count(r => r.Status == "infeasible"),
                median = found.Length == 0 ? 0 : found[found.Length / 2],
                max = found.DefaultIfEmpty(0).Max()
            };
        }).ToArray();
        string output = Path.Combine(Storage.Root, "results", seeded ? "panel-calibration-seeded.json" : "panel-calibration.json");
        Storage.Write(output, new { per_bucket = PerBucket, sample_seconds = SampleSeconds, buckets, rows = rows.OrderBy(r => r.Bucket).ThenBy(r => r.Seconds).ToArray() });
        foreach (var b in buckets)
            Console.WriteLine($"基础{b.base_seconds}秒档：样本{b.samples}，找到{b.found}，超时{b.timeout}，不可行{b.infeasible}；找到用时中位{b.median:F2}秒，最长{b.max:F2}秒。");
        Console.WriteLine($"报告：{output}");
        return 0;
    }
}
