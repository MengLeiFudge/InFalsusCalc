using System.Text.Json;
using System.Text.Json.Nodes;

namespace InFalsusCalc;

/// <summary>仅对既有完整报告执行跨回想制卡合并，不启动制卡几何或高分配队搜索。</summary>
internal static class CollectionReport
{
    /// <summary>读取兼容报告，联合优化全部惩罚档并原子输出；默认另存文件保留输入。</summary>
    /// <param name="args">可选input、output、seconds、threads参数，各参数均带双短横线。</param>
    /// <returns>完成0；用户取消2，取消时不覆盖输出。</returns>
    public static int Run(string[] args)
    {
        string input = Path.Combine(Storage.Root, "reports", "report.json");
        string output = Path.Combine(Storage.Root, "results", "collection-report.json");
        int seconds = 28800, threads = Environment.ProcessorCount;
        for (int i = 0; i < args.Length; i += 2)
        {
            if (i + 1 == args.Length) throw new ArgumentException($"参数{args[i]}缺少值。");
            switch (args[i])
            {
                case "--input": input = Path.GetFullPath(args[i + 1]); break;
                case "--output": output = Path.GetFullPath(args[i + 1]); break;
                case "--seconds": seconds = int.Parse(args[i + 1]); break;
                case "--threads": threads = int.Parse(args[i + 1]); break;
                default: throw new ArgumentException($"collect不支持参数{args[i]}。");
            }
        }
        if (seconds is < 1 or > 86400 || threads < 1 || threads > Environment.ProcessorCount)
            throw new ArgumentException("预算应为1至86400秒，线程数应在本机逻辑CPU数量内。");
        JsonObject report = JsonNode.Parse(File.ReadAllBytes(input))?.AsObject()
            ?? throw new InvalidDataException("报告为空。");
        Catalog catalog = new();
        if (report["schema"]?.GetValue<int>() != 14 || report["catalog"]?["id"]?.GetValue<string>() != catalog.Data.Id)
            throw new InvalidDataException("报告版本或资源指纹不兼容。");
        Dictionary<string, CardTemplate> templates = report["library_templates"]?.Deserialize<Dictionary<string, CardTemplate>>(Storage.Json)
            ?? throw new InvalidDataException("报告缺少有效卡库。");
        DeckResult[] decks = report["encounters"]!.AsObject().SelectMany(e => e.Value!.AsObject()
            .Select(t => t.Value!.Deserialize<DeckResult>(Storage.Json)!))
            .OrderBy(d => d.Encounter).ThenBy(d => d.MaxStrikes).ToArray();
        string library = report["library"]!.GetValue<string>();
        if (decks.Any(d => d.Library != library)) throw new InvalidDataException("配队指纹与报告不一致。");
        using CancellationTokenSource cancellation = new();
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        Console.CancelKeyPress += handler;
        try
        {
            var result = new DeckCollection(catalog, templates, cancellation.Token).Optimize(decks, seconds, threads);
            report["encounters"] = JsonSerializer.SerializeToNode(result.Decks.GroupBy(d => d.Encounter)
                .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.MaxStrikes)), Storage.Json);
            report["collection"] = JsonSerializer.SerializeToNode(result.Summary, Storage.Json);
            report["created"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            cancellation.Token.ThrowIfCancellationRequested();
            Storage.Write(output, report);
            CollectionSummary summary = result.Summary;
            Console.WriteLine($"统一制卡完成：{summary.Before} → {summary.After}张，下界{summary.LowerBound}，{summary.Status}；报告：{output}");
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("统一制卡已取消，输出报告未覆盖。");
            return 2;
        }
        finally { Console.CancelKeyPress -= handler; }
    }
}
