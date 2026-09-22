using System.Text.Json;
using System.Text.Json.Nodes;

namespace InFalsusCalc;

/// <summary>把统一模板库与通关高分配置导出为网页可读取的结果数据。</summary>
internal static class Reporting
{
    /// <summary>按用户给出的起始回想和游戏列表顺序返回章节。</summary>
    /// <param name="encounter">当前回想及其零基顺序。</param>
    /// <returns>章节1、章节2、章节2.5、章节3或章节4。</returns>
    private static string Chapter(Encounter encounter) => encounter.Order switch
    {
        < 5 => "章节1",
        < 12 => "章节2",
        < 14 => "章节2.5",
        < 22 => "章节3",
        _ => "章节4"
    };

    /// <summary>返回游戏TraitIconAssets为该技能组合选用的效果图标。</summary>
    /// <param name="trait">包含一个或多个原生效果的技能。</param>
    /// <returns>网页图片清单中的效果图标键。</returns>
    private static string TraitIcon(SkillTrait trait)
    {
        int[] kinds = trait.Effects.Select(effect => effect.Kind).Distinct().Order().ToArray();
        return kinds switch
        {
            [8096] => "burst",
            [8097] => "recover",
            [1] => "amplification",
            [2] => "aegis",
            [1, 2] => "rigor",
            [128] => "weaken",
            [129] => "vulnerable",
            [128, 129] => "turmoil",
            [4] => "chromatic-isolation",
            [5] => "critical-focus",
            [1, 3] => "impudence",
            _ => throw new InvalidDataException($"技能{trait.Id}没有对应的游戏效果图标。")
        };
    }

    /// <summary>读取只用于页面展示的完整游戏掉落快照并核对版本和回想范围。</summary>
    /// <param name="catalog">当前求解资源快照。</param>
    /// <returns>按回想编号索引的完整掉落表。</returns>
    private static IReadOnlyDictionary<int, EncounterLoot> EncounterLoot(Catalog catalog)
    {
        string path = Path.Combine(Storage.Input, "encounter-loot.json");
        EncounterLootSnapshot snapshot = Storage.Read<EncounterLootSnapshot>(path)
            ?? throw new FileNotFoundException("缺少完整回想掉落展示快照。", path);
        int[] expected = catalog.Data.Encounters.Select(encounter => encounter.Id).Order().ToArray();
        int[] actual = snapshot.Encounters.Select(encounter => encounter.Id).Order().ToArray();
        if (snapshot.Schema != 1 || snapshot.Commit != catalog.Data.Commit || !actual.SequenceEqual(expected)
            || snapshot.Encounters.Any(encounter => encounter.TotalWeight <= 0 || encounter.TraitTotalWeight <= 0 || encounter.Drops.Length == 0))
            throw new InvalidDataException("完整回想掉落展示快照与当前资源不一致。");
        return snapshot.Encounters.ToDictionary(encounter => encounter.Id);
    }

    /// <summary>列出当前回想完整掉落表中的粒子、权重、效能和技能池。</summary>
    /// <param name="catalog">提供粒子颜色、阶级和形状。</param>
    /// <param name="loot">目标回想的完整游戏掉落表。</param>
    /// <returns>供成果页展示的完整掉落数据。</returns>
    private static object Drops(Catalog catalog, EncounterLoot loot)
    {
        var traits = loot.Traits.GroupBy(item => item.Id)
            .Select(group => new { id = group.Key, weight = group.Sum(item => item.Weight) }).ToArray();
        int noTraitWeight = Math.Max(0, loot.TraitTotalWeight - traits.Sum(item => item.weight));
        var items = loot.Drops.Select(drop =>
        {
            Shape shape = catalog.Shapes.TryGetValue(drop.Shape, out Shape? found)
                ? found : throw new InvalidDataException($"掉落表引用了不存在的粒子{drop.Shape}。");
            return new
            {
                shape = shape.Id,
                color = shape.Color,
                tier = shape.Tier,
                size = shape.Cells.Length,
                weight = drop.Weight,
                min_potency = drop.MinPotency,
                max_potency = drop.MaxPotency,
                trait_rolls = drop.TraitRolls
            };
        }).ToArray();
        return new
        {
            table = loot.Table,
            total_weight = loot.TotalWeight,
            trait_total_weight = loot.TraitTotalWeight,
            no_trait_weight = noTraitWeight,
            traits,
            items
        };
    }

    /// <summary>判断放宽罚分后的配队结果是否低于上一档。</summary>
    /// <param name="next">允许更多罚分的结果。</param>
    /// <param name="previous">上一档结果。</param>
    /// <returns>封顶分更低，或封顶分相同但原始分更低时为true。</returns>
    private static bool WorseThan(DeckResult next, DeckResult previous) => next.Battle.Score < previous.Battle.Score
        || next.Battle.Score == previous.Battle.Score && next.Battle.RawScore < previous.Battle.RawScore;

    /// <summary>按同组全部已知候选归并字典序单项代表，并展开最大总和的全部面板拆分。</summary>
    private static KeyValuePair<string, string>[] GoalRepresentatives(RecipeResult recipe, GroupState group)
    {
        CardTemplate[] cards = group.Best.Values.Concat(group.TotalBest).Distinct().Where(recipe.Cards.ContainsKey).Select(id => recipe.Cards[id]).ToArray();
        if (cards.Length == 0)
            return [];
        List<KeyValuePair<string, string>> result = [];
        foreach (string goal in group.Best.Keys.Where(goal => goal != "total"))
            result.Add(new(goal, Craft.BestForGoal(cards, goal).Id));
        if (group.Best.ContainsKey("total"))
        {
            int total = cards.Max(card => card.Total);
            foreach (CardTemplate card in cards.Where(card => card.Total == total).GroupBy(card => (card.Power, card.Fortitude))
                .Select(panel => Craft.BestForGoal(panel, "total")).OrderByDescending(card => card.Power).ThenByDescending(card => card.Fortitude))
                result.Add(new("total", card.Id));
        }
        return result.ToArray();
    }

    /// <summary>导出完整计算结果，不读取或生成网页资源。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="recipes">全部配方成果。</param>
    /// <param name="decks">每个回想的通关得分配置。</param>
    /// <param name="library">统一模板库指纹。</param>
    /// <param name="output">完整结果JSON路径。</param>
    public static void Export(Catalog catalog, RecipeResult[] recipes, DeckResult[] decks, string library, string output)
    {
        if (recipes.Length != catalog.Data.Recipes.Length || recipes.Any(r => !r.Complete))
            throw new InvalidDataException("配方成果尚未全部计算完成。");
        int[] strikeLimits = ConfidenceAnalysis.RetainedStrikes;
        if (decks.Length != catalog.Data.Encounters.Length * strikeLimits.Length
            || !decks.Select(d => d.Encounter).Distinct().Order().SequenceEqual(catalog.Data.Encounters.Select(e => e.Id).Order())
            || decks.GroupBy(d => d.Encounter).Any(group =>
            {
                DeckResult[] ordered = group.OrderBy(d => d.MaxStrikes).ToArray();
                return !ordered.Select(d => d.MaxStrikes).SequenceEqual(strikeLimits)
                    || ordered.Zip(ordered.Skip(1)).Any(pair => WorseThan(pair.Second, pair.First));
            })
            || decks.Any(d => d.Library != library || d.Rating != Battle.SearchRating || !d.Chromatic
            || d.Notes != Battle.Notes || d.InitialHp != 100 || !d.Battle.Passed || d.RatingBattles.Count != 20
            || Enumerable.Range(1, 20).Any(rating => !d.RatingBattles.TryGetValue(rating, out BattleResult? battle)
                || !double.IsFinite(battle.Score) || !double.IsFinite(battle.RawScore)
                || !double.IsFinite(battle.OffensiveScore) || !double.IsFinite(battle.DefensiveScore))
            || !double.IsFinite(d.Battle.Score) || !double.IsFinite(d.Battle.OffensiveScore) || !double.IsFinite(d.Battle.DefensiveScore)))
            throw new InvalidDataException("仍有回想没有满足固定条件、通关或遭遇分字段校验，不能发布成完整结果。");
        Dictionary<string, JsonObject> templates = [];
        Dictionary<string, JsonObject> libraryTemplates = [];
        Dictionary<int, object[]> groups = [];
        foreach (RecipeResult recipe in recipes)
        {
            Recipe definition = catalog.Data.Recipes.Single(r => r.Id == recipe.Recipe);
            CardTemplate[] selected = ConfidenceAnalysis.SelectedCards(recipe, definition);
            HashSet<string> retained = ConfidenceAnalysis.RetainedKeys(definition).Select(k => string.Join(',', k)).ToHashSet();
            GroupState[] retainedGroups = recipe.Groups.Values.Where(g => retained.Contains(string.Join(',', g.Key))).ToArray();
            string[] visibleIds = retainedGroups.SelectMany(g => GoalRepresentatives(recipe, g).Select(item => item.Value)).Distinct().ToArray();
            HashSet<string> selectedIds = selected.Select(card => card.Id).ToHashSet();
            foreach (CardTemplate card in selected.Concat(visibleIds.Select(id => recipe.Cards[id])).DistinctBy(c => c.Id))
            {
                JsonObject node = JsonSerializer.SerializeToNode(card, Storage.Json)!.AsObject();
                node["goals"] = new JsonArray();
                templates[card.Id] = node;
                if (selectedIds.Contains(card.Id))
                    libraryTemplates[card.Id] = node.DeepClone().AsObject();
            }
            List<object> visible = [];
            foreach (var pair in recipe.Groups.Where(p => retained.Contains(string.Join(',', p.Value.Key)))
                .OrderBy(p => p.Value.Key[0]).ThenBy(p => p.Value.Key[1]).ThenBy(p => p.Value.Key[2]).ThenBy(p => p.Value.Strikes))
            {
                GroupState group = pair.Value;
                KeyValuePair<string, string>[] representatives = GoalRepresentatives(recipe, group);
                if (representatives.Length == 0)
                    continue;
                var rows = representatives.GroupBy(item => item.Value).Select(items => new { template = items.Key, goals = items.Select(p => p.Key).Distinct().ToArray() }).ToArray();
                foreach (var row in rows)
                    templates[row.template]["goals"] = JsonSerializer.SerializeToNode(row.goals, Storage.Json);
                int palette = rows.SelectMany(row => recipe.Cards[row.template].Colors).Distinct().Sum(color => 1 << color);
                visible.Add(new
                {
                    key = group.Key.Concat([palette]).ToArray(),
                    strikes = group.Strikes,
                    results = rows
                });
            }
            groups[recipe.Recipe] = visible.ToArray();
        }
        if (decks.SelectMany(deck => deck.Cards.Select(card => (deck, card))).Any(item => !templates.TryGetValue(item.card.Template, out JsonObject? template)
            || template["strikes"]!.GetValue<int>() > item.deck.MaxStrikes))
            throw new InvalidDataException("配队引用了网页选择范围之外或超过允许罚分的卡牌。");
        JsonElement raw = catalog.Raw;
        IReadOnlyDictionary<int, EncounterLoot> loot = EncounterLoot(catalog);
        var report = new
        {
            schema = 14,
            version = $"{KeyRecipeSolver.Policy}/{ConfidenceAnalysis.Scope}/{DeckSearch.Policy}",
            library,
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            catalog = new
            {
                id = catalog.Data.Id,
                commit = catalog.Data.Commit,
                traits = catalog.Data.Traits.Select(trait => new
                {
                    id = trait.Id,
                    name = trait.Name,
                    description = trait.Description,
                    tier = trait.Tier,
                    effects = trait.Effects,
                    icon = TraitIcon(trait)
                }).ToArray(),
                shapes = raw.GetProperty("shapes"),
                encounters = catalog.Data.Encounters.Select(encounter => new
                {
                    id = encounter.Id,
                    name = encounter.Name,
                    mode = encounter.Mode,
                    cards = encounter.Cards,
                    order = encounter.Order,
                    chapter = Chapter(encounter),
                    drops = Drops(catalog, loot[encounter.Id])
                }).ToArray(),
                recipes = catalog.Data.Recipes.Select(r => new { id = r.Id, name = r.Name, tier = r.Tier, color = r.BaseColor, is_sr = r.IsSr }).ToArray()
            },
            recipes = raw.GetProperty("recipes").EnumerateArray().ToDictionary(r => r.GetProperty("Id").GetProperty("Value").GetInt32(), r => r.Clone()),
            groups,
            templates,
            library_templates = libraryTemplates,
            encounters = decks.GroupBy(d => d.Encounter)
                .ToDictionary(group => group.Key, group => group.ToDictionary(deck => deck.MaxStrikes)),
            defaults = new
            {
                rating = Battle.SearchRating,
                rating_min = 1,
                rating_max = 20,
                search_rating = Battle.SearchRating,
                notes = 25,
                chromatic = true,
                initial_hp = 100,
                quality = 999
            },
            meta = new
            {
                feasible_groups = groups.Values.Sum(g => g.Length),
                templates = libraryTemplates.Count,
                published_templates = templates.Count,
                conditions = decks.Length,
                deck_objective = $"encounter_score_capped_at_{Battle.MaxScore:0}_with_clear",
                deck_optimality = "ordered_name_skeleton_beam",
                recipe_selection = ConfidenceAnalysis.Scope,
                deck_selection = DeckSearch.Policy,
                confidence_targets = recipes.Sum(r => r.Groups.Values.SelectMany(g => g.Goals.Values).Count(v => v.Status == "CONFIDENCE"))
            }
        };
        Storage.Write(output, report);
    }
}
