using System.Diagnostics;
using System.Text.Json;
using Google.OrTools.Sat;

namespace InFalsusCalc;

/// <summary>跨全部回想的统一制卡清单与最少性证明；不代表配队得分的全局最优。</summary>
internal sealed class CollectionSummary
{
    /// <summary>技能时机守恒、原生结算操作序列相同的固定卡牌分配空间。</summary>
    public string Scope { get; init; } = "fixed_cards_colors_timing_and_exact_battle_schedule";
    /// <summary>参与联合优化的配队数。</summary>
    public int Teams { get; init; }
    /// <summary>原始不同成品数，包含颜色及技能顺序。</summary>
    public int Before { get; init; }
    /// <summary>输出不同成品数。</summary>
    public int After { get; init; }
    /// <summary>完整候选模型给出的安全整数下界。</summary>
    public int LowerBound { get; init; }
    /// <summary>仅当上下界闭合时标记最少；预算结束保留best_found。</summary>
    public string Status { get; init; } = "best_found";
    /// <summary>排除的非等价完整队伍组合数。</summary>
    public int Rejected { get; init; }
    /// <summary>枚举、求解和等价核对的总秒数。</summary>
    public double Seconds { get; init; }
    /// <summary>各回想共用的实际成品及可复现材料，一种成品只需制作一张。</summary>
    public DeckCardResult[] Cards { get; init; } = [];
}

/// <summary>固定卡牌与颜色，通过等价技能分配最小化全部配队的成品并集。</summary>
internal sealed class DeckCollection
{
    private readonly Catalog catalog;
    private readonly IReadOnlyDictionary<string, CardTemplate> templates;
    private readonly CancellationToken token;
    private readonly Dictionary<string, AssignedMaterial[]?> materials = [];

    /// <summary>一个固定位置可选的合法成品。</summary>
    /// <param name="Card">实际技能顺序及材料。</param>
    /// <param name="Key">跨回想共享的成品身份。</param>
    /// <param name="Signatures">各技能在本位置的完整时机编码。</param>
    private sealed record Variant(DeckCardResult Card, string Key, string[] Signatures);

    /// <summary>一个配队位置的互斥配置变量。</summary>
    /// <param name="Variants">与变量一一对应的成品。</param>
    /// <param name="Chosen">恰好一个为真的选择变量。</param>
    private sealed record Position(Variant[] Variants, BoolVar[] Chosen);

    /// <summary>创建本次合并专用的材料缓存，不改变模板与原搜索结果。</summary>
    /// <param name="catalog">游戏资源与敌方配队。</param>
    /// <param name="templates">当前报告的有效模板库。</param>
    /// <param name="token">取消信号。</param>
    public DeckCollection(Catalog catalog, IReadOnlyDictionary<string, CardTemplate> templates, CancellationToken token)
    {
        this.catalog = catalog;
        this.templates = templates;
        this.token = token;
    }

    /// <summary>成品不包含战斗位置；同模板同颜色但技能顺序不同仍需分别制作。</summary>
    /// <param name="card">待识别成品。</param>
    /// <returns>无歧义的成品身份。</returns>
    private static string Key(DeckCardResult card) => $"{card.Template}:{card.Color}:{string.Join(',', card.Traits)}";

    /// <summary>编码每个效果在十个阶段边界的触发或持续状态，包含最后阶段结束的范围外状态。</summary>
    /// <param name="trait">保持数量守恒的技能。</param>
    /// <param name="card">接收技能的固定卡牌。</param>
    /// <param name="slot">零基卡位。</param>
    /// <returns>技能编号及按原始效果顺序排列的时机掩码。</returns>
    private string Signature(int trait, CardTemplate card, int slot)
    {
        int start = Math.Max(0, slot - card.Left), end = Math.Min(4, slot + card.Right);
        List<int> masks = [];
        foreach (TraitEffect effect in catalog.Traits[trait].Effects)
        {
            int mask = 0;
            for (int boundary = 0; boundary < 10; boundary++)
            {
                int phase = boundary / 2;
                bool ending = boundary % 2 == 1;
                int effective = boundary == 9 ? -1 : phase;
                bool own = effective == slot, inside = effective >= start && effective <= end;
                bool active = effect.Condition switch
                {
                    1 => ending && phase == slot,
                    2 => ending && phase >= start && phase <= end,
                    3 => ending && phase == end,
                    4 => !own,
                    5 => !inside,
                    6 => own,
                    7 => inside,
                    8 => !ending && phase == start,
                    9 => !ending && phase >= start && phase <= end,
                    10 => !ending && phase == slot,
                    _ => throw new InvalidDataException($"未支持的技能条件{effect.Condition}。")
                };
                if (active) mask |= 1 << boundary;
            }
            masks.Add(mask);
        }
        return $"{trait}:{string.Join(',', masks)}";
    }

    /// <summary>完整枚举最多三个不同技能的有序集合；空槽允许在卡间转移，技能总数由队伍守恒约束保证。</summary>
    /// <param name="source">固定模板与颜色的原成品。</param>
    /// <param name="slot">零基位置。</param>
    /// <param name="skills">原队伍拥有的不同技能。</param>
    /// <param name="required">原队伍各技能时机及其份数。</param>
    /// <returns>所有材料可行且时机允许的候选。</returns>
    private Variant[] Variants(DeckCardResult source, int slot, int[] skills, Dictionary<string, int> required)
    {
        CardTemplate card = templates[source.Template];
        var eligible = skills.Select(id => (Id: id, Signature: Signature(id, card, slot)))
            .Where(item => required.ContainsKey(item.Signature)).ToArray();
        List<Variant> result = [];
        List<int> selected = [];
        Expand();
        return result.ToArray();

        void Expand()
        {
            token.ThrowIfCancellationRequested();
            int[] traits = selected.ToArray();
            string materialKey = $"{card.Id}:{string.Join(',', traits.Order())}";
            if (!materials.TryGetValue(materialKey, out AssignedMaterial[]? assignment))
                materials[materialKey] = assignment = Craft.Assign(catalog, card, traits);
            // 材料不可行集合的超集也不可行；这是完整性不变的剪枝。
            if (assignment is null) return;
            DeckCardResult choice = new() { Template = source.Template, Color = source.Color, Traits = traits, Materials = assignment };
            result.Add(new(choice, Key(choice), traits.Select(id => Signature(id, card, slot)).ToArray()));
            if (selected.Count == card.Slots) return;
            foreach (var item in eligible)
                if (!selected.Contains(item.Id))
                {
                    selected.Add(item.Id);
                    Expand();
                    selected.RemoveAt(selected.Count - 1);
                }
        }
    }

    /// <summary>把报告成品投影为独立战斗输入。</summary>
    /// <param name="cards">固定五卡配置。</param>
    /// <returns>按原位置排列的战斗卡。</returns>
    private BattleCard[] BattleCards(DeckCardResult[] cards) => cards.Select(card =>
        new DeckChoice(templates[card.Template], card.Color, card.Traits).ToBattleCard()).ToArray();

    /// <summary>联合最小化成品并集；不等价队伍通过惰性禁配约束排除，未验证候选永不发布。</summary>
    /// <param name="decks">全部回想及全部惩罚档。</param>
    /// <param name="seconds">本次枚举和求解的总秒数预算。</param>
    /// <param name="threads">CP-SAT并行线程数。</param>
    /// <returns>保持原结果的队伍与真实制卡数量/证明状态。</returns>
    public (DeckResult[] Decks, CollectionSummary Summary) Optimize(DeckResult[] decks, int seconds, int threads)
    {
        Stopwatch timer = Stopwatch.StartNew();
        Validate(decks);
        int before = decks.SelectMany(d => d.Cards).Select(Key).Distinct().Count();
        int lower = decks.SelectMany(d => d.Cards).Select(c => (c.Template, c.Color)).Distinct().Count();
        int rejected = 0;
        DeckCardResult[][] best = decks.Select(d => d.Cards).ToArray();
        int bestCount = before;
        CpModel model = new();
        Dictionary<string, BoolVar> used = [];
        Dictionary<string, List<BoolVar>> uses = [];
        Position[][] positions = new Position[decks.Length][];
        int[] representatives = new int[decks.Length];
        Dictionary<(int Encounter, string Cards), int> repeated = [];
        HashSet<string> originalKeys = decks.SelectMany(d => d.Cards).Select(Key).ToHashSet();
        for (int team = 0; team < decks.Length; team++)
        {
            token.ThrowIfCancellationRequested();
            if (timer.Elapsed.TotalSeconds >= seconds) return Finish();
            DeckCardResult[] cards = decks[team].Cards;
            var context = (decks[team].Encounter, string.Join(';', cards.Select(Key)));
            if (repeated.TryGetValue(context, out int previous))
            {
                // 同一回想的不同惩罚档若固定五卡完全相同，可共用选择：取任一档的合法解不会增加成品并集。
                positions[team] = positions[previous];
                representatives[team] = previous;
                continue;
            }
            representatives[team] = team;
            repeated[context] = team;
            Dictionary<string, int> required = cards.SelectMany((card, slot) => card.Traits
                .Select(id => Signature(id, templates[card.Template], slot))).GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            int[] skills = cards.SelectMany(c => c.Traits).Distinct().Order().ToArray();
            Dictionary<string, List<BoolVar>> occurrences = required.Keys.ToDictionary(k => k, _ => new List<BoolVar>());
            positions[team] = new Position[5];
            for (int slot = 0; slot < 5; slot++)
            {
                Variant[] variants = Variants(cards[slot], slot, skills, required);
                BoolVar[] chosen = variants.Select((_, i) => model.NewBoolVar($"t{team}s{slot}v{i}")).ToArray();
                positions[team][slot] = new(variants, chosen);
                model.AddExactlyOne(chosen);
                if (!variants.Any(v => v.Key == Key(cards[slot])))
                    throw new InvalidDataException("原配队未进入可行候选，不能进行制卡合并。");
                for (int i = 0; i < variants.Length; i++)
                {
                    Variant variant = variants[i];
                    if (!used.TryGetValue(variant.Key, out BoolVar? present))
                    {
                        used[variant.Key] = present = model.NewBoolVar($"card{used.Count}");
                        uses[variant.Key] = [];
                        model.AddHint(present, originalKeys.Contains(variant.Key) ? 1 : 0);
                    }
                    model.AddImplication(chosen[i], present);
                    uses[variant.Key].Add(chosen[i]);
                    model.AddHint(chosen[i], variant.Key == Key(cards[slot]) ? 1 : 0);
                    foreach (string signature in variant.Signatures) occurrences[signature].Add(chosen[i]);
                }
            }
            foreach (var pair in required) model.Add(LinearExpr.Sum(occurrences[pair.Key]) == pair.Value);
        }
        foreach (var pair in used) model.Add(LinearExpr.Sum(uses[pair.Key]) >= pair.Value);
        LinearExpr count = LinearExpr.Sum(used.Values);
        model.Add(count <= before);
        model.Minimize(count);
        Console.WriteLine($"统一制卡：{decks.Length}套/{repeated.Count}个独立配置，原{before}张，结构下界{lower}张，候选成品{used.Count}种。");
        Dictionary<(int Team, int Rating), long[]> schedules = [];
        HashSet<(int Team, string Cards)> accepted = [];
        double seedDeadline = Math.Min(seconds * .4, timer.Elapsed.TotalSeconds + 60);
        while (timer.Elapsed.TotalSeconds < seedDeadline)
        {
            var seed = Seed(best, Equivalent, timer, seedDeadline, threads);
            if (seed is null) break;
            // 小模型可能为相同上下文选出不同等价解；统一取代表档只会缩小成品并集。
            var normalized = seed.Select((_, team) => seed[representatives[team]]).ToArray();
            int improved = normalized.SelectMany(cards => cards).Select(Key).Distinct().Count();
            if (improved >= bestCount) break;
            best = normalized;
            bestCount = improved;
            Console.WriteLine($"统一制卡：等价换位起点{bestCount}张。");
        }
        if (bestCount < before)
        {
            model.ClearHints();
            HashSet<string> seedKeys = best.SelectMany(cards => cards).Select(Key).ToHashSet();
            foreach (var pair in used) model.AddHint(pair.Value, seedKeys.Contains(pair.Key) ? 1 : 0);
            for (int team = 0; team < decks.Length; team++)
            {
                if (representatives[team] != team) continue;
                for (int slot = 0; slot < 5; slot++)
                    for (int i = 0; i < positions[team][slot].Variants.Length; i++)
                        model.AddHint(positions[team][slot].Chosen[i], positions[team][slot].Variants[i].Key == Key(best[team][slot]) ? 1 : 0);
            }
            Console.WriteLine($"统一制卡：等价换位起点{bestCount}张，继续完整模型证明。");
        }
        model.Add(count < bestCount);
        double slice = 5;
        while (timer.Elapsed.TotalSeconds < seconds && lower < bestCount)
        {
            token.ThrowIfCancellationRequested();
            CpSolver solver = new()
            {
                // 分片给候选验证留出预算；未经等价核对的求解器解不能成为输出上界。
                StringParameters = FormattableString.Invariant($"max_time_in_seconds:{Math.Max(.001, Math.Min(slice, (seconds - timer.Elapsed.TotalSeconds) * .8))} num_search_workers:{threads} random_seed:1")
            };
            CpSolverStatus status;
            using (token.Register(solver.StopSearch)) status = solver.Solve(model);
            token.ThrowIfCancellationRequested();
            if (status == CpSolverStatus.ModelInvalid) throw new InvalidOperationException(solver.ResponseStats());
            if (status == CpSolverStatus.Infeasible)
            {
                lower = bestCount;
                break;
            }
            if (status is CpSolverStatus.Optimal or CpSolverStatus.Feasible or CpSolverStatus.Unknown)
                lower = Math.Max(lower, (int)Math.Ceiling(solver.BestObjectiveBound - 1e-6));
            if (status == CpSolverStatus.Unknown) { slice = Math.Min(30, slice * 2); continue; }
            slice = 5;
            if (status is not (CpSolverStatus.Optimal or CpSolverStatus.Feasible)) break;
            bool valid = true;
            DeckCardResult[][] candidate = new DeckCardResult[decks.Length][];
            for (int team = 0; team < decks.Length; team++)
            {
                token.ThrowIfCancellationRequested();
                if (timer.Elapsed.TotalSeconds >= seconds) return Finish();
                int[] indices = positions[team].Select(p => Array.FindIndex(p.Chosen, variable => solver.Value(variable) != 0)).ToArray();
                candidate[team] = positions[team].Select((p, slot) => p.Variants[indices[slot]].Card).ToArray();
                string identity = string.Join(';', candidate[team].Select(Key));
                if (accepted.Contains((representatives[team], identity))) continue;
                if (Equivalent(team, candidate[team])) accepted.Add((representatives[team], identity));
                else
                {
                    model.Add(LinearExpr.Sum(positions[team].Select((p, slot) => p.Chosen[indices[slot]])) <= 4);
                    rejected++;
                    valid = false;
                }
            }
            if (!valid)
            {
                Console.WriteLine($"统一制卡：已排除{rejected}种非等价队伍，保留{bestCount}张，下界{lower}张。");
                continue;
            }
            best = candidate;
            bestCount = best.SelectMany(cards => cards).Select(Key).Distinct().Count();
            Console.WriteLine($"统一制卡：已验证{bestCount}张，下界{lower}张，已排除{rejected}种非等价队伍。");
            if (status == CpSolverStatus.Optimal) { lower = bestCount; break; }
            model.Add(count < bestCount);
            // 新上界排除了旧提示，移除它，避免反复尝试已知过大的基准。
            model.ClearHints();
        }
        return Finish();

        bool Equivalent(int team, DeckCardResult[] cards)
        {
            team = representatives[team];
            DeckResult source = decks[team];
            if (source.Cards.Select(Key).SequenceEqual(cards.Select(Key))) return true;
            Encounter encounter = catalog.Data.Encounters.Single(e => e.Id == source.Encounter);
            BattleCard[] original = BattleCards(source.Cards), proposed = BattleCards(cards);
            for (int rating = 1; rating <= 20; rating++)
            {
                token.ThrowIfCancellationRequested();
                if (!schedules.TryGetValue((team, rating), out long[]? baseline))
                    schedules[(team, rating)] = baseline = new Battle(catalog, original, encounter, rating).Schedule();
                if (!baseline.SequenceEqual(new Battle(catalog, proposed, encounter, rating).Schedule())) return false;
                BattleResult actual = new Battle(catalog, proposed, encounter, rating).Run(true, true);
                if (JsonSerializer.Serialize(actual, Storage.Json) != JsonSerializer.Serialize(source.RatingBattles[rating], Storage.Json)) return false;
                if (rating == source.Rating && JsonSerializer.Serialize(actual, Storage.Json) != JsonSerializer.Serialize(source.Battle, Storage.Json)) return false;
            }
            return true;
        }

        (DeckResult[], CollectionSummary) Finish()
        {
            token.ThrowIfCancellationRequested();
            DeckResult[] output = decks.Select((source, i) =>
            {
                if (source.Cards.Select(Key).SequenceEqual(best[i].Select(Key))) return source;
                // 使用完整序列化副本保留既有结果及元数据；新技能配置不继承旧配置的邻域闭合声明。
                var node = JsonSerializer.SerializeToNode(source, Storage.Json)!;
                node["cards"] = JsonSerializer.SerializeToNode(best[i], Storage.Json);
                node["neighborhood_complete"] = false;
                node["method"] = source.Method + "；固定卡牌的跨回想等价技能分配";
                return node.Deserialize<DeckResult>(Storage.Json)!;
            }).ToArray();
            DeckCardResult[] cards = best.SelectMany(c => c).DistinctBy(Key).OrderBy(Key, StringComparer.Ordinal).ToArray();
            if (lower > cards.Length) throw new InvalidOperationException("制卡下界超过已验证可行解。");
            return (output, new CollectionSummary
            {
                Teams = decks.Length, Before = before, After = cards.Length, LowerBound = lower,
                Status = lower == cards.Length ? "optimal" : "best_found", Rejected = rejected,
                Seconds = timer.Elapsed.TotalSeconds, Cards = cards
            });
        }
    }

    /// <summary>用已验证的单次技能换位构造联合可行起点；仅改善上界，不裁剪完整模型或提供最少性证明。</summary>
    /// <param name="sources">当前已验证的全部队伍。</param>
    /// <param name="equivalent">包含全等级结果和操作序列的严格等价判断。</param>
    /// <param name="timer">整体预算计时器。</param>
    /// <param name="deadline">本轮起点优化截止时的整体累计秒数。</param>
    /// <param name="threads">求解线程数。</param>
    /// <returns>预算内找到的合法联合分配，求解无可行解时为空。</returns>
    private DeckCardResult[][]? Seed(DeckCardResult[][] sources, Func<int, DeckCardResult[], bool> equivalent,
        Stopwatch timer, double deadline, int threads)
    {
        // 该小模型允许多个回想同时换位，能跨过“只改一队反而多做一张卡”的局部障碍。
        CpModel model = new();
        Dictionary<string, BoolVar> used = [];
        List<(DeckCardResult[] Cards, BoolVar Chosen)>[] choices = new List<(DeckCardResult[], BoolVar)>[sources.Length];
        for (int team = 0; team < sources.Length; team++)
        {
            token.ThrowIfCancellationRequested();
            DeckCardResult[] source = sources[team];
            List<DeckCardResult[]> alternatives = [source];
            HashSet<string> visited = [string.Join(';', source.Select(Key))];
            var slots = source.SelectMany((c, owner) => Enumerable.Range(0,
                Math.Min(c.Traits.Length + 1, templates[c.Template].Slots)).Select(index => (Owner: owner, Index: index))).ToArray();
            for (int a = 0; a < slots.Length && timer.Elapsed.TotalSeconds < deadline; a++)
                for (int b = a + 1; b < slots.Length && timer.Elapsed.TotalSeconds < deadline; b++)
                {
                    token.ThrowIfCancellationRequested();
                    var left = slots[a];
                    var right = slots[b];
                    int first = source[left.Owner].Traits.ElementAtOrDefault(left.Index);
                    int second = source[right.Owner].Traits.ElementAtOrDefault(right.Index);
                    if (first == second) continue;
                    CardTemplate leftCard = templates[source[left.Owner].Template], rightCard = templates[source[right.Owner].Template];
                    if (first > 1 && Signature(first, leftCard, left.Owner) != Signature(first, rightCard, right.Owner)
                        || second > 1 && Signature(second, rightCard, right.Owner) != Signature(second, leftCard, left.Owner)) continue;
                    int[][] skills = source.Select(c => c.Traits.Concat(c.Traits.Length < templates[c.Template].Slots ? new[] { 0 } : []).ToArray()).ToArray();
                    (skills[left.Owner][left.Index], skills[right.Owner][right.Index]) = (second, first);
                    DeckCardResult[] candidate = (DeckCardResult[])source.Clone();
                    bool legal = true;
                    foreach (int owner in new[] { left.Owner, right.Owner }.Distinct())
                    {
                        int[] traits = skills[owner].Where(t => t > 1).ToArray();
                        AssignedMaterial[]? assignment = traits.Distinct().Count() == traits.Length
                            ? Craft.Assign(catalog, templates[source[owner].Template], traits) : null;
                        if (assignment is null) { legal = false; break; }
                        candidate[owner] = new() { Template = source[owner].Template, Color = source[owner].Color, Traits = traits, Materials = assignment };
                    }
                    if (legal && visited.Add(string.Join(';', candidate.Select(Key))) && equivalent(team, candidate)) alternatives.Add(candidate);
                }
            choices[team] = [];
            foreach (DeckCardResult[] cards in alternatives)
            {
                BoolVar chosen = model.NewBoolVar($"seed{team}v{choices[team].Count}");
                choices[team].Add((cards, chosen));
                model.AddHint(chosen, ReferenceEquals(cards, source) ? 1 : 0);
                foreach (string key in cards.Select(Key))
                {
                    if (!used.TryGetValue(key, out BoolVar? present)) used[key] = present = model.NewBoolVar($"seedcard{used.Count}");
                    model.AddImplication(chosen, present);
                }
            }
            model.AddExactlyOne(choices[team].Select(c => c.Chosen));
        }
        if (timer.Elapsed.TotalSeconds >= deadline) return null;
        model.Minimize(LinearExpr.Sum(used.Values));
        model.Add(LinearExpr.Sum(used.Values) <= sources.SelectMany(cards => cards).Select(Key).Distinct().Count());
        CpSolver solver = new()
        {
            StringParameters = FormattableString.Invariant($"max_time_in_seconds:{Math.Min(10, deadline - timer.Elapsed.TotalSeconds)} num_search_workers:{threads} random_seed:1")
        };
        CpSolverStatus status;
        using (token.Register(solver.StopSearch)) status = solver.Solve(model);
        token.ThrowIfCancellationRequested();
        if (status == CpSolverStatus.ModelInvalid) throw new InvalidOperationException(solver.ResponseStats());
        return status is CpSolverStatus.Optimal or CpSolverStatus.Feasible
            ? choices.Select(team => team.Single(c => solver.Value(c.Chosen) != 0).Cards).ToArray() : null;
    }

    /// <summary>拒绝不完整或不合法报告，确保优化基准自身属于完整可行空间。</summary>
    /// <param name="decks">全部待合并队伍。</param>
    private void Validate(DeckResult[] decks)
    {
        var expected = catalog.Data.Encounters.SelectMany(e => ConfidenceAnalysis.RetainedStrikes.Select(s => (e.Id, s))).Order().ToArray();
        if (!decks.Select(d => (d.Encounter, d.MaxStrikes)).Order().SequenceEqual(expected))
            throw new InvalidDataException("统一制卡必须包含全部回想的0、1、2惩罚档。");
        foreach (DeckResult deck in decks)
        {
            token.ThrowIfCancellationRequested();
            if (deck.Cards.Length != 5 || deck.Rating != Battle.SearchRating || deck.Notes != Battle.Notes
                || !deck.Chromatic || deck.InitialHp != 100 || !deck.Battle.Passed
                || !deck.RatingBattles.Keys.Order().SequenceEqual(Enumerable.Range(1, 20)))
                throw new InvalidDataException("统一制卡的配队条件或等级结果不完整。");
            foreach (DeckCardResult card in deck.Cards)
            {
                if (!templates.TryGetValue(card.Template, out CardTemplate? template) || !template.Colors.Contains(card.Color)
                    || template.Strikes > deck.MaxStrikes || card.Traits.Length > template.Slots
                    || card.Traits.Any(t => t <= 1 || !catalog.Traits.ContainsKey(t))
                    || card.Traits.Distinct().Count() != card.Traits.Length || Craft.Assign(catalog, template, card.Traits) is null)
                    throw new InvalidDataException("原配队包含不可制作的成品。");
            }
            if (deck.Cards.Select(c => templates[c.Template].BaseId).Distinct().Count() != 5)
                throw new InvalidDataException("原配队包含同名卡。");
            Encounter encounter = catalog.Data.Encounters.Single(e => e.Id == deck.Encounter);
            BattleCard[] cards = BattleCards(deck.Cards);
            for (int rating = 1; rating <= 20; rating++)
            {
                BattleResult actual = new Battle(catalog, cards, encounter, rating).Run(true, true);
                if (JsonSerializer.Serialize(actual, Storage.Json) != JsonSerializer.Serialize(deck.RatingBattles[rating], Storage.Json)
                    || rating == deck.Rating && JsonSerializer.Serialize(actual, Storage.Json) != JsonSerializer.Serialize(deck.Battle, Storage.Json))
                    throw new InvalidDataException($"回想{deck.Encounter}/惩罚{deck.MaxStrikes}/等级{rating}原结果与当前结算不一致。");
            }
        }
    }
}
