using System.Collections.Concurrent;
using System.Diagnostics;

namespace InFalsusCalc;

/// <summary>在几何求解前按卡牌定位、范围和技能槽收益生成通用置信度筛选报告。</summary>
internal static class ConfidenceAnalysis
{
    public const string Scope = "confidence-key-penalty-v3";
    /// <summary>正式保留的净惩罚层；三次惩罚会令面板归零。</summary>
    public static readonly int[] RetainedStrikes = [0, 1, 2];
    private static readonly ConcurrentDictionary<int, ConfidenceRecipe> ConfidenceCache = new();
    private sealed record BenchmarkKey(string Key, bool Passed, double Value, double Score, int Position, int[] Traits);
    private sealed record BenchmarkEncounter(int Encounter, string Name, BenchmarkKey[] Keys);
    private readonly record struct Panel(int Power, int Fortitude);
    private sealed record ConfidenceKey(string Key, int Power, int Fortitude, int Total, double PowerScore, double FortitudeScore, double TotalScore,
        double PowerRatio, double FortitudeRatio, double TotalRatio, double Confidence, string Band);
    private sealed record ConfidenceRecipe(int Recipe, string Name, ConfidenceKey[] Keys, string[] High, string[] Reserve, string[] Pruned,
        string[] AtLeastThree, string[] AtLeastFour);

    /// <summary>只返回置信策略选中key的代表卡，不带入保留在文件中的低置信历史组。</summary>
    public static CardTemplate[] SelectedCards(RecipeResult result, Recipe recipe)
    {
        HashSet<string> keys = RetainedKeys(recipe).Select(Key).ToHashSet();
        CardTemplate[] cards = result.Groups.Where(p => keys.Contains(Key(p.Value.Key)) && RetainedStrikes.Contains(p.Value.Strikes))
            .SelectMany(p => p.Value.Best.Values.Concat(p.Value.TotalBest)).Distinct()
            .Where(result.Cards.ContainsKey).Select(id => result.Cards[id]).ToArray();
        return cards.Where(card => !cards.Any(other => other.Id != card.Id && Dominates(other, card))).ToArray();
    }

    /// <summary>同一卡同结构只保留最终攻防未被支配的候选。</summary>
    private static bool Dominates(CardTemplate candidate, CardTemplate other)
    {
        bool sameStructure = candidate.Slots == other.Slots && candidate.Left == other.Left && candidate.Right == other.Right;
        bool noWorse = candidate.Power >= other.Power && candidate.Fortitude >= other.Fortitude;
        bool strict = candidate.Power > other.Power || candidate.Fortitude > other.Fortitude;
        return sameStructure && noWorse && strict;
    }

    /// <summary>按70%底线保留，若不足3种则按置信度补齐；返回顺序即计算优先级。</summary>
    /// <param name="recipe">待计算配方。</param>
    /// <returns>正式送入几何阶段的结构key。</returns>
    public static int[][] RetainedKeys(Recipe recipe)
    {
        ConfidenceKey[] ranked = ConfidenceRatios(recipe).Keys;
        int minimum = Math.Min(3, ranked.Length);
        HashSet<string> selected = ranked.Where(k => k.Confidence >= .7).Select(k => k.Key).ToHashSet();
        foreach (ConfidenceKey key in ranked.Take(minimum))
            selected.Add(key.Key);
        return ranked.Where(k => selected.Contains(k.Key)).Select(k => ParseKey(k.Key)).ToArray();
    }

    /// <summary>取得结构的相对理想值，用于80%优先、70%候补调度。</summary>
    public static double KeyConfidence(Recipe recipe, string key) => ConfidenceRatios(recipe).Keys.FirstOrDefault(k => k.Key == key)?.Confidence ?? 0;

    /// <summary>报告与正式调度使用同一组选中结构。</summary>
    private static int[][] ProposedKeys(Recipe recipe) => RetainedKeys(recipe);

    /// <summary>把旧的精确0槽范围分组折叠为同一个战斗等价分组，保留数值区域和证明上界。</summary>
    /// <param name="catalog">当前资源快照。</param>
    /// <param name="recipe">待迁移配方。</param>
    public static void Migrate(Catalog catalog, Recipe recipe)
    {
        RecipeResult? saved = KeyRecipeSolver.Load(catalog, recipe);
        if (saved is null || saved.SelectionScope == Scope)
            return;
        GroupState[] zero = saved.Groups.Values.Where(g => g.Key.Length == 3 && g.Key[0] == 0).ToArray();
        if (zero.Length > 1)
        {
            GroupState merged = new()
            {
                Key = [0, 0, 0]
            };
            foreach (string goal in new[] { "power", "fortitude", "total" })
            {
                KeyGoalState[] states = zero.Select(g => g.Goals.GetValueOrDefault(goal)).Where(s => s is not null).Select(s => s!).ToArray();
                int upper = states.Select(s => s.UpperBound).DefaultIfEmpty(0).Max();
                int index = goal == "power" ? 0 : goal == "fortitude" ? 1 : 2;
                CardTemplate? best = zero.SelectMany(g => g.Best.TryGetValue(goal, out string? id) && saved.Cards.TryGetValue(id, out CardTemplate? c) ? new[] { c } : [])
                    .Where(c => c.Valid && c.Strikes == 0).OrderByDescending(c => Craft.Panel(c, index)).ThenBy(c => c.Id).FirstOrDefault();
                string status = best is not null && Craft.Panel(best, index) == upper ? "OPTIMAL" : states.Length == zero.Length && states.All(s => s.Status == "INFEASIBLE") ? "INFEASIBLE" : "UNKNOWN";
                merged.Goals[goal] = new KeyGoalState { Status = status, SearchPolicy = saved.Policy, UpperBound = upper, Seconds = states.Sum(s => s.Seconds), GeometryQueries = states.Sum(s => s.GeometryQueries) };
                if (best is not null)
                {
                    saved.Cards[best.Id] = best;
                    merged.Best[goal] = best.Id;
                    if (goal == "total")
                        merged.TotalBest = zero.SelectMany(g => g.TotalBest.Append(best.Id)).Distinct().ToArray();
                }
            }
            foreach (string key in saved.Groups.Where(p => p.Value.Key.Length == 3 && p.Value.Key[0] == 0).Select(p => p.Key).ToArray())
                saved.Groups.Remove(key);
            saved.Groups["0,0,0"] = merged;
        }
        saved.SelectionScope = Scope;
        saved.Complete = false;
        saved.Updated = DateTimeOffset.UtcNow;
        string path = Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json");
        string backup = Path.Combine(Storage.Root, ".codex", "trash", "confidence-key-v1", $"{recipe.Id:00}-{catalog.Data.Id}.json");
        if (!File.Exists(backup))
            Storage.Write(backup, KeyRecipeSolver.Load(catalog, recipe)!);
        Storage.Write(path, saved);
    }

    /// <summary>输出全部结构组合，并统一验证所有可承担全局覆盖定位的配方。</summary>
    /// <param name="catalog">当前游戏资源快照。</param>
    /// <returns>写入的JSON报告路径。</returns>
    public static string Run(Catalog catalog)
    {
        Stopwatch clock = Stopwatch.StartNew();
        int[] traits = catalog.Data.Shapes.SelectMany(s => s.Sources).SelectMany(s => s.Traits).Where(t => t > 1).Distinct().Order().ToArray();
        var recipes = catalog.Data.Recipes.OrderBy(r => r.Id).Select(recipe =>
        {
            int[][] keys = KeyRecipeSolver.Keys(recipe), retained = ProposedKeys(recipe);
            var regions = recipe.Areas.Select((area, index) =>
            {
                int slots = area.Effects.Where(e => e.Kind == 7).Sum(e => e.Arguments[0].Value);
                int left = area.Effects.Where(e => e.Kind == 8).Sum(e => e.Arguments[0].Value);
                int right = area.Effects.Where(e => e.Kind == 8).Sum(e => e.Arguments[1].Value);
                return new
                {
                    area = index,
                    slots,
                    left,
                    right
                };
            }).Where(x => x.slots != 0 || x.left != 0 || x.right != 0).ToArray();
            return new
            {
                recipe = recipe.Id,
                name = recipe.Name,
                global_range = keys.Any(k => k[0] == 3 && k[1] + k[2] >= 4),
                structural_regions = regions,
                keys = keys.Select(Key).ToArray(),
                retained_keys = retained.Select(Key).ToArray()
            };
        }).ToArray();
        var globalRecipes = recipes.Where(r => r.global_range).ToArray();
        Encounter[] benchmarkEncounters = [
            new Encounter { Id = -1, Name = "对称连接基准", Mode = "connect", Cards = BenchmarkOpponents() },
            new Encounter { Id = -2, Name = "对称反射基准", Mode = "reflection", Cards = BenchmarkOpponents() }
        ];
        Dictionary<int, int[][]> combinations = Enumerable.Range(0, 4).ToDictionary(slots => slots, slots => Combinations(traits, slots));
        var globalRangeReports = globalRecipes.Select(recipe =>
        {
            Recipe source = catalog.Data.Recipes.Single(r => r.Id == recipe.recipe);
            int power = Craft.FinalStat(source.Areas.SelectMany(a => a.Effects).Where(e => e.Kind == 3).Sum(e => e.Arguments[0].Value), 0);
            int fortitude = Craft.FinalStat(source.Areas.SelectMany(a => a.Effects).Where(e => e.Kind == 4).Sum(e => e.Arguments[0].Value), 0);
            BenchmarkEncounter[] benchmark = benchmarkEncounters.Select(encounter => new BenchmarkEncounter(encounter.Id, encounter.Name,
                recipe.keys.Select(key =>
                {
                    int[] k = ParseKey(key);
                    BenchmarkKey? best = null;
                    for (int position = 0; position < 5; position++)
                        foreach (int[] skillSet in combinations[k[0]])
                        {
                            BattleResult battle = Simulate(catalog, encounter, position, k[1], k[2], skillSet, power, fortitude);
                            BenchmarkKey candidate = new(key, battle.Passed, battle.Passed ? battle.Score : battle.Margin, battle.Score, position, skillSet);
                            if (best is null || Better(candidate, best))
                                best = candidate;
                        }
                    return best!;
                }).ToArray())).ToArray();
            HashSet<string> retained = recipe.retained_keys.ToHashSet();
            var encounters = benchmark.Select(e =>
            {
                BenchmarkKey kept = e.Keys.Where(k => retained.Contains(k.Key)).OrderByDescending(k => k.Passed).ThenByDescending(k => k.Value).First();
                BenchmarkKey? omitted = e.Keys.Where(k => !retained.Contains(k.Key)).OrderByDescending(k => k.Passed).ThenByDescending(k => k.Value).FirstOrDefault();
                return new
                {
                    encounter = e.Encounter,
                    kept,
                    omitted,
                    omitted_exceeds = omitted is not null && Better(omitted, kept),
                    strict = omitted is null || Better(kept, omitted)
                };
            }).ToArray();
            return new
            {
                recipe.recipe,
                recipe.name,
                optimistic_power = power,
                optimistic_fortitude = fortitude,
                theoretical = recipe.keys,
                retained = recipe.retained_keys,
                no_omitted_exceeds = encounters.All(e => !e.omitted_exceeds),
                strict_encounters = encounters.Count(e => e.strict),
                encounters
            };
        }).ToArray();
        var frontiers = catalog.Data.Recipes.OrderBy(r => r.Id).Select(PanelSlotFrontier).Where(x => x is not null).ToArray();
        ConfidenceRecipe[] confidenceRatios = catalog.Data.Recipes.OrderBy(r => r.Id).Select(ConfidenceRatios).ToArray();

        string path = Path.Combine(Storage.Root, "output", "结构置信度报告.json");
        Storage.Write(path, new
        {
            generated = DateTimeOffset.UtcNow,
            snapshot = catalog.Data.Id,
            benchmark = new
            {
                focus_card_stats = "各配方全部区域攻击/防御奖励之和，按999效能换算，不要求几何可同时命中",
                other_cards = 4,
                other_card_power = 40000,
                other_card_fortitude = 40000,
                team_power_without_focus = 160000,
                team_fortitude_without_focus = 160000,
                opponent_cards = 5,
                opponent_card_power = 40000,
                opponent_card_fortitude = 40000,
                modes = new[] { "connect", "reflection" },
                method = "对称四万攻防、无技能、无颜色克制基准；分别运行连接与反射模式。每个结构key遍历5个卡位及恰好槽数个可刷技能；忽略材料承载和几何命中，仅作几何前置信度筛选"
            },
            rule = "若配方可达3槽且左+右>=4，同时保留面板路线0,0,0与3槽全局范围路线；否则暂不按结构删除",
            global_range_recipes = globalRangeReports,
            panel_slot_frontiers = frontiers,
            confidence_ratios = new
            {
                high_threshold = .8,
                reserve_threshold = .7,
                skill_model = "每槽理想收益=max(范围内27%,范围外14%,除自身13%)；按最优卡位覆盖数量计算；候选值=乐观卡面+槽位收益",
                recipes = confidenceRatios
            },
            recipes,
            elapsed_seconds = clock.Elapsed.TotalSeconds
        });

        string textPath = Path.Combine(Storage.Root, "output", "结构置信度报告.txt");
        List<string> lines =
        [
            "结构置信度初筛",
            $"资源快照：{catalog.Data.Id}",
            "通用规则：若配方可达3槽且左+右>=4，同时保留面板路线0,0,0和3槽全局范围路线；其他卡暂不按结构删除。",
            "基准：目标卡取该配方全部区域数值之和，不要求实际命中；我方其余四卡和对方五卡均各40000攻防、无技能、无颜色克制；分别运行connect和reflection。",
            ""
        ];
        foreach (var report in globalRangeReports)
            lines.Add($"{report.recipe:00} {report.name}：乐观面板{report.optimistic_power}/{report.optimistic_fortitude}；保留 {string.Join(' ', report.retained)}；两种模式无反超={report.no_omitted_exceeds}；严格领先={report.strict_encounters}/2");
        lines.Add("");
        lines.Add("70%/80%相对理想值分档：");
        foreach (ConfidenceRecipe report in confidenceRatios)
            lines.Add($"{report.Recipe:00} {report.Name}：>=80% [{string.Join(' ', report.High)}]；70-80% [{string.Join(' ', report.Reserve)}]；<70% [{string.Join(' ', report.Pruned)}]；至少3 [{string.Join(' ', report.AtLeastThree)}]；至少4 [{string.Join(' ', report.AtLeastFour)}]");
        lines.Add("");
        lines.Add("全部配方理论结构 → 分析报告临时双路线：");
        foreach (var recipe in recipes)
            lines.Add($"{recipe.recipe:00} {recipe.name}：{string.Join(' ', recipe.keys)} → {string.Join(' ', recipe.retained_keys)}");
        Directory.CreateDirectory(Path.GetDirectoryName(textPath)!);
        File.WriteAllLines(textPath, lines);
        Console.WriteLine($"结构置信度报告：{path}");
        Console.WriteLine($"结构组合摘要：{textPath}");
        Console.WriteLine($"全局范围卡{globalRangeReports.Length}张，全部简化基准无反超={globalRangeReports.All(r => r.no_omitted_exceeds)}；耗时{clock.Elapsed.TotalSeconds:F3}秒。");
        return path;
    }

    /// <summary>忽略几何，以结构兼容区域的乐观面板和技能覆盖估计相对理想值。</summary>
    private static ConfidenceRecipe ConfidenceRatios(Recipe recipe) => ConfidenceCache.GetOrAdd(recipe.Id, _ => BuildConfidenceRatios(recipe));

    private static ConfidenceRecipe BuildConfidenceRatios(Recipe recipe)
    {
        Dictionary<string, (Panel Power, Panel Fortitude, Panel Total)> panels = IdealPanels(recipe);
        var scores = panels.Select(pair =>
        {
            int[] key = ParseKey(pair.Key);
            Panel p = pair.Value.Power, f = pair.Value.Fortitude, t = pair.Value.Total;
            return new
            {
                Key = pair.Key,
                Power = p.Power,
                Fortitude = f.Fortitude,
                TotalPower = t.Power,
                TotalFortitude = t.Fortitude,
                PowerScore = IdealScore(p, key, 0),
                FortitudeScore = IdealScore(f, key, 1),
                TotalScore = IdealScore(t, key, 2)
            };
        }).ToArray();
        double maxPower = scores.Max(x => x.PowerScore), maxFortitude = scores.Max(x => x.FortitudeScore), maxTotal = scores.Max(x => x.TotalScore);
        ConfidenceKey[] keys = scores.Select(x =>
        {
            double pr = x.PowerScore / maxPower, fr = x.FortitudeScore / maxFortitude, tr = x.TotalScore / maxTotal;
            double confidence = Math.Max(pr, Math.Max(fr, tr));
            return new ConfidenceKey(x.Key, x.Power, x.Fortitude, x.TotalPower + x.TotalFortitude, x.PowerScore, x.FortitudeScore, x.TotalScore,
                pr, fr, tr, confidence, confidence >= .8 ? "high" : confidence >= .7 ? "reserve" : "pruned");
        }).OrderByDescending(x => x.Confidence).ThenBy(x => x.Key).ToArray();
        string[] Ensure(int count) => keys.Take(Math.Max(count, keys.Count(k => k.Band == "high"))).Select(k => k.Key).ToArray();
        return new ConfidenceRecipe(recipe.Id, recipe.Name, keys, keys.Where(k => k.Band == "high").Select(k => k.Key).ToArray(),
            keys.Where(k => k.Band == "reserve").Select(k => k.Key).ToArray(), keys.Where(k => k.Band == "pruned").Select(k => k.Key).ToArray(), Ensure(3), Ensure(4));
    }

    /// <summary>按区域奖励做小状态动态规划，取得各结构方向的乐观攻击、防御和总和面板。</summary>
    private static Dictionary<string, (Panel Power, Panel Fortitude, Panel Total)> IdealPanels(Recipe recipe)
    {
        Dictionary<(int Slots, int Left, int Right), Panel> power = new()
        {
            [(0, 0, 0)] = new(0, 0)
        };
        Dictionary<(int Slots, int Left, int Right), Panel> fortitude = new(power), total = new(power);
        uint excluded = KeyRecipeSolver.Excluded(recipe);
        for (int region = 0; region < recipe.Areas.Length; region++)
        {
            if ((excluded & (1u << region)) != 0)
                continue;
            BonusArea area = recipe.Areas[region];
            int slots = area.Effects.Where(e => e.Kind == 7).Sum(e => e.Arguments[0].Value);
            int left = area.Effects.Where(e => e.Kind == 8).Sum(e => e.Arguments[0].Value);
            int right = area.Effects.Where(e => e.Kind == 8).Sum(e => e.Arguments[1].Value);
            int p = area.Effects.Where(e => e.Kind == 3).Sum(e => e.Arguments[0].Value);
            int f = area.Effects.Where(e => e.Kind == 4).Sum(e => e.Arguments[0].Value);
            AddArea(power, slots, left, right, p, f, 0);
            AddArea(fortitude, slots, left, right, p, f, 1);
            AddArea(total, slots, left, right, p, f, 2);
        }
        Dictionary<string, (Panel Power, Panel Fortitude, Panel Total)> result = [];
        foreach (var raw in power.Keys.Union(fortitude.Keys).Union(total.Keys))
        {
            var normalized = raw.Slots == 0 ? (0, 0, 0) : raw;
            string key = $"{normalized.Item1},{normalized.Item2},{normalized.Item3}";
            Panel p = power.GetValueOrDefault(raw), f = fortitude.GetValueOrDefault(raw), t = total.GetValueOrDefault(raw);
            if (!result.TryGetValue(key, out var old))
                old = (new(), new(), new());
            result[key] = (BetterPanel(old.Power, p, 0), BetterPanel(old.Fortitude, f, 1), BetterPanel(old.Total, t, 2));
        }
        return result;
    }

    private static void AddArea(Dictionary<(int Slots, int Left, int Right), Panel> states, int slots, int left, int right, int power, int fortitude, int goal)
    {
        foreach (var state in states.ToArray())
        {
            var key = (Math.Min(3, state.Key.Slots + slots), Math.Min(4, state.Key.Left + left), Math.Min(4, state.Key.Right + right));
            Panel panel = new(state.Value.Power + power, state.Value.Fortitude + fortitude);
            states[key] = BetterPanel(states.GetValueOrDefault(key), panel, goal);
        }
    }

    private static Panel BetterPanel(Panel current, Panel candidate, int goal)
    {
        int Current() => goal == 0 ? current.Power : goal == 1 ? current.Fortitude : current.Power + current.Fortitude;
        int Candidate() => goal == 0 ? candidate.Power : goal == 1 ? candidate.Fortitude : candidate.Power + candidate.Fortitude;
        return Candidate() > Current() || Candidate() == Current() && candidate.Power + candidate.Fortitude > current.Power + current.Fortitude ? candidate : current;
    }

    /// <summary>把卡面和每槽最佳覆盖收益换成共同的单项或攻防总量单位。</summary>
    private static double IdealScore(Panel raw, int[] key, int goal)
    {
        double own = goal == 0 ? Craft.FinalStat(raw.Power, 0) : goal == 1 ? Craft.FinalStat(raw.Fortitude, 0)
            : Craft.FinalStat(raw.Power, 0) + Craft.FinalStat(raw.Fortitude, 0);
        if (key[0] == 0)
            return own;
        double other = goal == 2 ? 80000 : 40000;
        int width = Math.Min(5, key[1] + key[2] + 1);
        double inside = .27 * (own + (width - 1) * other);
        double peripheral = .14 * (5 - width) * other;
        double external = .13 * 4 * other;
        return own + key[0] * Math.Max(inside, Math.Max(peripheral, external));
    }

    private static BattleCard[] BenchmarkOpponents() => Enumerable.Range(0, 5)
        .Select(_ => new BattleCard { Name = "对称四万基准卡", Power = 40000, Fortitude = 40000 }).ToArray();

    private static string Key(int[] key) => string.Join(',', key);
    private static int[] ParseKey(string key) => key.Split(',').Select(int.Parse).ToArray();
    private static bool Better(BenchmarkKey candidate, BenchmarkKey previous) => candidate.Passed && !previous.Passed || candidate.Passed == previous.Passed && candidate.Value > previous.Value;

    /// <summary>枚举恰好占用指定槽数的不同技能集合。</summary>
    private static int[][] Combinations(int[] traits, int slots)
    {
        if (slots == 0)
            return [[]];
        List<int[]> result = [];
        void Pick(int start, List<int> selected)
        {
            if (selected.Count == slots)
            {
                result.Add(selected.ToArray());
                return;
            }
            for (int i = start; i <= traits.Length - (slots - selected.Count); i++)
            {
                selected.Add(traits[i]);
                Pick(i + 1, selected);
                selected.RemoveAt(selected.Count - 1);
            }
        }
        Pick(0, []);
        return result.ToArray();
    }

    /// <summary>用一张目标结构卡和四张固定四万攻防卡执行真实战斗时序。</summary>
    private static BattleResult Simulate(Catalog catalog, Encounter encounter, int position, int left, int right, int[] traits, int power, int fortitude)
    {
        BattleCard[] team = Enumerable.Range(0, 5).Select(i => i == position
            ? new BattleCard { Name = "结构基准卡", Power = power, Fortitude = fortitude, Left = left, Right = right, Traits = traits }
            : new BattleCard { Name = "固定四万基准卡", Power = 40000, Fortitude = 40000 }).ToArray();
        return new Battle(catalog, team, encounter).Run();
    }

    /// <summary>从已有候选中提取面板、槽数和范围互不支配的路线，不按配方编号硬编码。</summary>
    private static object? PanelSlotFrontier(Recipe recipe)
    {
        RecipeResult? result = Storage.Read<RecipeResult>(Path.Combine(Storage.State, "key-recipes", $"{recipe.Id:00}.json"));
        if (result is null)
            return null;
        var routes = result.Groups.Select(pair =>
        {
            if (!pair.Value.Best.TryGetValue("total", out string? id) || !result.Cards.TryGetValue(id, out CardTemplate? card))
                return null;
            return new
            {
                key = pair.Key,
                card.Slots,
                card.Left,
                card.Right,
                width = Math.Min(4, card.Left + card.Right),
                card.Power,
                card.Fortitude,
                total = card.Total
            };
        }).Where(x => x is not null).Select(x => x!).ToArray();
        var frontier = routes.Where(route => !routes.Any(other => other.key != route.key && other.Slots >= route.Slots && other.width >= route.width && other.total >= route.total &&
            (other.Slots > route.Slots || other.width > route.width || other.total > route.total))).OrderBy(r => r.Slots).ThenBy(r => r.width).ThenByDescending(r => r.total).ToArray();
        return new
        {
            recipe = recipe.Id,
            name = recipe.Name,
            routes = frontier
        };
    }
}
