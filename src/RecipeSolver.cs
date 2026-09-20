using Google.OrTools.Sat;
using System.Text.Json;

namespace InFalsusCalc;

/// <summary>整体粒子布局求解器；分别最大化内向和外向结构，再最大化攻防总和。</summary>
internal sealed class RecipeSolver
{
    public static readonly string[] Goals = ["power", "fortitude", "total"];
    private readonly Catalog catalog;
    private readonly Recipe recipe;
    private readonly HashSet<Hex> board;
    private readonly HashSet<Hex> safe;
    private readonly int limit;
    private readonly int threads;
    private readonly int directionSeconds;
    private readonly int searchSeed;
    private readonly HashSet<int> usefulAreas;
    public RecipeResult Result { get; }

    public RecipeSolver(Catalog catalog, Recipe recipe, int threads, int directionSeconds, int searchSeed)
    {
        this.catalog = catalog; this.recipe = recipe;
        board = recipe.Board.Select(c => c.Position).ToHashSet();
        safe = recipe.Safe.Select(c => c.Position).ToHashSet();
        limit = Math.Min(Craft.Skills[recipe.Character].Count, recipe.MaxIota);
        this.threads = threads;
        this.directionSeconds = directionSeconds;
        this.searchSeed = searchSeed;
        usefulAreas = UsefulAreas();
        Result = new RecipeResult { Recipe = recipe.Id, Snapshot = catalog.Data.Id, Policy = Storage.Policy };
    }

    /// <summary>直接从整体粒子布局求内向、外向两个结构方向。</summary>
    public void FinalizeBounded()
    {
        DateTime started = DateTime.UtcNow;
        RestorePrevious();
        foreach (bool outer in new[] { false, true })
        {
            string direction = outer ? "outer" : "inner";
            if (HasDirection(direction)) continue;
            CardTemplate? card = Solve(outer, directionSeconds);
            if (card is null) continue;
            Result.StructureScores[direction] = outer ? card.OuterStructure : card.InnerStructure;
            AddCandidate(card, direction);
        }
        foreach (bool outer in new[] { false, true })
        {
            string direction = outer ? "outer" : "inner";
            if (HasDirection(direction)) continue;
            double target = outer ? MaximumStructure(true) : MaximumStructure(false) / 2d;
            CardTemplate? shared = Result.Cards.Values.Where(c => (outer ? c.OuterStructure : c.InnerStructure) == target)
                .OrderByDescending(c => c.Total).FirstOrDefault();
            if (shared is not null)
            {
                Result.StructureScores[direction] = target;
                AddCandidate(shared, direction);
            }
        }
        if (!HasDirection("inner") || !HasDirection("outer"))
        {
            Save();
            throw new InvalidDataException($"配方{recipe.Id}没有找到内向和外向最高结构的惩罚0正攻防布局。");
        }
        Result.BoundedFinalized = true;
        Result.SelectionScope = "from_scratch_cpsat_inner_outer_total";
        Result.SolveSeconds = (DateTime.UtcNow - started).TotalSeconds;
        Save();
    }

    private HashSet<int> UsefulAreas()
    {
        HashSet<int> result = [];
        for (int i = 0; i < recipe.Areas.Length; i++)
        {
            BonusArea area = recipe.Areas[i];
            int power = area.Effects.Where(e => e.Kind == 3).Sum(e => e.Arguments[0].Value);
            int fortitude = area.Effects.Where(e => e.Kind == 4).Sum(e => e.Arguments[0].Value);
            bool structure = area.Effects.Any(e => e.Kind is 1 or 7 or 8 or 9 or 10 or 11);
            if (structure || power + fortitude > 10) result.Add(i);
        }
        return result;
    }

    private CardTemplate? Solve(bool outer, double seconds)
    {
        int targetStructure = MaximumStructure(outer);
        double realTarget = outer ? targetStructure : targetStructure / 2d;
        CardTemplate? fallback = SeedFallback(outer, realTarget);
        DateTime started = DateTime.UtcNow;
        for (int target = targetStructure; target >= 0; target--)
        {
            double remaining = seconds - (DateTime.UtcNow - started).TotalSeconds;
            if (remaining <= 0) break;
            (CardTemplate? _, CpSolverStatus relaxedStatus) = SolveTarget(outer, Math.Min(5, remaining), target, false);
            if (relaxedStatus == CpSolverStatus.Infeasible) continue;
            remaining = seconds - (DateTime.UtcNow - started).TotalSeconds;
            if (remaining <= 0) break;
            (CardTemplate? card, CpSolverStatus status) = SolveTarget(outer, remaining, target, true);
            if (card is not null) return card;
            if (status != CpSolverStatus.Infeasible) return fallback;
        }
        return fallback;
    }

    private (CardTemplate? Card, CpSolverStatus Status) SolveTarget(bool outer, double seconds, int targetStructure, bool enforceConnectivity)
    {
        List<Choice> choices = BuildChoices();
        if (choices.Count == 0) return (null, CpSolverStatus.Infeasible);
        CpModel model = new();
        BoolVar[] selected = choices.Select((_, i) => model.NewBoolVar($"p{i}")).ToArray();
        Dictionary<Hex, List<BoolVar>> covers = [];
        Dictionary<(Hex, int), List<BoolVar>> matching = [];
        foreach (var pair in choices.Select((choice, index) => (choice, index)))
        {
            foreach (Hex cell in pair.choice.Placement.Cells)
            {
                if (!covers.TryGetValue(cell, out List<BoolVar>? list)) covers[cell] = list = [];
                list.Add(selected[pair.index]);
            }
            foreach ((Hex cell, int color) in pair.choice.Matches)
            {
                if (!matching.TryGetValue((cell, color), out List<BoolVar>? list)) matching[(cell, color)] = list = [];
                list.Add(selected[pair.index]);
            }
        }
        Dictionary<Hex, BoolVar> occupied = [];
        List<BoolVar> overlapped = [];
        foreach (var pair in covers)
        {
            BoolVar used = model.NewBoolVar($"occupied_{pair.Key}");
            model.AddMaxEquality(used, pair.Value);
            occupied[pair.Key] = used;
            if (pair.Value.Count < 2) continue;
            BoolVar overlap = model.NewBoolVar($"overlap_{pair.Key}");
            model.Add(LinearExpr.Sum(pair.Value) >= 2).OnlyEnforceIf(overlap);
            model.Add(LinearExpr.Sum(pair.Value) <= 1).OnlyEnforceIf(overlap.Not());
            overlapped.Add(overlap);
        }
        model.Add(LinearExpr.Sum(overlapped) <= Craft.Skills[recipe.Character].Overlap);
        model.Add(LinearExpr.Sum(selected.Where((_, i) => choices[i].Outside)) <= Craft.Skills[recipe.Character].Outside);
        model.Add(LinearExpr.Sum(selected) <= limit);
        Dictionary<int, BoolVar> active = [];
        Dictionary<int, LinearExpr> effects = [];
        List<LinearExpr> slotTerms = [], leftTerms = [], rightTerms = [], powerTerms = [], fortitudeTerms = [];
        for (int i = 0; i < recipe.Areas.Length; i++)
        {
            BonusArea area = recipe.Areas[i];
            BoolVar on = model.NewBoolVar($"area{i}"); active[i] = on;
            if (!usefulAreas.Contains(i)) { model.Add(on == 0); continue; }
            List<BoolVar> filled = [];
            foreach (BoardCell cell in area.Cells)
            {
                BoolVar value = model.NewBoolVar($"fill{i}_{cell.Position}");
                List<BoolVar>? matchingValues = matching.GetValueOrDefault((cell.Position, cell.Color));
                if (matchingValues is null) model.Add(value == 0); else model.AddMaxEquality(value, matchingValues);
                filled.Add(value);
            }
            model.AddMinEquality(on, filled);
            foreach (RegionEffect effect in area.Effects)
            {
                int value = effect.Arguments[0].Value;
                if (effect.Kind == 7) slotTerms.Add(on * value);
                else if (effect.Kind == 8) { leftTerms.Add(on * value); rightTerms.Add(on * effect.Arguments[1].Value); }
                else if (effect.Kind == 3) powerTerms.Add(on * value);
                else if (effect.Kind == 4) fortitudeTerms.Add(on * value);
            }
        }
        IntVar rawSlots = model.NewIntVar(0, 20, "raw_slots");
        IntVar rawLeft = model.NewIntVar(0, 20, "raw_left");
        IntVar rawRight = model.NewIntVar(0, 20, "raw_right");
        IntVar slots = model.NewIntVar(0, 3, "slots");
        IntVar left = model.NewIntVar(0, 4, "left");
        IntVar right = model.NewIntVar(0, 4, "right");
        IntVar power = model.NewIntVar(0, 200000, "power");
        IntVar fortitude = model.NewIntVar(0, 200000, "fortitude");
        model.Add(rawSlots == LinearExpr.Sum(slotTerms)); model.Add(rawLeft == LinearExpr.Sum(leftTerms)); model.Add(rawRight == LinearExpr.Sum(rightTerms));
        model.AddMinEquality(slots, [rawSlots, model.NewConstant(3)]);
        model.AddMinEquality(left, [rawLeft, model.NewConstant(4)]);
        model.AddMinEquality(right, [rawRight, model.NewConstant(4)]);
        model.Add(power == LinearExpr.Sum(powerTerms)); model.Add(fortitude == LinearExpr.Sum(fortitudeTerms));
        model.Add(power >= 1); model.Add(fortitude >= 1);
        IntVar range = model.NewIntVar(0, 40, "range"); model.Add(range == left + right);
        IntVar capped = model.NewIntVar(0, 5, "capped"); model.AddMinEquality(capped, [range + 1, model.NewConstant(5)]);
        IntVar over = model.NewIntVar(0, 40, "over"); model.AddMaxEquality(over, [range - 4, model.NewConstant(0)]);
        IntVar innerFactor = model.NewIntVar(0, 100, "inner_factor"); model.Add(innerFactor == capped * 2 + over);
        IntVar innerUnits = model.NewIntVar(0, 300, "inner_units");
        model.AddMultiplicationEquality(innerUnits, [slots, innerFactor]);
        IntVar outsideRange = model.NewIntVar(0, 40, "outside_range");
        model.AddMaxEquality(outsideRange, [model.NewConstant(4) - range, model.NewConstant(0)]);
        IntVar outerUnits = model.NewIntVar(0, 100, "outer_units");
        model.AddMultiplicationEquality(outerUnits, [slots, outsideRange]);
        IntVar total = model.NewIntVar(0, 400000, "total"); model.Add(total == power + fortitude);
        LinearExpr structure = outer ? outerUnits : innerUnits;
        model.Add(structure == targetStructure);
        CardTemplate? hintCard = SeedFallback(outer, outer ? targetStructure : targetStructure / 2d);
        HashSet<(int Id, int Q, int R)> seed = (hintCard?.Placements ?? []).Select(p => (p.Id, p.Q, p.R)).ToHashSet();
        for (int i = 0; i < choices.Count; i++)
            model.AddHint(selected[i], seed.Contains((choices[i].Placement.Id, choices[i].Placement.Q, choices[i].Placement.R)) ? 1 : 0);
        CpSolver solver = new(); solver.StringParameters = $"max_time_in_seconds:{seconds} num_search_workers:{threads} random_seed:{recipe.Id * 2 + (outer ? 1 : 0) + searchSeed} randomize_search:true stop_after_first_solution:true";
        DateTime started = DateTime.UtcNow;
        for (int attempt = 0; attempt < 5000 && (DateTime.UtcNow - started).TotalSeconds < seconds; attempt++)
        {
            CpSolverStatus status = solver.Solve(model);
            if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible)) return (null, status);
            if (!enforceConnectivity) return (null, status);
            int[] chosenIndexes = choices.Select((_, i) => i).Where(i => solver.Value(selected[i]) != 0).ToArray();
            Placement[] layout = chosenIndexes.Select(i => choices[i].Placement).ToArray();
            CardTemplate card = Craft.Evaluate(catalog, recipe, layout);
            double expectedStructure = outer ? targetStructure : targetStructure / 2d;
            double actualStructure = outer ? card.OuterStructure : card.InnerStructure;
            if (card.Valid && card.Strikes == 0 && actualStructure == expectedStructure) return (card, status);
            Placement[] repaired = Repair(layout, choices);
            CardTemplate repairedCard = Craft.Evaluate(catalog, recipe, repaired);
            double repairedStructure = outer ? repairedCard.OuterStructure : repairedCard.InnerStructure;
            if (repairedCard.Valid && repairedCard.Strikes == 0 && repairedStructure == expectedStructure) return (repairedCard, status);
            AddConnectivityCut(model, selected, choices, chosenIndexes, card);
            LinearExpr noGood = LinearExpr.Sum(chosenIndexes.Select(i => selected[i])) -
                LinearExpr.Sum(Enumerable.Range(0, choices.Count).Except(chosenIndexes).Select(i => selected[i]));
            model.Add(noGood <= chosenIndexes.Length - 1);
        }
        return (null, CpSolverStatus.Unknown);
    }

    private void AddConnectivityCut(CpModel model, BoolVar[] selected, List<Choice> choices, int[] chosenIndexes, CardTemplate card)
    {
        List<HashSet<Hex>> components = Craft.Components(card.Placements.SelectMany(p => p.Cells));
        HashSet<Hex>? component = components.FirstOrDefault(c => !c.Overlaps(safe));
        if (component is null && components.Count > Craft.Skills[recipe.Character].Split + 1)
            component = components.OrderBy(c => c.Count).First();
        if (component is null) return;
        HashSet<Hex> border = component.SelectMany(c => Hex.Directions.Select(c.Add)).Where(c => !component.Contains(c)).ToHashSet();
        int[] members = chosenIndexes.Where(i => choices[i].Placement.Cells.Any(component.Contains)).ToArray();
        int[] connectors = Enumerable.Range(0, choices.Count).Except(chosenIndexes)
            .Where(i => choices[i].Placement.Cells.Any(c => component.Contains(c) || border.Contains(c)) &&
                choices[i].Placement.Cells.Any(c => !component.Contains(c))).ToArray();
        LinearExpr left = LinearExpr.Sum(members.Select(i => selected[i])) - LinearExpr.Sum(connectors.Select(i => selected[i]));
        model.Add(left <= members.Length - 1);
    }

    private Placement[] Repair(Placement[] input, List<Choice> choices)
    {
        List<Placement> current = [.. input];
        for (int step = current.Count; step < limit; step++)
        {
            CardTemplate before = Craft.Evaluate(catalog, recipe, current);
            if (before.Strikes == 0 && before.Valid) return current.ToArray();
            (Placement Placement, CardTemplate Card)? best = null;
            foreach (Choice choice in choices)
            {
                if (current.Any(p => p.Id == choice.Placement.Id && p.Q == choice.Placement.Q && p.R == choice.Placement.R)) continue;
                List<Placement> candidate = [.. current, choice.Placement];
                CardTemplate value = Craft.Evaluate(catalog, recipe, candidate);
                if (best is null || RepairRank(value).CompareTo(RepairRank(best.Value.Card)) < 0) best = (choice.Placement, value);
            }
            if (best is null || RepairRank(best.Value.Card).CompareTo(RepairRank(before)) >= 0) break;
            current.Add(best.Value.Placement);
        }
        return current.ToArray();
    }

    private static (int Strikes, int Falsehood, int Components, int NegativeTotal) RepairRank(CardTemplate card) =>
        (card.Strikes, card.Falsehood ? 1 : 0, Craft.Components(card.Placements.SelectMany(p => p.Cells)).Count, -card.Total);

    private List<Choice> BuildChoices()
    {
        HashSet<(Hex, int)> needed = recipe.Areas.Where((_, i) => usefulAreas.Contains(i)).SelectMany(a => a.Cells).Select(c => (c.Position, c.Color)).ToHashSet();
        List<Choice> choices = [];
        HashSet<string> seen = [];
        foreach (Shape shape in catalog.Data.Shapes)
        {
            IEnumerable<Hex> anchors = needed.Where(x => x.Item2 == shape.Color).Select(x => x.Item1).Concat(safe).Distinct();
            foreach (Hex anchor in anchors)
                foreach (Hex local in shape.Cells)
                {
                    Hex at = new(anchor.Q - local.Q, anchor.R - local.R);
                    Hex[] cells = shape.Cells.Select(c => c.Add(at)).ToArray();
                    if (cells.Any(c => !board.Contains(c))) continue;
                    var matches = cells.Where(c => needed.Contains((c, shape.Color))).Select(c => (c, shape.Color)).ToArray();
                    bool outside = cells.Any(c => !safe.Contains(c));
                    if (matches.Length == 0 && outside && Craft.Skills[recipe.Character].Outside == 0) continue;
                    string key = $"{shape.Id}:{at.Q}:{at.R}";
                    if (!seen.Add(key)) continue;
                    choices.Add(new Choice(new Placement { Id = shape.Id, Q = at.Q, R = at.R, Color = shape.Color, Cells = cells }, matches, outside));
                }
        }
        string seedPath = Path.Combine(Storage.Root, "Data", "Seeds", $"{recipe.Id:00}.json");
        if (File.Exists(seedPath))
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(seedPath));
            foreach (Placement seedPlacement in document.RootElement.GetProperty("placements").Deserialize<Placement[]>(Storage.Json) ?? [])
            {
                Shape shape = catalog.Shapes[seedPlacement.Id];
                Hex[] cells = shape.Cells.Select(c => c.Add(new Hex(seedPlacement.Q, seedPlacement.R))).ToArray();
                if (cells.Any(c => !board.Contains(c))) continue;
                var matches = cells.Where(c => needed.Contains((c, shape.Color))).Select(c => (c, shape.Color)).ToArray();
                bool outside = cells.Any(c => !safe.Contains(c));
                string key = $"{shape.Id}:{seedPlacement.Q}:{seedPlacement.R}";
                if (!seen.Add(key)) continue;
                choices.Add(new Choice(new Placement { Id = shape.Id, Q = seedPlacement.Q, R = seedPlacement.R, Color = shape.Color, Cells = cells }, matches, outside));
            }
        }
        return choices;
    }

    private int MaximumStructure(bool outer)
    {
        int[] areas = usefulAreas.Order().ToArray();
        int best = 0;
        for (int mask = 0; mask < (1 << areas.Length); mask++)
        {
            int slots = 0, left = 0, right = 0;
            for (int bit = 0; bit < areas.Length; bit++)
            {
                if ((mask & (1 << bit)) == 0) continue;
                foreach (RegionEffect effect in recipe.Areas[areas[bit]].Effects)
                {
                    if (effect.Kind == 7) slots += effect.Arguments[0].Value;
                    else if (effect.Kind == 8) { left += effect.Arguments[0].Value; right += effect.Arguments[1].Value; }
                }
            }
            slots = Math.Min(3, slots); left = Math.Min(4, left); right = Math.Min(4, right);
            int score = outer ? slots * Math.Max(4 - left - right, 0)
                : slots * (2 * Math.Min(left + right + 1, 5) + Math.Max(left + right - 4, 0));
            best = Math.Max(best, score);
        }
        return best;
    }

    private IEnumerable<CardTemplate> SeedCards()
    {
        string resultPath = Path.Combine(Storage.State, "recipes", $"{recipe.Id:00}.json");
        RecipeResult? previous = Storage.Read<RecipeResult>(resultPath);
        if (previous is not null)
            foreach (CardTemplate saved in previous.Cards.Values)
                yield return Craft.Evaluate(catalog, recipe, saved.Placements);
        string seedPath = Path.Combine(Storage.Root, "Data", "Seeds", $"{recipe.Id:00}.json");
        if (!File.Exists(seedPath)) yield break;
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(seedPath));
        Placement[] placements = document.RootElement.GetProperty("placements").Deserialize<Placement[]>(Storage.Json) ?? [];
        yield return Craft.Evaluate(catalog, recipe, placements);
    }

    private CardTemplate? SeedFallback(bool outer, double targetStructure)
    {
        return SeedCards().Where(card => card.Valid && card.Strikes == 0 &&
                (outer ? card.OuterStructure : card.InnerStructure) == targetStructure)
            .OrderByDescending(card => card.Total).FirstOrDefault();
    }

    private void RestorePrevious()
    {
        RecipeResult? previous = Storage.Read<RecipeResult>(Path.Combine(Storage.State, "recipes", $"{recipe.Id:00}.json"));
        if (previous is null || previous.Policy != Storage.Policy || previous.Snapshot != catalog.Data.Id) return;
        foreach (string direction in new[] { "inner", "outer" })
        {
            bool outer = direction == "outer";
            double target = previous.StructureScores.GetValueOrDefault(direction,
                outer ? MaximumStructure(true) : MaximumStructure(false) / 2d);
            CardTemplate? card = previous.Groups.Values.SelectMany(g => g.Best)
                .Where(pair => pair.Key.StartsWith(direction + ":", StringComparison.Ordinal))
                .Select(pair => previous.Cards.GetValueOrDefault(pair.Value)).Where(c => c is not null)
                .Select(c => Craft.Evaluate(catalog, recipe, c!.Placements))
                .Where(c => c.Valid && c.Strikes == 0 && (outer ? c.OuterStructure : c.InnerStructure) == target)
                .OrderByDescending(c => c.Total).FirstOrDefault();
            if (card is not null)
            {
                Result.StructureScores[direction] = target;
                AddCandidate(card, direction);
            }
        }
    }

    private bool HasDirection(string direction) => Result.Groups.Values.Any(g => g.Best.Keys.Any(k => k.StartsWith(direction + ":", StringComparison.Ordinal)));

    private void AddCandidate(CardTemplate card, string direction)
    {
        string key = string.Join(',', card.Group);
        if (!Result.Groups.TryGetValue(key, out GroupState? group)) Result.Groups[key] = group = new GroupState { Key = card.Group };
        foreach (int goal in Enumerable.Range(0, 3))
        {
            string goalKey = $"{direction}:{Goals[goal]}";
            if (!group.Best.TryGetValue(goalKey, out string? old) || Craft.Better(card, Result.Cards[old], goal))
            { Result.Cards[card.Id] = card; group.Best[goalKey] = card.Id; }
        }
    }

    public void Save()
    {
        HashSet<string> used = Result.Groups.Values.SelectMany(g => g.Best.Values).ToHashSet();
        Result.Cards = Result.Cards.Where(pair => used.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
        foreach (GroupState group in Result.Groups.Values) { group.Complete = true; group.Infeasible = false; }
        Result.Complete = Result.BoundedFinalized; Result.Updated = DateTimeOffset.UtcNow;
        Storage.Write(Path.Combine(Storage.State, "recipes", $"{recipe.Id:00}.json"), Result);
    }

    private sealed record Choice(Placement Placement, (Hex Cell, int Color)[] Matches, bool Outside);
}
