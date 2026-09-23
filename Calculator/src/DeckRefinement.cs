namespace InFalsusCalc;

/// <summary>完整枚举单卡配置邻域，以逐判定得分收敛；该邻域只提供全局搜索下界，不构成全局证明。</summary>
internal sealed class DeckRefinement
{
    /// <summary>五个固定队伍位置的全部排列，避免只检查两位置互换遗漏循环换位。</summary>
    private static readonly int[][] positionOrders = DeckSearch.Permutations([0, 1, 2, 3, 4]).ToArray();
    /// <summary>解释技能条件及粒子载体能力的只读规则。</summary>
    private readonly Catalog catalog;
    /// <summary>所有候选共享的对手、谱面判定数和通关模式。</summary>
    private readonly Encounter encounter;
    /// <summary>在组合扩展和逐候选计算之间响应中断。</summary>
    private readonly CancellationToken token;
    /// <summary>当前预算内的完整候选库；为空时保持起点卡面及颜色。</summary>
    private readonly CardTemplate[]? alternatives;
    /// <summary>敌方隔离效果要求的颜色，等价合并时必须逐一保留其真假结果。</summary>
    private readonly HashSet<int> isolationColors;
    /// <summary>按材料能力共享合法技能集合和顺序，避免每次战斗重新匹配载体。</summary>
    private readonly Dictionary<string, (int[][] Orders, HashSet<string> Sets)> packages = [];
    /// <summary>已构建且不再修改的技能集合之间的包含关系。</summary>
    private readonly Dictionary<(HashSet<string>, HashSet<string>), bool> coverage = [];

    /// <summary>当前已逐判定复算的最高分通关配置。</summary>
    public DeckChoice[] Team { get; private set; }
    /// <summary>当前配置的精确代表谱面战斗结果。</summary>
    public BattleResult Battle { get; private set; }
    /// <summary>实际逐判定比较的候选数。</summary>
    public long Evaluated { get; private set; }
    /// <summary>是否已经完成无改善的一整轮邻域检查，或达到游戏得分上限。</summary>
    public bool Completed { get; private set; }

    /// <summary>固定合法五卡起点，以原战斗计算器评价所有候选。</summary>
    /// <param name="catalog">原始技能和材料能力。</param>
    /// <param name="encounter">指定回想。</param>
    /// <param name="seed">已有通关队伍，包含颜色及技能顺序。</param>
    /// <param name="token">计算取消信号，取消后仍可读取当前最高分。</param>
    /// <param name="alternatives">可替换模板；为空时固定卡牌及颜色，只精化技能。</param>
    public DeckRefinement(Catalog catalog, Encounter encounter, DeckChoice[] seed, CancellationToken token, CardTemplate[]? alternatives = null)
    {
        this.catalog = catalog;
        this.encounter = encounter;
        this.token = token;
        this.alternatives = alternatives;
        isolationColors = encounter.Cards.SelectMany(c => c.Traits).Where(t => t > 1)
            .SelectMany(t => catalog.Traits[t].Effects).Where(e => e.Kind == 4).Select(e => (int)e.Value).ToHashSet();
        Team = (DeckChoice[])seed.Clone();
        Battle = new Battle(catalog, Team.Select(c => c.ToBattleCard()).ToArray(), encounter).Run(true);
        if (!Battle.Passed)
            throw new InvalidDataException("技能精化的起点必须已经通关。");
    }

    /// <summary>不使用估分截断；完整检查合法技能顺序，选卡时仅去掉在当前固定队友下被支配的配置。</summary>
    /// <param name="includePairs">同时完整检查两张卡各一项技能的联合替换，避免单步收益下降造成停滞。</param>
    public void Run(bool includePairs = false)
    {
        Completed = false;
        while (Battle.Score < InFalsusCalc.Battle.MaxScore)
        {
            double before = Battle.Score;
            for (int slot = 0; slot < Team.Length; slot++)
            {
                DeckChoice current = Team[slot];
                DeckChoice[] candidate = (DeckChoice[])Team.Clone();
                foreach (DeckChoice body in Bodies(slot))
                foreach (int[] skills in Packages(body.Template).Orders)
                {
                    token.ThrowIfCancellationRequested();
                    if (body.Template.Id == current.Template.Id && body.Color == current.Color && skills.SequenceEqual(current.Traits))
                        continue;
                    candidate[slot] = body with { Traits = skills };
                    Consider(candidate);
                    if (Battle.Score >= InFalsusCalc.Battle.MaxScore)
                    {
                        Completed = true;
                        return;
                    }
                }
            }
            if (alternatives is not null)
                Arrange();
            if (includePairs && Battle.Score < InFalsusCalc.Battle.MaxScore)
                ImprovePairs();
            if (Battle.Score <= before)
                break;
        }
        Completed = true;
    }

    /// <summary>固定五张卡及其技能，枚举全部位置和允许颜色，只合并战斗输入完全等价的颜色组合。</summary>
    internal void Arrange()
    {
        DeckChoice[] start = (DeckChoice[])Team.Clone();
        foreach (int[] order in positionOrders)
        {
            DeckChoice[] candidate = order.Select(i => start[i]).ToArray();
            HashSet<ulong> seen = [];
            Expand(0, 0, 0);

            void Expand(int slot, uint colors, ulong state)
            {
                token.ThrowIfCancellationRequested();
                if (Battle.Score >= InFalsusCalc.Battle.MaxScore)
                    return;
                if (slot == Team.Length)
                {
                    // 每位置克制及隔离真假、联觉颜色数完全一致时，原战斗的每个输入系数相同。
                    ulong key = (state << 3) | (uint)System.Numerics.BitOperations.PopCount(colors & 0b111110);
                    if (seen.Add(key))
                        Consider(candidate);
                    return;
                }
                int enemyColor = encounter.Cards[slot].Color;
                foreach (int color in candidate[slot].Template.Colors)
                {
                    int value = (color != 0 && InFalsusCalc.Battle.Advantage[color] == enemyColor ? 1 : 0)
                        | (enemyColor != 0 && InFalsusCalc.Battle.Advantage[enemyColor] == color ? 2 : 0)
                        | (isolationColors.Contains(color) ? 1 << (color + 2) : 0)
                        | (color == 0 ? 256 : 0);
                    candidate[slot] = candidate[slot] with { Color = color };
                    Expand(slot + 1, colors | (1u << color), (state << 9) | (uint)value);
                }
            }
        }
    }

    /// <summary>逐判定比较完整队伍，只有通关且得分严格提高才替换已保存的下界。</summary>
    /// <param name="candidate">临时候选；保存时复制数组，防止下一次枚举覆盖。</param>
    private void Consider(DeckChoice[] candidate)
    {
        token.ThrowIfCancellationRequested();
        BattleResult value = new Battle(catalog, candidate.Select(c => c.ToBattleCard()).ToArray(), encounter).Run(true);
        Evaluated++;
        if (!value.Passed || value.Score <= Battle.Score)
            return;
        Battle = value;
        Team = (DeckChoice[])candidate.Clone();
        Completed = false;
    }

    /// <summary>检查不同卡牌的两项技能同时变化，包括卸下或补入技能，不用单项收益筛选组合。</summary>
    private void ImprovePairs()
    {
        for (int left = 0; left < Team.Length; left++)
            for (int right = left + 1; right < Team.Length; right++)
            {
                DeckChoice[] a = Mutations(Team[left]), b = Mutations(Team[right]);
                DeckChoice[] candidate = (DeckChoice[])Team.Clone();
                foreach (DeckChoice first in a)
                    foreach (DeckChoice second in b)
                    {
                        candidate[left] = first;
                        candidate[right] = second;
                        Consider(candidate);
                        if (Battle.Score >= InFalsusCalc.Battle.MaxScore)
                            return;
                    }
            }
    }

    /// <summary>列出一个技能位置的全部合法替换，材料合法性复用完整集合缓存。</summary>
    /// <param name="card">当前装备。</param>
    /// <returns>去重后的一项技能变化。</returns>
    private DeckChoice[] Mutations(DeckChoice card)
    {
        HashSet<string> legal = Packages(card.Template).Sets;
        HashSet<string> seen = [];
        List<DeckChoice> result = [];
        int positions = Math.Min(card.Template.Slots, card.Traits.Length + 1);
        for (int index = 0; index < positions; index++)
            foreach (int trait in card.Template.AvailableTraits.Prepend(0))
            {
                int[] traits = index == card.Traits.Length ? [.. card.Traits, trait] : (int[])card.Traits.Clone();
                traits[index] = trait;
                traits = traits.Where(t => t > 1).ToArray();
                if (traits.SequenceEqual(card.Traits) || traits.Distinct().Count() != traits.Length
                    || !legal.Contains(string.Join(',', traits.Order())) || !seen.Add(string.Join(',', traits)))
                    continue;
                result.Add(card with { Traits = traits });
            }
        return result.ToArray();
    }

    /// <summary>其余四卡固定后，按颜色战斗等价类、有效范围与技能能力做面板支配剪枝，不使用近似收益。</summary>
    /// <param name="slot">要替换的位置。</param>
    /// <returns>不同名且在本轮合法的候选卡面。</returns>
    private DeckChoice[] Bodies(int slot)
    {
        if (alternatives is null)
            return [Team[slot]];
        HashSet<int> used = Team.Where((_, i) => i != slot).Select(c => c.Template.BaseId).ToHashSet();
        HashSet<int> otherColors = Team.Where((_, i) => i != slot).Select(c => c.Color).Where(c => c != 0).ToHashSet();
        int enemyColor = encounter.Cards[slot].Color;
        // 相同编码保证双方克制倍率、联觉颜色数及全部隔离判断一致，仅在当前固定队友下合并。
        int[] colorStates = Enumerable.Range(0, 6).Select(color =>
            (color != 0 && InFalsusCalc.Battle.Advantage[color] == enemyColor ? 1 : 0)
            | (enemyColor != 0 && InFalsusCalc.Battle.Advantage[enemyColor] == color ? 2 : 0)
            | (!otherColors.Contains(color) ? 4 : 0)
            | (isolationColors.Contains(color) ? 1 << (color + 3) : 0)
            | (color == 0 ? 512 : 0)).ToArray();
        List<DeckChoice> frontier = [];
        foreach (CardTemplate card in alternatives.Where(c => !used.Contains(c.BaseId)))
            foreach (int color in card.Colors)
            {
                token.ThrowIfCancellationRequested();
                DeckChoice candidate = new(card, color, []);
                if (frontier.Any(old => Dominates(old, candidate, slot, colorStates)))
                    continue;
                frontier.RemoveAll(old => Dominates(candidate, old, slot, colorStates));
                frontier.Add(candidate);
            }
        return frontier.ToArray();
    }

    /// <summary>固定颜色效应和生效区间时，复用同一技能顺序只会提高整队基础攻防；包含判断确保材料能力不丢失。</summary>
    /// <param name="better">拟保留的卡面。</param>
    /// <param name="other">拟剪去的卡面。</param>
    /// <param name="slot">固定队伍位置。</param>
    /// <param name="colorStates">当前固定队友下的颜色战斗等价类。</param>
    /// <returns>前者是否能替代后者的每个合法技能配置。</returns>
    private bool Dominates(DeckChoice better, DeckChoice other, int slot, int[] colorStates)
    {
        CardTemplate a = better.Template, b = other.Template;
        if (colorStates[better.Color] != colorStates[other.Color] || a.Power < b.Power || a.Fortitude < b.Fortitude
            || Math.Max(0, slot - a.Left) != Math.Max(0, slot - b.Left)
            || Math.Min(4, slot + a.Right) != Math.Min(4, slot + b.Right))
            return false;
        HashSet<string> aSets = Packages(a).Sets, bSets = Packages(b).Sets;
        if (!coverage.TryGetValue((aSets, bSets), out bool contains))
            coverage[(aSets, bSets)] = contains = aSets.IsSupersetOf(bSets);
        return contains;
    }

    /// <summary>按槽数、实际可用技能与载体能力缓存完整集合；每个合法集合枚举全部装备顺序。</summary>
    /// <param name="card">固定卡牌的真实材料能力。</param>
    /// <returns>包含空技能、少于槽数以及满槽配置的全部合法顺序。</returns>
    private (int[][] Orders, HashSet<string> Sets) Packages(CardTemplate card)
    {
        string key = $"{card.Slots}:{string.Join(',', card.AvailableTraits)}:{string.Join(',', card.Carriers)}";
        if (packages.TryGetValue(key, out var cached))
            return cached;
        List<int[]> result = [];
        HashSet<string> sets = [];
        List<int> selected = [];
        Expand(0);
        return packages[key] = (result.ToArray(), sets);

        void Expand(int start)
        {
            token.ThrowIfCancellationRequested();
            int[] skills = selected.ToArray();
            // 不能承载当前集合时，更大的集合也不可能合法，可直接剪去整个扩展分支。
            if (!Craft.CanAssign(catalog.Data.Profiles, card.Carriers, skills))
                return;
            sets.Add(string.Join(',', skills.Order()));
            result.AddRange(DeckSearch.Permutations(skills));
            if (selected.Count >= card.Slots)
                return;
            for (int i = start; i < card.AvailableTraits.Length; i++)
            {
                selected.Add(card.AvailableTraits[i]);
                Expand(i + 1);
                selected.RemoveAt(selected.Count - 1);
            }
        }
    }
}
