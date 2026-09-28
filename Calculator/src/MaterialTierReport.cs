using System.Text.Json;
using System.Text.Json.Nodes;

namespace InFalsusCalc;

/// <summary>对既有完整报告应用原始惩罚与材料阶级精化，不重新搜索卡牌面板或配队。</summary>
internal static class MaterialTierReport
{
    /// <summary>读取报告、精化指定或全部模板，并在完整重映射后原子输出。</summary>
    /// <param name="args">input、output、seconds、threads和可选template参数。</param>
    /// <returns>完成0；用户取消或总预算耗尽2，取消时不写输出。</returns>
    public static int Run(string[] args)
    {
        string input = Path.Combine(Storage.Root, "reports", "report.json");
        string output = Path.Combine(Storage.Root, "results", "material-tier-report.json");
        int seconds = 28800, threads = Environment.ProcessorCount;
        string? selectedTemplate = null;
        for (int i = 0; i < args.Length; i += 2)
        {
            if (i + 1 == args.Length)
                throw new ArgumentException($"参数{args[i]}缺少值。");
            switch (args[i])
            {
                case "--input": input = Path.GetFullPath(args[i + 1]); break;
                case "--output": output = Path.GetFullPath(args[i + 1]); break;
                case "--seconds": seconds = int.Parse(args[i + 1]); break;
                case "--threads": threads = int.Parse(args[i + 1]); break;
                case "--template": selectedTemplate = args[i + 1]; break;
                default: throw new ArgumentException($"refine-materials不支持参数{args[i]}。");
            }
        }
        if (seconds is < 1 or > 86400 || threads < 1 || threads > Environment.ProcessorCount)
            throw new ArgumentException("预算应为1至86400秒，线程数应在本机逻辑CPU数量内。");

        JsonObject report = JsonNode.Parse(File.ReadAllBytes(input))?.AsObject()
            ?? throw new InvalidDataException("报告为空。");
        Catalog catalog = new();
        if (report["schema"]?.GetValue<int>() != 14 || report["catalog"]?["id"]?.GetValue<string>() != catalog.Data.Id)
            throw new InvalidDataException("报告版本或资源指纹不兼容。");
        Dictionary<string, CardTemplate> templates = report["templates"]?.Deserialize<Dictionary<string, CardTemplate>>(Storage.Json)
            ?? throw new InvalidDataException("报告缺少模板。");
        string[] requested = selectedTemplate is null ? templates.Keys.Order(StringComparer.Ordinal).ToArray()
            : templates.ContainsKey(selectedTemplate) ? [selectedTemplate]
            : throw new ArgumentException($"报告中不存在模板{selectedTemplate}。");

        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(seconds));
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        Console.CancelKeyPress += handler;
        try
        {
            Dictionary<string, CardTemplate> refined = [];
            object refinedGate = new();
            int completed = 0, changed = 0;
            // 每个求解器只用一个原生线程；总预算按并行槽位均分。每八张同配方模板复用一次预编译域，避免大配方形成串行尾部。
            double secondsPerCard = selectedTemplate is null
                ? Math.Clamp((double)seconds * threads / requested.Length, 1, MaterialTierRefinement.DefaultSecondsPerCard)
                : Math.Max(1, seconds - 5);
            var jobs = requested.GroupBy(id => templates[id].Recipe)
                .SelectMany(group => group.Order(StringComparer.Ordinal).Chunk(8)
                    .Select(ids => (Recipe: group.Key, Ids: ids)))
                .OrderByDescending(job => job.Ids.Length).ToArray();
            Parallel.ForEach(jobs, new ParallelOptions
            {
                MaxDegreeOfParallelism = threads,
                CancellationToken = cancellation.Token
            }, job =>
            {
                Recipe recipe = catalog.Data.Recipes.Single(item => item.Id == job.Recipe);
                MaterialTierRefinement refinement = new(catalog, recipe, cancellation.Token);
                foreach (string id in job.Ids)
                {
                    CardTemplate source = templates[id];
                    MaterialTierRefinement.Outcome outcome = refinement.Refine(source, secondsPerCard, 1, cancellation.Token);
                    lock (refinedGate)
                        refined[id] = outcome.Card;
                    if (outcome.Card.Id != source.Id)
                    {
                        Interlocked.Increment(ref changed);
                        Console.WriteLine($"[{source.Recipe}] {source.Name} {source.Id} → {outcome.Card.Id}："
                            + $"原始惩罚[{string.Join(',', source.RawPenalties)}] → [{string.Join(',', outcome.Card.RawPenalties)}]，"
                            + $"阶级[{string.Join(',', source.TierCounts)}] → [{string.Join(',', outcome.Card.TierCounts)}]，"
                            + $"粒子{source.Count}，{outcome.Seconds:F2}秒{(outcome.Complete ? "" : "（未闭合）")}。");
                    }
                    int done = Interlocked.Increment(ref completed);
                    if (done % 25 == 0 || done == requested.Length)
                        Console.WriteLine($"材料精化进度：{done}/{requested.Length}，已改善{Volatile.Read(ref changed)}张。");
                }
            });
            cancellation.Token.ThrowIfCancellationRequested();

            Dictionary<string, string> remap = templates.Keys.ToDictionary(id => id,
                id => refined.TryGetValue(id, out CardTemplate? card) ? card.Id : id);
            Dictionary<string, CardTemplate> outputCards = templates.Select(pair => new
            {
                Id = remap[pair.Key],
                Card = refined.TryGetValue(pair.Key, out CardTemplate? card) ? card : pair.Value
            }).GroupBy(item => item.Id, StringComparer.Ordinal).ToDictionary(group => group.Key, group => group.First().Card);
            report["templates"] = RemapTemplates(report["templates"]!.AsObject(), remap, refined);
            report["library_templates"] = RemapTemplates(report["library_templates"]!.AsObject(), remap, refined);
            RemapGroups(report["groups"]!.AsObject(), remap);

            CardTemplate[] libraryCards = report["library_templates"]!.Deserialize<Dictionary<string, CardTemplate>>(Storage.Json)!.Values
                .OrderBy(card => card.Id, StringComparer.Ordinal).ToArray();
            string library = Storage.Digest(new object[]
            {
                KeyRecipeSolver.Policy, ConfidenceAnalysis.Scope, MaterialTierRefinement.Policy,
                DeckSearch.Policy, catalog.Data.Id, libraryCards
            });
            DeckResult[] sourceDecks = report["encounters"]!.AsObject().SelectMany(encounter => encounter.Value!.AsObject()
                .Select(layer => layer.Value!.Deserialize<DeckResult>(Storage.Json)!))
                .OrderBy(deck => deck.Encounter).ThenBy(deck => deck.MaxStrikes).ToArray();
            DeckResult[] decks = sourceDecks.Select(deck => RemapDeck(catalog, deck, outputCards, remap, library, cancellation.Token)).ToArray();
            report["encounters"] = JsonSerializer.SerializeToNode(decks.GroupBy(deck => deck.Encounter)
                .ToDictionary(group => group.Key, group => group.ToDictionary(deck => deck.MaxStrikes)), Storage.Json);

            CollectionSummary previous = report["collection"]?.Deserialize<CollectionSummary>(Storage.Json)
                ?? throw new InvalidDataException("报告缺少统一制卡结果。");
            DeckCardResult[] products = decks.SelectMany(deck => deck.Cards).DistinctBy(ProductKey)
                .OrderBy(ProductKey, StringComparer.Ordinal).ToArray();
            int lower = decks.SelectMany(deck => deck.Cards).Select(card => (card.Template, card.Color)).Distinct().Count();
            CollectionSummary collection = new()
            {
                Scope = previous.Scope,
                Teams = decks.Length,
                Before = previous.Before,
                After = products.Length,
                LowerBound = lower,
                Status = previous.Status == "optimal" && lower == products.Length ? "optimal" : "best_found",
                Rejected = previous.Rejected,
                Seconds = previous.Seconds,
                Cards = products
            };
            report["collection"] = JsonSerializer.SerializeToNode(collection, Storage.Json);
            report["library"] = library;
            report["version"] = $"{KeyRecipeSolver.Policy}/{ConfidenceAnalysis.Scope}/{MaterialTierRefinement.Policy}/{DeckSearch.Policy}";
            report["created"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            cancellation.Token.ThrowIfCancellationRequested();
            Storage.Write(output, report);
            Console.WriteLine($"材料精化完成：检查{requested.Length}张，改善{changed}张，有效卡库{libraryCards.Length}张，"
                + $"统一制卡{collection.After}张；报告：{output}");
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("材料精化已取消或达到总预算，输出报告未覆盖。");
            return 2;
        }
        finally
        {
            Console.CancelKeyPress -= handler;
        }
    }

    /// <summary>序列化精化布局并合并因模板编号收敛而重合的展示目标。</summary>
    private static JsonObject RemapTemplates(JsonObject source, IReadOnlyDictionary<string, string> remap,
        IReadOnlyDictionary<string, CardTemplate> refined)
    {
        Dictionary<string, (JsonObject Node, List<string> Goals)> output = [];
        foreach ((string id, JsonNode? value) in source)
        {
            string next = remap[id];
            JsonObject node = refined.TryGetValue(id, out CardTemplate? card)
                ? JsonSerializer.SerializeToNode(card, Storage.Json)!.AsObject()
                : value!.DeepClone().AsObject();
            string[] goals = value!["goals"]?.AsArray().Select(goal => goal!.GetValue<string>()).ToArray() ?? [];
            if (!output.TryGetValue(next, out var existing))
                output[next] = (node, [.. goals]);
            else
                AppendDistinct(existing.Goals, goals);
        }
        JsonObject result = [];
        foreach ((string id, var item) in output.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            item.Node["goals"] = JsonSerializer.SerializeToNode(item.Goals, Storage.Json);
            result[id] = item.Node;
        }
        return result;
    }

    /// <summary>重映射网页分组，并在两个旧布局收敛时合并其面板目标。</summary>
    private static void RemapGroups(JsonObject groups, IReadOnlyDictionary<string, string> remap)
    {
        foreach ((_, JsonNode? recipe) in groups)
            foreach (JsonNode? group in recipe!.AsArray())
            {
                Dictionary<string, List<string>> rows = [];
                foreach (JsonNode? result in group!["results"]!.AsArray())
                {
                    string id = remap[result!["template"]!.GetValue<string>()];
                    string[] goals = result["goals"]!.AsArray().Select(goal => goal!.GetValue<string>()).ToArray();
                    if (!rows.TryGetValue(id, out List<string>? merged))
                        rows[id] = [.. goals];
                    else
                        AppendDistinct(merged, goals);
                }
                group["results"] = new JsonArray(rows.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair =>
                    JsonSerializer.SerializeToNode(new { template = pair.Key, goals = pair.Value }, Storage.Json)).ToArray());
            }
    }

    /// <summary>按首次出现顺序合并目标标签。</summary>
    private static void AppendDistinct(List<string> target, IEnumerable<string> source)
    {
        foreach (string value in source)
            if (!target.Contains(value, StringComparer.Ordinal))
                target.Add(value);
    }

    /// <summary>替换五张模板和材料清单，并核对等级1至20的原生结算结果逐字节等价。</summary>
    private static DeckResult RemapDeck(Catalog catalog, DeckResult source, IReadOnlyDictionary<string, CardTemplate> templates,
        IReadOnlyDictionary<string, string> remap, string library, CancellationToken cancellation)
    {
        DeckCardResult[] cards = source.Cards.Select(card =>
        {
            string template = remap[card.Template];
            CardTemplate definition = templates[template];
            AssignedMaterial[] materials = Craft.Assign(catalog, definition, card.Traits)
                ?? throw new InvalidDataException($"模板{card.Template}精化后无法继续承载既有技能。");
            return new DeckCardResult { Template = template, Color = card.Color, Traits = card.Traits, Materials = materials };
        }).ToArray();
        JsonObject node = JsonSerializer.SerializeToNode(source, Storage.Json)!.AsObject();
        node["library"] = library;
        node["cards"] = JsonSerializer.SerializeToNode(cards, Storage.Json);
        DeckResult result = node.Deserialize<DeckResult>(Storage.Json)!;

        Encounter encounter = catalog.Data.Encounters.Single(item => item.Id == source.Encounter);
        BattleCard[] battleCards = cards.Select(card => new DeckChoice(templates[card.Template], card.Color, card.Traits).ToBattleCard()).ToArray();
        for (int rating = 1; rating <= 20; rating++)
        {
            cancellation.ThrowIfCancellationRequested();
            BattleResult actual = new Battle(catalog, battleCards, encounter, rating).Run(true, true);
            string serialized = JsonSerializer.Serialize(actual, Storage.Json);
            if (serialized != JsonSerializer.Serialize(source.RatingBattles[rating], Storage.Json)
                || rating == source.Rating && serialized != JsonSerializer.Serialize(source.Battle, Storage.Json))
                throw new InvalidDataException($"回想{source.Encounter}/惩罚{source.MaxStrikes}/等级{rating}在材料精化后发生结算变化。");
        }
        return result;
    }

    /// <summary>成品身份由模板、固定颜色和有序技能共同组成。</summary>
    private static string ProductKey(DeckCardResult card) => $"{card.Template}:{card.Color}:{string.Join(',', card.Traits)}";
}
