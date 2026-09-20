using System.Diagnostics;

namespace InFalsusCalc;

/// <summary>一次选定颜色和技能顺序的卡牌配置。</summary>
/// <param name="Template">实际可制作的模板。</param>
/// <param name="Color">模板允许的最终颜色。</param>
/// <param name="Traits">按装备顺序排列的合法技能。</param>
internal sealed record DeckChoice(CardTemplate Template, int Color, int[] Traits)
{
    /// <summary>只传递战斗字段，不复制几何与材料大数组。</summary>
    /// <returns>当前实际装备的卡牌。</returns>
    public BattleCard ToBattleCard() => new() { Name = Template.Name, Color = Color, Power = Template.Power,
        Fortitude = Template.Fortitude, Left = Template.Left, Right = Template.Right, Traits = Traits };
}

/// <summary>最终通关配置中的卡牌和可复现材料。</summary>
internal sealed class DeckCardResult
{
    /// <summary>成果模板编号。</summary>
    public string Template { get; init; } = "";
    /// <summary>实际选定颜色。</summary>
    public int Color { get; init; }
    /// <summary>实际装备顺序。</summary>
    public int[] Traits { get; init; } = [];
    /// <summary>需要携带技能的具体粒子。</summary>
    public AssignedMaterial[] Materials { get; init; } = [];
}

/// <summary>在通关约束下找到的高遭遇分配置，不冒称完整组合空间的最优证明。</summary>
internal sealed class DeckResult
{
    /// <summary>资源和策略指纹。</summary>
    public string Library { get; init; } = "";
    /// <summary>回想编号。</summary>
    public int Encounter { get; init; }
    /// <summary>固定谱面等级。</summary>
    public int Rating { get; init; } = InFalsusCalc.Battle.SearchRating;
    /// <summary>联觉固定开启。</summary>
    public bool Chromatic { get; init; } = true;
    /// <summary>固定全EXACT判定数。</summary>
    public int Notes { get; init; } = 25;
    /// <summary>正常可解锁条件的初始HP。</summary>
    public int InitialHp { get; init; } = 100;
    /// <summary>按游戏槽位排列的五张卡。</summary>
    public DeckCardResult[] Cards { get; init; } = [];
    /// <summary>逐判定复算的对局。</summary>
    public required BattleResult Battle { get; init; }
    /// <summary>同一推荐配队在等级1至20下的完整复算结果。</summary>
    public Dictionary<int, BattleResult> RatingBattles { get; init; } = [];
    /// <summary>实际比较的配置数量。</summary>
    public long Evaluated { get; init; }
    /// <summary>本次真实计算用时。</summary>
    public double Seconds { get; init; }
    /// <summary>以遭遇得分为目标的有限搜索结果。</summary>
    public string Optimality { get; init; } = "best_found";
    /// <summary>公开的搜索范围说明。</summary>
    public string Method { get; init; } = "通关约束下按遭遇得分排序，多起点、整卡替换、位置交换与技能顺序搜索";
}

/// <summary>以真实遭遇得分选择通关队伍，失败方案只用于寻找可行起点。</summary>
internal sealed class DeckSearch
{
    /// <summary>对手画像、70模板筛选、技能后验合法性和多状态骨架搜索版本。</summary>
    public const string Policy = "opponent-beam-v3";
    private readonly Catalog catalog;
    private readonly CardTemplate[] templates;
    private readonly Encounter encounter;
    private readonly string library;
    private readonly CancellationToken token;
    private readonly int[] allowed;
    private readonly Dictionary<string, int[][]> combinationCache = [];
    private readonly HashSet<int>[] isolation = [[], [], [], [], []];
    private readonly HashSet<string> visited = [];
    private DeckChoice[] team = [];
    private BattleResult? localBest;
    private DeckChoice[]? winner;
    private BattleResult? winningBattle;
    private long evaluated;

    /// <summary>为一个回想建立独立搜索缓存，允许不同回想并行计算。</summary>
    /// <param name="catalog">只读游戏资源。</param>
    /// <param name="templates">网页也使用的同一代表库。</param>
    /// <param name="encounter">目标回想。</param>
    /// <param name="library">固定模板库指纹。</param>
    /// <param name="token">停止信号。</param>
    public DeckSearch(Catalog catalog, CardTemplate[] templates, Encounter encounter, string library, CancellationToken token)
    {
        this.catalog = catalog; this.encounter = encounter; this.library = library; this.token = token;
        int maxPower = templates.Max(c => c.Power), maxFortitude = templates.Max(c => c.Fortitude), maxTotal = templates.Max(c => c.Total);
        this.templates = templates.Where(c => c.Slots == 3 && c.Left + c.Right >= 4
            || c.Power >= maxPower * .8 || c.Fortitude >= maxFortitude * .8 || c.Total >= maxTotal * .8).ToArray();
        if (this.templates.Select(c => c.BaseId).Distinct().Count() < 5) throw new InvalidDataException("二阶段筛选后不足五种不同名卡。");
        allowed = this.templates.SelectMany(c => c.AvailableTraits).Distinct().Order().ToArray();
        for (int slot = 0; slot < 5; slot++)
            foreach (int tid in encounter.Cards[slot].Traits.Where(t => t > 1))
                foreach (TraitEffect effect in catalog.Traits[tid].Effects.Where(e => e.Kind == 4))
                    for (int phase = 0; phase < 5; phase++)
                    {
                        BattleCard card = encounter.Cards[slot]; bool own = phase == slot;
                        bool inside = Math.Max(0, slot - card.Left) <= phase && phase <= Math.Min(4, slot + card.Right);
                        if (effect.Condition switch { 4 => !own, 5 => !inside, 6 => own, 7 => inside, _ => false }) isolation[phase].Add((int)effect.Value);
                    }
    }

    /// <summary>通关后的首目标始终是遭遇分，不能被生存裕量反超。</summary>
    /// <param name="candidate">新对局。</param>
    /// <param name="previous">已有对局。</param>
    /// <returns>新对局是否更符合目标。</returns>
    private static bool Better(BattleResult candidate, BattleResult? previous)
    {
        if (previous is null) return true;
        if (candidate.Passed != previous.Passed) return candidate.Passed;
        double first = candidate.Passed ? candidate.Score : candidate.Margin;
        double old = previous.Passed ? previous.Score : previous.Margin;
        if (first != old) return first > old;
        if (candidate.Passed && candidate.RawScore != previous.RawScore) return candidate.RawScore > previous.RawScore;
        return candidate.Passed ? candidate.MinimumHp > previous.MinimumHp : candidate.Score > previous.Score;
    }

    /// <summary>只比较合法不同名五卡；改善项采用逐判定原生顺序复算。</summary>
    /// <param name="candidate">完整待选队伍。</param>
    /// <returns>是否更新当前起点的局部最佳。</returns>
    private bool Consider(DeckChoice[] candidate)
    {
        if ((evaluated & 63) == 0) token.ThrowIfCancellationRequested();
        if (candidate.Select(c => c.Template.BaseId).Distinct().Count() != 5) return false;
        string key = string.Join(';', candidate.Select(c => $"{c.Template.Id}:{c.Color}:{string.Join(',', c.Traits)}"));
        if (!visited.Add(key)) return false;
        evaluated++;
        BattleCard[] cards = candidate.Select(c => c.ToBattleCard()).ToArray();
        BattleResult value = new Battle(catalog, cards, encounter).Run(false);
        if (!Better(value, localBest)) return false;
        value = new Battle(catalog, cards, encounter).Run(true);
        if (!Better(value, localBest)) return false;
        localBest = value; team = candidate;
        if (value.Passed && Better(value, winningBattle)) { winningBattle = value; winner = (DeckChoice[])candidate.Clone(); }
        return true;
    }

    /// <summary>展开同一材料能力下最多三个不同特性的合法集合。</summary>
    /// <param name="card">用于取得槽数与材料能力的模板。</param>
    /// <returns>包含空技能集合的合法组合。</returns>
    private int[][] Combinations(CardTemplate card)
    {
        string key = card.Slots.ToString();
        if (combinationCache.TryGetValue(key, out int[][]? cached)) return cached;
        List<int[]> result = [[]];
        for (int a = 0; a < allowed.Length && card.Slots > 0; a++)
        {
            int[] one = [allowed[a]]; result.Add(one);
            for (int b = a + 1; b < allowed.Length && card.Slots > 1; b++)
            {
                int[] two = [allowed[a], allowed[b]]; result.Add(two);
                for (int c = b + 1; c < allowed.Length && card.Slots > 2; c++) result.Add([allowed[a], allowed[b], allowed[c]]);
            }
        }
        return combinationCache[key] = result.ToArray();
    }

    /// <summary>近似收益只用来缩小候选池，最终队伍仍按完整遭遇得分比较。</summary>
    /// <param name="slot">拟放入的卡槽。</param>
    /// <param name="card">范围及材料能力。</param>
    /// <param name="offense">当前攻击引导权重。</param>
    /// <param name="defense">当前防御引导权重。</param>
    /// <returns>排名靠前的合法技能组合和估计增益。</returns>
    private (int[] Traits, double Gain)[] BestSkills(int slot, CardTemplate card, double offense, double defense)
    {
        int start = Math.Max(0, slot - card.Left), end = Math.Min(4, slot + card.Right), width = end - start + 1;
        Dictionary<int, double> gains = [];
        foreach (int tid in allowed)
        {
            double gain = 0;
            foreach (TraitEffect e in catalog.Traits[tid].Effects)
            {
                double share = e.Condition switch { 4 => .8, 5 => 1 - width / 5d, 6 => .2, 7 => width / 5d, _ => 0 };
                if (e.Kind is 1 or 129) gain += e.Value / 100 * share * offense;
                else if (e.Kind is 2 or 128) gain += e.Value / (100 + e.Value) * share * defense;
                else if (e.Kind == 3) gain -= e.Value / 100 * share * defense;
                else if (e.Kind is 8096 or 8097)
                {
                    int count = e.Condition is 2 or 9 ? width : 1;
                    if (e.Kind == 8097)
                    {
                        if (e.Condition == 8 && start == 0 || e.Condition == 10 && slot == 0) count = 0;
                        else if (e.Condition == 9 && start == 0) count--;
                    }
                    gain += e.Kind == 8096 ? e.Value / 100 * count / 180 * offense : e.Value / 100 * count / (100 + (localBest?.Healing[0] ?? 0)) * defense;
                }
            }
            gains[tid] = gain;
        }
        return Combinations(card).Select(skills => (Traits: skills, Gain: skills.Sum(t => gains[t])))
            .OrderByDescending(x => x.Gain).ThenBy(x => x.Traits.Length)
            .Where(x => Craft.CanAssign(catalog.Data.Profiles, card.Carriers, x.Traits)).Take(12).ToArray();
    }

    /// <summary>保留整体高收益候选和每种卡名的代表，避免只替换同一种强卡。</summary>
    /// <param name="slot">当前待替换位置。</param>
    /// <returns>优先考虑的实际卡牌配置。</returns>
    private DeckChoice[] Pool(int slot)
    {
        double p = Math.Max(1, team.Sum(c => c.Template.Power)), f = Math.Max(1, team.Sum(c => c.Template.Fortitude));
        double offense = 1, defense = 1;
        if (localBest is { Passed: false } && encounter.Mode == "connect")
        { if (localBest.MinimumHp <= 0 && localBest.EnemyProgress >= 100) offense = .25; else if (localBest.MinimumHp > 0 && localBest.EnemyProgress < 100) defense = .35; }
        int diversity = team.Select(c => c.Color).Where(c => c != 0).Distinct().Count();
        HashSet<int> others = team.Where((_, i) => i != slot).Select(c => c.Color).Where(c => c != 0).ToHashSet();
        Dictionary<string, (int[] Traits, double Gain)[]> skillsCache = [];
        PriorityQueue<DeckChoice, double> best = new(); Dictionary<int, (DeckChoice Choice, double Score)> byName = [];
        foreach (CardTemplate card in templates)
        {
            token.ThrowIfCancellationRequested();
            string signature = $"{card.Slots},{card.Left},{card.Right}:{string.Join(',', card.Carriers)}";
            if (!skillsCache.TryGetValue(signature, out var skills)) skillsCache[signature] = skills = BestSkills(slot, card, offense, defense);
            foreach (int color in card.Colors)
            {
                double advantage = color != 0 && Battle.Advantage[color] == encounter.Cards[slot].Color ? 1.25 : 1;
                double score = advantage * (card.Power / p * offense + card.Fortitude / f * defense);
                if (isolation[slot].Count == 1 && !isolation[slot].Contains(color)) score -= .3 * offense;
                int newDiversity = others.Count + (color != 0 && !others.Contains(color) ? 1 : 0);
                score += 2 * ((1 + .05 * newDiversity) / (1 + .05 * diversity) - 1);
                foreach (var skill in skills)
                {
                    DeckChoice choice = new(card, color, skill.Traits); double value = score + skill.Gain;
                    best.Enqueue(choice, value); if (best.Count > 64) best.Dequeue();
                    if (!byName.TryGetValue(card.BaseId, out var old) || value > old.Score) byName[card.BaseId] = (choice, value);
                }
            }
        }
        return best.UnorderedItems.Select(i => (Choice: i.Element, Score: i.Priority)).Concat(byName.Values)
            .OrderByDescending(x => x.Score).Select(x => x.Choice).Distinct().ToArray();
    }

    /// <summary>构建不同面板侧重的初始队伍，并照顾敌方颜色隔离条件。</summary>
    private DeckChoice[] Seed(int goal)
    {
        HashSet<int> used = []; List<DeckChoice> result = [];
        for (int slot = 0; slot < 5; slot++)
        {
            int position = slot;
            var options = templates.Where(c => !used.Contains(c.BaseId)).SelectMany(c => c.Colors.Select(color => new DeckChoice(c, color, [])));
            DeckChoice selected = options.OrderByDescending(c => isolation[position].Count != 1 || isolation[position].Contains(c.Color))
                .ThenByDescending(c => (goal == 3 ? Math.Sqrt((double)c.Template.Power * c.Template.Fortitude) : Craft.Panel(c.Template, goal))
                    * (c.Color != 0 && Battle.Advantage[c.Color] == encounter.Cards[position].Color ? 1.25 : 1)).First();
            used.Add(selected.Template.BaseId); result.Add(selected);
        }
        return result.ToArray();
    }

    /// <summary>按位置、卡名和攻防侧重预计算技能优先、材料后验合法的候选。</summary>
    private Dictionary<(int Slot, int BaseId, int Goal), DeckChoice[]> SeedOptions()
    {
        Dictionary<(int, int, int), DeckChoice[]> result = [];
        double maxPower = templates.Max(c => c.Power), maxFortitude = templates.Max(c => c.Fortitude);
        foreach (int slot in Enumerable.Range(0, 5))
            foreach (int baseId in templates.Select(c => c.BaseId).Distinct())
                for (int goal = 0; goal < 4; goal++)
                {
                    (double offense, double defense) = goal switch { 0 => (1, .15), 1 => (.15, 1), 2 => (1, 1), _ => (.7, 1.3) };
                    List<(DeckChoice Choice, double Value)> choices = [];
                    foreach (CardTemplate card in templates.Where(c => c.BaseId == baseId))
                        foreach (int color in card.Colors)
                        {
                            double advantage = color != 0 && Battle.Advantage[color] == encounter.Cards[slot].Color ? 1.25 : 1;
                            double panel = advantage * (offense * card.Power / maxPower + defense * card.Fortitude / maxFortitude);
                            if (isolation[slot].Count == 1 && !isolation[slot].Contains(color)) panel -= offense;
                            foreach (var skills in BestSkills(slot, card, offense, defense))
                                choices.Add((new(card, color, skills.Traits), panel + skills.Gain));
                        }
                    result[(slot, baseId, goal)] = choices.OrderByDescending(x => x.Value).Select(x => x.Choice).Distinct().Take(8).ToArray();
                }
        return result;
    }

    /// <summary>完整枚举不同名卡的有序五卡骨架。</summary>
    private static IEnumerable<int[]> Skeletons(int[] names)
    {
        int[] current = new int[5]; bool[] used = new bool[names.Length];
        return Walk(0);
        IEnumerable<int[]> Walk(int depth)
        {
            if (depth == current.Length) { yield return (int[])current.Clone(); yield break; }
            for (int i = 0; i < names.Length; i++)
            {
                if (used[i]) continue;
                used[i] = true; current[depth] = names[i];
                foreach (int[] value in Walk(depth + 1)) yield return value;
                used[i] = false;
            }
        }
    }

    /// <summary>从独立位置候选构造一个骨架起点；多样化分支优先补充新颜色。</summary>
    private static DeckChoice[] SkeletonSeed(int[] names, int goal, bool diversify,
        Dictionary<(int Slot, int BaseId, int Goal), DeckChoice[]> options)
    {
        HashSet<int> colors = []; DeckChoice[] result = new DeckChoice[5];
        for (int slot = 0; slot < 5; slot++)
        {
            DeckChoice[] candidates = options[(slot, names[slot], goal)];
            result[slot] = !diversify ? candidates[0] : candidates.Take(4).Select((choice, index) => (choice, index))
                .OrderByDescending(x => x.choice.Color != 0 && !colors.Contains(x.choice.Color)).ThenBy(x => x.index).First().choice;
            if (result[slot].Color != 0) colors.Add(result[slot].Color);
        }
        return result;
    }

    /// <summary>把候选按快速完整阶段计算放入固定宽度beam，封顶后继续比较原始公式分。</summary>
    private void AddBeam(DeckChoice[] candidate, PriorityQueue<DeckChoice[], double> beam, HashSet<string> seen)
    {
        string key = string.Join(';', candidate.Select(c => $"{c.Template.Id}:{c.Color}:{string.Join(',', c.Traits)}"));
        if (!seen.Add(key)) return;
        if ((evaluated & 63) == 0) token.ThrowIfCancellationRequested();
        evaluated++; BattleCard[] cards = candidate.Select(c => c.ToBattleCard()).ToArray();
        BattleResult value = new Battle(catalog, cards, encounter).Run(false);
        double priority = value.Passed ? 1e12 + value.RawScore : value.Margin;
        beam.Enqueue((DeckChoice[])candidate.Clone(), priority); if (beam.Count > 256) beam.Dequeue();
    }

    /// <summary>固定卡牌身体后，组合各位置的候选技能包并按完整战斗保留多种协同路线。</summary>
    private DeckChoice[][] SkillBeam(DeckChoice[] seed)
    {
        List<DeckChoice[]> states = [(DeckChoice[])seed.Clone()];
        for (int slot = 0; slot < 5; slot++)
        {
            CardTemplate card = seed[slot].Template;
            int[][] packages = new[] { (1d, .15d), (.15d, 1d), (1d, 1d), (.7d, 1.3d) }
                .SelectMany(w => BestSkills(slot, card, w.Item1, w.Item2))
                .GroupBy(x => string.Join(',', x.Traits)).Select(g => g.MaxBy(x => x.Gain))
                .OrderByDescending(x => x.Gain).Take(16).Select(x => x.Traits)
                .Append(seed[slot].Traits).DistinctBy(x => string.Join(',', x)).ToArray();
            PriorityQueue<DeckChoice[], double> next = new(); HashSet<string> seen = [];
            foreach (DeckChoice[] state in states)
                foreach (int[] skills in packages)
                {
                    DeckChoice[] candidate = (DeckChoice[])state.Clone(); candidate[slot] = candidate[slot] with { Traits = skills };
                    string key = string.Join(';', candidate.Select(c => string.Join(',', c.Traits)));
                    if (!seen.Add(key)) continue;
                    if ((evaluated & 63) == 0) token.ThrowIfCancellationRequested();
                    evaluated++; BattleResult value = new Battle(catalog, candidate.Select(c => c.ToBattleCard()).ToArray(), encounter).Run(false);
                    double priority = value.Passed ? 1e12 + value.RawScore : value.Margin;
                    next.Enqueue(candidate, priority); if (next.Count > 24) next.Dequeue();
                }
            states = next.UnorderedItems.OrderByDescending(x => x.Priority).Select(x => x.Element).ToList();
        }
        return states.Take(4).ToArray();
    }

    /// <summary>从一个beam保留起点做整卡、换位与技能顺序局部精化。</summary>
    private void Improve(DeckChoice[] seed)
    {
        localBest = null; team = (DeckChoice[])seed.Clone(); visited.Clear(); Consider(team);
        for (int round = 0; round < 5; round++)
        {
            BattleResult before = localBest!;
            for (int slot = 0; slot < 5; slot++)
                foreach (DeckChoice choice in Pool(slot))
                {
                    if (team.Where((_, i) => i != slot).Any(c => c.Template.BaseId == choice.Template.BaseId)) continue;
                    DeckChoice[] candidate = (DeckChoice[])team.Clone(); candidate[slot] = choice; Consider(candidate);
                }
            for (int left = 0; left < 5; left++) for (int right = left + 1; right < 5; right++)
            { DeckChoice[] candidate = (DeckChoice[])team.Clone(); (candidate[left], candidate[right]) = (candidate[right], candidate[left]); Consider(candidate); }
            for (int slot = 0; slot < 5; slot++)
            {
                DeckChoice current = team[slot];
                foreach (int[] order in Permutations(current.Traits))
                { DeckChoice[] candidate = (DeckChoice[])team.Clone(); candidate[slot] = current with { Traits = order }; Consider(candidate); }
            }
            if (!Better(localBest!, before)) break;
        }
    }

    /// <summary>完整枚举卡名位置骨架，再以技能优先候选beam和精确战斗选择配队。</summary>
    /// <param name="initialTeams">需要保留或验证的附加五卡起点。</param>
    /// <returns>游戏分数最高的已找到通关队伍。</returns>
    public DeckResult? Run(IEnumerable<DeckChoice[]>? initialTeams = null)
    {
        Stopwatch timer = Stopwatch.StartNew(); PriorityQueue<DeckChoice[], double> beam = new(); HashSet<string> seen = [];
        foreach (DeckChoice[] seed in Enumerable.Range(0, 4).Select(Seed).Concat(initialTeams ?? [])) AddBeam(seed, beam, seen);
        Dictionary<(int Slot, int BaseId, int Goal), DeckChoice[]> options = SeedOptions();
        int[] names = templates.Select(c => c.BaseId).Distinct().Order().ToArray();
        foreach (int[] skeleton in Skeletons(names))
            for (int goal = 0; goal < 4; goal++)
                for (int diverse = 0; diverse < 2; diverse++)
                    AddBeam(SkeletonSeed(skeleton, goal, diverse != 0, options), beam, seen);
        foreach (DeckChoice[] seed in beam.UnorderedItems.OrderByDescending(x => x.Priority).Take(8).Select(x => x.Element))
        {
            foreach (DeckChoice[] skillSeed in SkillBeam(seed)) Improve(skillSeed);
        }
        return Result(timer);
    }

    /// <summary>把最终队伍复算并附加真实材料；没有通关候选时返回空。</summary>
    private DeckResult? Result(Stopwatch timer)
    {
        if (winner is null) return null;
        BattleCard[] player = winner.Select(c => c.ToBattleCard()).ToArray();
        Dictionary<int, BattleResult> ratingBattles = Enumerable.Range(1, 20)
            .ToDictionary(rating => rating, rating => new Battle(catalog, player, encounter, rating).Run(true, true));
        BattleResult battle = ratingBattles[Battle.SearchRating];
        if (!battle.Passed) throw new InvalidDataException("保存前复算未满足通关条件。");
        DeckCardResult[] cards = winner.Select(c => new DeckCardResult { Template = c.Template.Id, Color = c.Color, Traits = c.Traits,
            Materials = Craft.Assign(catalog, c.Template, c.Traits) ?? throw new InvalidDataException("技能不能由实际粒子同时提供。") }).ToArray();
        return new DeckResult { Library = library, Encounter = encounter.Id, Cards = cards, Battle = battle, RatingBattles = ratingBattles,
            Evaluated = evaluated,
            Seconds = timer.Elapsed.TotalSeconds, Method = "70模板/10卡名有序骨架枚举、技能优先后验合法、多状态beam与完整战斗复算" };
    }

    /// <summary>最多三个技能的全部装备顺序；不改变技能集合或材料合法性。</summary>
    /// <param name="values">当前不同技能。</param>
    /// <returns>每种顺序的独立数组。</returns>
    private static IEnumerable<int[]> Permutations(int[] values)
    {
        if (values.Length < 2) { yield return values; yield break; }
        for (int i = 0; i < values.Length; i++)
            foreach (int[] tail in Permutations(values.Where((_, index) => index != i).ToArray())) yield return [values[i], .. tail];
    }
}
