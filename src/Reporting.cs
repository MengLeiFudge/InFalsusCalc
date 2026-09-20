using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InFalsusCalc;

/// <summary>把统一模板库与通关高分配置生成为纯静态成果页。</summary>
internal static class Reporting
{
    /// <summary>发布完整结果，HTML内嵌所有数据和脚本，查看时不启动计算。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="recipes">全部配方成果。</param>
    /// <param name="decks">每个回想的通关得分配置。</param>
    /// <param name="library">统一模板库指纹。</param>
    /// <param name="output">独立HTML路径。</param>
    public static void Export(Catalog catalog, RecipeResult[] recipes, DeckResult[] decks, string library, string output)
    {
        if (recipes.Length != catalog.Data.Recipes.Length || recipes.Any(r => !r.Complete)) throw new InvalidDataException("配方成果尚未全部计算完成。");
        if (decks.Length != catalog.Data.Encounters.Length || decks.Any(d => d.Library != library || d.Rating != Battle.SearchRating || !d.Chromatic
            || d.Notes != Battle.Notes || d.InitialHp != 100 || !d.Battle.Passed || d.RatingBattles.Count != 20
            || Enumerable.Range(1, 20).Any(rating => !d.RatingBattles.TryGetValue(rating, out BattleResult? battle)
                || !double.IsFinite(battle.Score) || !double.IsFinite(battle.RawScore)
                || !double.IsFinite(battle.OffensiveScore) || !double.IsFinite(battle.DefensiveScore))
            || !double.IsFinite(d.Battle.Score) || !double.IsFinite(d.Battle.OffensiveScore) || !double.IsFinite(d.Battle.DefensiveScore)))
            throw new InvalidDataException("仍有回想没有满足固定条件、通关或遭遇分字段校验，不能发布成完整结果。");
        Dictionary<string, JsonObject> templates = [];
        Dictionary<int, object[]> groups = [];
        foreach (RecipeResult recipe in recipes)
        {
            Recipe definition = catalog.Data.Recipes.Single(r => r.Id == recipe.Recipe);
            CardTemplate[] selected = ConfidenceAnalysis.SelectedCards(recipe, definition);
            foreach (CardTemplate card in selected)
            {
                JsonObject node = JsonSerializer.SerializeToNode(card, Storage.Json)!.AsObject();
                node["goals"] = new JsonArray(); templates[card.Id] = node;
            }
            List<object> visible = [];
            if (recipe.FullCover is not null)
            {
                CardTemplate card = recipe.Cards[recipe.FullCover];
                templates[card.Id]["goals"] = new JsonArray("full");
                visible.Add(new { key = card.Group, full_coverage = true, results = new[] { new { template = card.Id, goals = new[] { "full" } } } });
            }
            else
            {
                HashSet<string> retained = ConfidenceAnalysis.RetainedKeys(definition).Select(k => string.Join(',', k)).ToHashSet();
                foreach (var pair in recipe.Groups.Where(p => retained.Contains(p.Key)).OrderBy(p => p.Value.Key[0]).ThenBy(p => p.Value.Key[1]).ThenBy(p => p.Value.Key[2]))
                {
                    GroupState group = pair.Value;
                    if (group.Best.Count == 0) continue;
                    var rows = group.Best.GroupBy(item => item.Value).Select(items => new { template = items.Key, goals = items.Select(p => p.Key).ToArray() }).ToArray();
                    foreach (var row in rows) templates[row.template]["goals"] = JsonSerializer.SerializeToNode(row.goals, Storage.Json);
                    int palette = rows.SelectMany(row => recipe.Cards[row.template].Colors).Distinct().Sum(color => 1 << color);
                    visible.Add(new { key = group.Key.Concat([palette]).ToArray(), full_coverage = false, results = rows });
                }
            }
            groups[recipe.Recipe] = visible.ToArray();
        }
        if (decks.SelectMany(d => d.Cards).Any(c => !templates.ContainsKey(c.Template))) throw new InvalidDataException("配队引用了网页选择范围之外的卡牌。");
        JsonElement raw = catalog.Raw;
        var report = new
        {
            schema = 7, version = $"{KeyRecipeSolver.Policy}/{ConfidenceAnalysis.Scope}/{DeckSearch.Policy}", library, created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            catalog = new { id = catalog.Data.Id, commit = catalog.Data.Commit, traits = raw.GetProperty("traits"),
                shapes = raw.GetProperty("shapes"), encounters = raw.GetProperty("encounters"),
                recipes = catalog.Data.Recipes.Select(r => new { id = r.Id, name = r.Name }).ToArray() },
            recipes = raw.GetProperty("recipes").EnumerateArray().ToDictionary(r => r.GetProperty("Id").GetProperty("Value").GetInt32(), r => r.Clone()),
            groups, templates, encounters = decks.ToDictionary(d => d.Encounter),
            defaults = new { rating = Battle.SearchRating, rating_min = 1, rating_max = 20, search_rating = Battle.SearchRating,
                notes = 25, chromatic = true, initial_hp = 100, quality = 999 },
            meta = new { feasible_groups = groups.Values.Sum(g => g.Length), templates = templates.Count, conditions = decks.Length,
                deck_objective = $"encounter_score_capped_at_{Battle.MaxScore:0}_with_clear", deck_optimality = "ordered_name_skeleton_beam", recipe_selection = ConfidenceAnalysis.Scope,
                deck_selection = DeckSearch.Policy,
                confidence_targets = recipes.Sum(r => r.Groups.Values.SelectMany(g => g.Goals.Values).Count(v => v.Status == "CONFIDENCE")) }
        };
        Storage.Write(Path.Combine(Storage.State, "report.json"), report);
        string web = Path.Combine(Storage.Root, "Web");
        string html = File.ReadAllText(Path.Combine(web, "index.html"), Encoding.UTF8);
        string payload = JsonSerializer.Serialize(report, Storage.Json).Replace("<", "\\u003c", StringComparison.Ordinal);
        html = html.Replace("<link rel=\"stylesheet\" href=\"/style.css\">", "<style>" + File.ReadAllText(Path.Combine(web, "style.css")) + "</style>", StringComparison.Ordinal)
            .Replace("<script src=\"/app.js\" defer></script>", "<script>window.PLANNER_REPORT=" + payload + ";</script><script>" + File.ReadAllText(Path.Combine(web, "app.js")) + "</script>", StringComparison.Ordinal);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        string temporary = output + $".{Environment.ProcessId}.tmp";
        File.WriteAllText(temporary, html, new UTF8Encoding(false)); File.Move(temporary, output, true);
        Storage.Write(Path.Combine(Storage.State, "report-html.json"), new { policy = KeyRecipeSolver.Policy, selection_scope = ConfidenceAnalysis.Scope,
            deck_policy = DeckSearch.Policy, path = Path.GetFullPath(output), library, created = report.created });
    }
}
