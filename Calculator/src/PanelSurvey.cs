using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InFalsusCalc;

/// <summary>
/// 一阶段新跑法的只统计运行：枚举全部区域组合的理论面板，按有用性分层，并对照检查点与现有配队。
/// 不调用任何布局求解，不写检查点；输出results/panel-survey.json。
/// </summary>
internal static class PanelSurvey
{
    /// <summary>配队层阈值：任一目标的加权分不低于同名卡同范围同惩罚最高分的该比例。</summary>
    internal const double Threshold = .85;

    /// <summary>对配队等价的面板身份：同配方、同惩罚、同槽位范围和同攻防。</summary>
    internal readonly record struct Panel(int Recipe, int Strikes, int Slots, int Left, int Right, int Power, int Fortitude);

    /// <summary>实现同一面板的区域组合统计。</summary>
    /// <param name="Masks">能产生该面板且通过粒子数下界的区域组合数。</param>
    /// <param name="Known">检查点中已有合法布局的组合数。</param>
    /// <param name="Impossible">检查点中已证不可行的组合数。</param>
    internal readonly record struct Support(int Masks, int Known, int Impossible);

    /// <summary>一张配方去重后的理论面板。</summary>
    /// <param name="Recipe">配方编号。</param>
    /// <param name="BaseId">同名卡编号，阈值与支配按同名卡跨配方比较。</param>
    /// <param name="Name">配方中文名。</param>
    /// <param name="Cells">棋盘去重格数，决定基础时限。</param>
    /// <param name="Sets">通过粒子数下界的区域组合数。</param>
    /// <param name="Panels">理论面板及其区域组合支持。</param>
    /// <param name="Masks">每个面板尚未被证明不可行的区域位集，供面板求解使用。</param>
    internal sealed record Layers(int Recipe, int BaseId, string Name, int Cells, int Sets, Dictionary<Panel, Support> Panels,
        Dictionary<Panel, List<uint>> Masks);

    /// <summary>三层分类结果。</summary>
    /// <param name="Deck">配队层中检查点已有布局的面板。</param>
    /// <param name="Open">配队层中尚无布局、需要求解的面板。</param>
    /// <param name="Display">只供网页展示的已知面板。</param>
    internal sealed record Plan(HashSet<Panel> Deck, HashSet<Panel> Open, HashSet<Panel> Display);

    /// <summary>运行普查并打印摘要。</summary>
    /// <param name="args">可选--threads与--report。</param>
    /// <returns>完成0。</returns>
    public static int Run(string[] args)
    {
        int threads = Environment.ProcessorCount;
        string reportPath = Path.Combine(Storage.Root, "reports", "report.json");
        for (int i = 0; i + 1 < args.Length; i += 2)
            switch (args[i])
            {
                case "--threads": threads = int.Parse(args[i + 1]); break;
                case "--report": reportPath = Path.GetFullPath(args[i + 1]); break;
                default: throw new ArgumentException($"survey-panels不支持参数{args[i]}。");
            }
        Stopwatch clock = Stopwatch.StartNew();
        Catalog catalog = new();
        return Summarize(SurveyAll(catalog, threads), reportPath, clock);
    }

    /// <summary>并行普查全部配方。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="threads">同时普查的配方数。</param>
    /// <returns>按配方编号排列的普查结果。</returns>
    internal static Layers[] SurveyAll(Catalog catalog, int threads)
    {
        ConcurrentBag<Layers> results = [];
        Parallel.ForEach(catalog.Data.Recipes.OrderByDescending(r => r.Areas.Length),
            new ParallelOptions { MaxDegreeOfParallelism = threads }, recipe => results.Add(Survey(catalog, recipe)));
        return results.OrderBy(r => r.Recipe).ToArray();
    }

    /// <summary>枚举一张配方的区域组合，并把相同面板合并；不调用几何求解。</summary>
    /// <param name="catalog">当前资源。</param>
    /// <param name="recipe">目标配方。</param>
    /// <returns>去重后的理论面板与检查点覆盖。</returns>
    private static Layers Survey(Catalog catalog, Recipe recipe)
    {
        // 构造器只读取检查点并做几何预编译；普查不调用Run或Save，不改检查点。
        KeyRecipeSolver solver = new(catalog, recipe, () => 1, 1, CancellationToken.None);
        var solved = solver.SurveySolved();
        var sets = solver.SurveySets();
        Dictionary<Panel, Support> panels = [];
        Dictionary<Panel, List<uint>> masks = [];
        foreach (var set in sets)
            foreach (int strikes in ConfidenceAnalysis.RetainedStrikes)
            {
                Panel panel = new(recipe.Id, strikes, set.Slots, set.Left, set.Right,
                    Craft.FinalStat(set.Power, strikes), Craft.FinalStat(set.Fortitude, strikes));
                bool found = solved.TryGetValue((set.Mask, strikes), out CardTemplate? card);
                Support old = panels.GetValueOrDefault(panel);
                panels[panel] = new(old.Masks + 1, old.Known + (found && card is not null ? 1 : 0),
                    old.Impossible + (found && card is null ? 1 : 0));
                if (!(found && card is null))
                {
                    if (!masks.TryGetValue(panel, out List<uint>? list))
                        masks[panel] = list = [];
                    list.Add(set.Mask);
                }
            }
        return new(recipe.Id, recipe.BaseId, recipe.Name, recipe.Board.Select(c => c.Position).Distinct().Count(), sets.Length, panels, masks);
    }

    /// <summary>把普查面板分为配队层已知、配队层待求解和展示层。</summary>
    /// <param name="recipes">按配方编号排列的普查结果。</param>
    /// <returns>三层分类。</returns>
    internal static Plan Classify(Layers[] recipes)
    {
        var all = recipes.SelectMany(r => r.Panels.Select(p => (Layer: r, Panel: p.Key, Support: p.Value))).ToArray();
        // feasible：检查点已有合法布局；open：尚无布局也未被证明不可行，只有求解后才知道能否实现。
        static bool Feasible(Support x) => x.Known > 0;
        static bool Open(Support x) => x.Known == 0 && x.Impossible < x.Masks;
        HashSet<Panel> deck = [], open = [], display = [];

        // 配队层：同名卡、同净惩罚、同左右范围为一族。支配者和阈值基准只取已有布局的面板，
        // 避免把几何上无法实现的理论面板当成“更好的卡”。未被已知面板支配且达到阈值的open面板需要求解。
        foreach (var family in all.GroupBy(x => (x.Layer.BaseId, x.Panel.Strikes, x.Panel.Left, x.Panel.Right)))
        {
            var ordered = family.Where(x => Feasible(x.Support) || Open(x.Support))
                .OrderByDescending(x => x.Panel.Power).ThenByDescending(x => x.Panel.Fortitude)
                .ThenByDescending(x => Feasible(x.Support)).ThenByDescending(x => x.Panel.Slots).ToArray();
            // maxFortitude[s]：已扫描（攻击不低于当前）且槽数不少于s的已知可行面板的最高防御。
            int[] maxFortitude = [-1, -1, -1, -1];
            List<(Panel Panel, bool Known)> front = [];
            foreach (var x in ordered)
            {
                if (maxFortitude[x.Panel.Slots] < x.Panel.Fortitude)
                    front.Add((x.Panel, Feasible(x.Support)));
                if (Feasible(x.Support))
                    for (int s = 0; s <= x.Panel.Slots; s++)
                        maxFortitude[s] = Math.Max(maxFortitude[s], x.Panel.Fortitude);
            }
            double[] best = [.. Enumerable.Range(0, 3).Select(goal => family.Where(x => Feasible(x.Support))
                .Select(x => Score(x.Panel, goal)).DefaultIfEmpty(0).Max())];
            foreach (var (panel, known) in front)
                if (Enumerable.Range(0, 3).Any(goal => Score(panel, goal) >= best[goal] * Threshold))
                    (known ? deck : open).Add(panel);
        }

        // 展示层：每个配方、槽位范围与净惩罚保留已知可行面板中的攻最高、防最高，以及最大总和下的全部攻防拆分，
        // 与现有“攻防总和保留全部拆分”的规则一致；已在配队层的不重复计入。
        foreach (var group in all.Where(x => Feasible(x.Support))
            .GroupBy(x => (x.Panel.Recipe, x.Panel.Strikes, x.Panel.Slots, x.Panel.Left, x.Panel.Right)))
        {
            int maxTotal = group.Max(x => x.Panel.Power + x.Panel.Fortitude);
            Panel[] ends =
            [
                group.OrderByDescending(x => x.Panel.Power).ThenByDescending(x => x.Panel.Fortitude).First().Panel,
                group.OrderByDescending(x => x.Panel.Fortitude).ThenByDescending(x => x.Panel.Power).First().Panel,
                .. group.Where(x => x.Panel.Power + x.Panel.Fortitude == maxTotal).Select(x => x.Panel)
            ];
            foreach (Panel end in ends)
                if (!deck.Contains(end))
                    display.Add(end);
        }

        return new(deck, open, display);
    }

    /// <summary>分层、对照现有配队并写出普查报告。</summary>
    /// <param name="recipes">按配方编号排列的普查结果。</param>
    /// <param name="reportPath">提供现有配队的完整报告。</param>
    /// <param name="clock">普查总计时。</param>
    /// <returns>完成0。</returns>
    private static int Summarize(Layers[] recipes, string reportPath, Stopwatch clock)
    {
        Dictionary<int, Layers> byRecipe = recipes.ToDictionary(r => r.Recipe);
        (HashSet<Panel> deck, HashSet<Panel> open, HashSet<Panel> display) = Classify(recipes);
        int theoryPanels = recipes.Sum(r => r.Panels.Count);

        // 对照现有93套配队实际使用的模板：它们的面板必须全部落在配队层，否则筛选过严。
        JsonObject report = JsonNode.Parse(File.ReadAllBytes(reportPath))?.AsObject()
            ?? throw new InvalidDataException("报告为空。");
        Dictionary<string, CardTemplate> templates = report["templates"]!.Deserialize<Dictionary<string, CardTemplate>>(Storage.Json)!;
        HashSet<string> usedIds = [];
        foreach (var encounter in report["encounters"]!.AsObject())
            foreach (var team in encounter.Value!.AsObject())
                foreach (JsonNode? card in team.Value!["cards"]!.AsArray())
                    usedIds.Add(card!["template"]!.GetValue<string>());
        // 0槽卡的范围不影响配队，普查面板统一归零，对照时同样归零。
        static Panel Of(CardTemplate card) => card.Slots == 0 ? new(card.Recipe, card.Strikes, 0, 0, 0, card.Power, card.Fortitude)
            : new(card.Recipe, card.Strikes, card.Slots, card.Left, card.Right, card.Power, card.Fortitude);
        CardTemplate[] missing = usedIds.Select(id => templates[id]).Where(card => !deck.Contains(Of(card))).ToArray();
        // 现有卡库模板若落在两层之外，说明新跑法会丢掉它。
        CardTemplate[] libraryDropped = report["library_templates"]!.Deserialize<Dictionary<string, CardTemplate>>(Storage.Json)!
            .Values.Where(card => !deck.Contains(Of(card)) && !display.Contains(Of(card))).ToArray();

        // 按“时限内未找到即视为不存在”，每个open面板最多消耗一个基础时限；不可行的面板必然用满时限。
        double openSeconds = open.Sum(p => BaseSeconds(byRecipe[p.Recipe].Cells));

        string output = Path.Combine(Storage.Root, "results", "panel-survey.json");
        Storage.Write(output, new
        {
            threshold = Threshold,
            region_sets = recipes.Sum(r => r.Sets),
            theory_panels = theoryPanels,
            deck_known = deck.Count,
            deck_open = open.Count,
            display_known = display.Count,
            open_hours_single_thread = Math.Round(openSeconds / 3600, 2),
            used_templates = usedIds.Count,
            used_missing = missing.Select(c => new { c.Id, c.Name, c.Power, c.Fortitude, c.Slots, c.Left, c.Right, c.Strikes }).ToArray(),
            library_dropped = libraryDropped.Select(c => new { c.Id, c.Name, c.Power, c.Fortitude, c.Slots, c.Left, c.Right, c.Strikes }).ToArray(),
            recipes = recipes.Select(r => new
            {
                recipe = r.Recipe,
                name = r.Name,
                cells = r.Cells,
                region_sets = r.Sets,
                panels = r.Panels.Count,
                deck = deck.Count(p => p.Recipe == r.Recipe),
                open = open.Count(p => p.Recipe == r.Recipe),
                display = display.Count(p => p.Recipe == r.Recipe),
                base_seconds = BaseSeconds(r.Cells)
            }).ToArray()
        });
        Console.WriteLine($"面板普查：区域组合{recipes.Sum(r => r.Sets)}，理论面板{theoryPanels}；"
            + $"配队层已知{deck.Count}、待求解{open.Count}，展示层已知{display.Count}。");
        Console.WriteLine($"配队在用模板{usedIds.Count}张，不在配队层{missing.Length}张；现有卡库落在两层之外{libraryDropped.Length}张。");
        Console.WriteLine($"待求解面板基础时限合计{openSeconds / 3600:F2}小时（单线程上限，未含顺延）；普查{clock.Elapsed.TotalSeconds:F1}秒。");
        Console.WriteLine($"报告：{output}");
        return 0;
    }

    /// <summary>按槽位与范围加权的目标分，与置信度报告使用同一口径。</summary>
    /// <param name="panel">理论面板。</param>
    /// <param name="goal">0攻击、1防御、2攻防总和。</param>
    /// <returns>可在同名卡内比较的加权分。</returns>
    internal static double Score(Panel panel, int goal) =>
        ConfidenceAnalysis.PanelScore(panel.Power, panel.Fortitude, [panel.Slots, panel.Left, panel.Right], goal);

    /// <summary>按棋盘格数给出单个面板的基础时限；时限内有新发现时由求解阶段顺延。</summary>
    /// <param name="cells">配方棋盘的去重格数。</param>
    /// <returns>基础时限秒数，初值待普查后按实测校准。</returns>
    internal static double BaseSeconds(int cells) => cells switch
    {
        < 1000 => 2,
        < 1600 => 4,
        < 2200 => 8,
        _ => 15
    };
}
