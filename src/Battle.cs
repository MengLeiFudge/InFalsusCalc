namespace InFalsusCalc;

/// <summary>固定谱面条件下的逐判定战斗与原生遭遇得分。</summary>
internal sealed class Battle
{
    /// <summary>游戏榜单与显示采用的遭遇分上限。</summary>
    public const double MaxScore = 999_999_999;
    /// <summary>制卡趋势与配队候选使用五阶段、每阶段五键的代表性谱面。</summary>
    public const int Notes = 25;
    /// <summary>配队搜索固定使用的谱面等级。</summary>
    public const int SearchRating = 10;
    /// <summary>颜色优势环：红→绿→紫→黄→蓝→红。</summary>
    public static readonly int[] Advantage = [0, 3, 4, 5, 1, 2];
    private readonly Catalog catalog;
    private readonly Encounter encounter;
    private readonly BattleCard[][] decks;
    private readonly List<Effect>[] effects = [[], []];
    private readonly Dictionary<long, Modifier>[] modifiers = [[], []];
    private readonly double[] hp = [100, 100], damage = [0, 0], healing = [0, 0];
    private readonly bool[] broken = [false, false];
    private readonly double[,] baseStats = new double[2, 2];
    private readonly List<PhaseResult> phases = [];
    private readonly List<TraitEvent> events = [];
    private readonly double playerBonus;
    private readonly int rating;
    private int phase, notesDone;
    private double minimum = 100;
    private bool fast, keepTrace;

    /// <summary>构造一场独立战斗，避免候选之间残留状态。</summary>
    /// <param name="catalog">特性与规则资源。</param>
    /// <param name="player">按位置排序的五张玩家卡。</param>
    /// <param name="encounter">敌方固定配置。</param>
    /// <param name="rating">谱面等级1至20；配队搜索省略时固定为10。</param>
    public Battle(Catalog catalog, BattleCard[] player, Encounter encounter, int rating = SearchRating)
    {
        if (player.Length != 5) throw new ArgumentException("需要五张卡。", nameof(player));
        if (rating is < 1 or > 20) throw new ArgumentOutOfRangeException(nameof(rating), "等级应为1至20。");
        if (encounter.NearChance != 0 || encounter.MissChance != 0) throw new InvalidDataException("该回想存在未支持的随机敌方判定。");
        this.catalog = catalog; this.encounter = encounter; this.rating = rating; decks = [player, encounter.Cards];
        playerBonus = (1 + .05 * player.Select(c => c.Color).Where(c => c != 0).Distinct().Count()) * (1 + rating / 100d);
        for (int side = 0; side < 2; side++)
            for (int slot = 0; slot < 5; slot++)
            {
                BattleCard card = decks[side][slot];
                double multiplier = card.Color != 0 && Advantage[card.Color] == decks[1 - side][slot].Color ? 1.25 : 1;
                if (side == 0) multiplier *= playerBonus;
                baseStats[side, 0] += card.Power * multiplier; baseStats[side, 1] += card.Fortitude * multiplier;
                for (int traitSlot = 0; traitSlot < card.Traits.Length; traitSlot++)
                {
                    int tid = card.Traits[traitSlot]; if (tid <= 1) continue;
                    SkillTrait trait = catalog.Traits[tid];
                    for (int i = 0; i < trait.Effects.Length; i++)
                    {
                        TraitEffect e = trait.Effects[i];
                        if (e.Kind is not (1 or 2 or 3 or 4 or 5 or 128 or 129 or 8096 or 8097)) throw new InvalidDataException($"未支持的特性效果{e.Kind}。");
                        long key = ((long)side << 48) | ((long)slot << 32) | ((long)traitSlot << 16) | (uint)i;
                        effects[side].Add(new(slot, key, tid, e.Kind, e.Condition, e.Value));
                    }
                }
            }
    }

    /// <summary>一项装备特性的真实效果，标识区分两方、卡槽、技能槽与效果槽。</summary>
    /// <param name="Owner">拥有者卡槽。</param>
    /// <param name="Identity">持续效果稳定标识。</param>
    /// <param name="Trait">特性编号。</param>
    /// <param name="Kind">效果类型。</param>
    /// <param name="Condition">原生触发条件。</param>
    /// <param name="Value">原生数值。</param>
    private readonly record struct Effect(int Owner, long Identity, int Trait, int Kind, int Condition, double Value);
    /// <summary>一个当前生效的持续修正。</summary>
    /// <param name="Kind">原生效果类型。</param>
    /// <param name="Value">原始参数。</param>
    private readonly record struct Modifier(int Kind, double Value);
    /// <summary>原生事件队列的等键元素。</summary>
    /// <param name="Kind">结束1、开始2、触发3、添加4或删除5。</param>
    /// <param name="Side">作用侧。</param>
    /// <param name="Index">阶段或特性效果列表索引。</param>
    /// <param name="Identity">持续效果标识。</param>
    /// <param name="Modifier">持续修正参数。</param>
    private readonly record struct StageEvent(int Kind, int Side, int Index, long Identity = 0, Modifier Modifier = default);

    /// <summary>还原旧原生Array.Sort对等键队列的重排，不能使用稳定排序代替。</summary>
    /// <param name="queue">正在处理的事件队列。</param>
    /// <param name="start">已处理前缀之后的起点。</param>
    private static void NativeSort(List<StageEvent> queue, int start = 0)
    {
        Partition(start, queue.Count - 1);
        // 等键插入排序不移动元素；较长区间保留原生分区交换次序。
        void Partition(int low, int high)
        {
            while (high - low + 1 > 16)
            {
                int middle = low + (high - low) / 2;
                (queue[middle], queue[high - 1]) = (queue[high - 1], queue[middle]);
                int left = low, right = high - 1;
                while (true)
                {
                    left++; right--; if (left >= right) break;
                    (queue[left], queue[right]) = (queue[right], queue[left]);
                }
                (queue[left], queue[high - 1]) = (queue[high - 1], queue[left]);
                Partition(left + 1, high); high = left - 1;
            }
        }
    }

    /// <summary>刷新双方整队攻防；削弱是除数，色彩隔离按当前对位颜色判断。</summary>
    /// <returns>攻击、防御、未修正防御及暴击倍率。</returns>
    private (double[] Attack, double[] Defense, double[] PlainDefense, double[] Critical) Stats()
    {
        double[] attack = new double[2], defense = new double[2], plain = new double[2], critical = new double[2];
        bool[] isolation = new bool[2];
        for (int side = 0; side < 2; side++)
        {
            double pUp = 1, fUp = 1, pDown = 1, fDown = 1, crit = -1;
            foreach (Modifier modifier in modifiers[side].Values)
                switch (modifier.Kind)
                {
                    case 1: pUp += modifier.Value * .01; break;
                    case 2: fUp += modifier.Value * .01; break;
                    case 3 or 129: fDown += modifier.Value / 100; break;
                    case 128: pDown += modifier.Value / 100; break;
                    case 5: crit = Math.Max(0, crit) + modifier.Value / 100; break;
                    case 4: isolation[side] |= decks[1 - side][phase].Color != modifier.Value; break;
                }
            if (fast)
            {
                attack[side] = baseStats[side, 0] * (pUp / pDown);
                defense[side] = baseStats[side, 1] * (fUp / fDown); plain[side] = baseStats[side, 1];
            }
            else
            {
                double chromatic = side == 0 ? 1 + .05 * decks[0].Select(c => c.Color).Where(c => c != 0).Distinct().Count() : 1;
                double difficulty = side == 0 ? 1 + rating / 100d : 1;
                for (int slot = 0; slot < 5; slot++)
                {
                    BattleCard card = decks[side][slot];
                    double color = card.Color != 0 && Advantage[card.Color] == decks[1 - side][slot].Color ? 1.25 : 1;
                    attack[side] += card.Power * (pUp / pDown) * chromatic * color * difficulty;
                    defense[side] += card.Fortitude * (fUp / fDown) * chromatic * color * difficulty;
                    plain[side] += card.Fortitude * chromatic * color * difficulty;
                }
            }
            critical[side] = crit > 0 ? crit : 5;
        }
        if (isolation[0]) attack[1] = 0;
        if (isolation[1]) attack[0] = 0;
        return (attack, defense, plain, critical);
    }

    /// <summary>累计全部伤害，包括击破后的伤害；连接破损标记不可撤销。</summary>
    /// <param name="target">受伤侧。</param>
    /// <param name="amount">伤害百分点。</param>
    private void Damage(int target, double amount)
    {
        if (!double.IsFinite(amount) || amount < 0) throw new InvalidDataException("出现无效伤害。");
        hp[target] -= amount; damage[target] += amount;
        if (encounter.Mode == "connect" && hp[target] <= 0) broken[target] = true;
        minimum = Math.Min(minimum, hp[0]);
    }

    /// <summary>额外攻击不套共鸣倍率；得分和HP均只计实际有效回复。</summary>
    /// <param name="side">特性所属侧。</param>
    /// <param name="effect">被触发的效果。</param>
    private void Trigger(int side, Effect effect)
    {
        double amount = effect.Value * (double)(float).01;
        if (effect.Kind == 8096)
        {
            var values = Stats(); int other = 1 - side;
            if (values.Defense[other] <= 0 || values.PlainDefense[other] <= 0) throw new InvalidDataException("额外攻击遇到未核对的零防御条件。");
            amount = amount * values.Attack[side] / (values.Defense[other] / values.PlainDefense[other]) / values.PlainDefense[other];
            Damage(other, amount);
        }
        else if (effect.Kind == 8097)
        {
            if (encounter.Mode == "connect" && broken[side]) amount = 0;
            else { double next = Math.Min(100, hp[side] + amount); amount = next - hp[side]; hp[side] = next; healing[side] += amount; }
        }
        else throw new InvalidDataException("阶段触发使用了未知瞬时效果。");
        if (keepTrace) events.Add(new TraitEvent { At = notesDone, Phase = phase + 1, Side = side, Trait = effect.Trait,
            Kind = effect.Kind == 8096 ? "damage" : "heal", Amount = amount, Hp = (double[])hp.Clone() });
    }

    /// <summary>按原生开始/结束事件顺序处理持续修正及进出范围触发。</summary>
    /// <param name="current">零基阶段。</param>
    /// <param name="ending">是否阶段结束。</param>
    private void ProcessEvents(int current, bool ending)
    {
        phase = ending ? Math.Min(4, current + 1) : current;
        int effective = ending && current == 4 ? -1 : current;
        List<StageEvent> queue = [new(ending ? 1 : 2, 0, current), new(ending ? 1 : 2, 1, current)];
        for (int slot = 0; slot < 5; slot++)
            for (int side = 0; side < 2; side++)
                foreach (Effect effect in effects[side])
                {
                    if (effect.Owner != slot || effect.Condition is not (4 or 5 or 6 or 7)) continue;
                    BattleCard card = decks[side][slot];
                    bool inside = effective >= 0 && Math.Max(0, slot - card.Left) <= effective && effective <= Math.Min(4, slot + card.Right);
                    bool own = effective == slot;
                    bool enabled = effect.Condition switch { 4 => !own, 5 => !inside, 6 => own, _ => inside };
                    int target = effect.Kind >= 128 ? 1 - side : side;
                    if (enabled && !modifiers[target].ContainsKey(effect.Identity)) queue.Add(new(4, target, 0, effect.Identity, new(effect.Kind, effect.Value)));
                    else if (!enabled && modifiers[target].ContainsKey(effect.Identity)) queue.Add(new(5, target, 0, effect.Identity));
                }
        NativeSort(queue);
        for (int index = 0; index < queue.Count; index++)
        {
            StageEvent e = queue[index];
            if (e.Kind is 1 or 2)
            {
                int before = queue.Count;
                for (int i = 0; i < effects[e.Side].Count; i++)
                {
                    Effect effect = effects[e.Side][i]; BattleCard card = decks[e.Side][effect.Owner];
                    int start = Math.Max(0, effect.Owner - card.Left), end = Math.Min(4, effect.Owner + card.Right);
                    bool fires = ending
                        ? effect.Condition == 1 && effect.Owner == current || effect.Condition == 2 && start <= current && current <= end || effect.Condition == 3 && current == end
                        : effect.Condition == 10 && effect.Owner == current || effect.Condition == 9 && start <= current && current <= end || effect.Condition == 8 && current == start;
                    if (fires) queue.Add(new(3, e.Side, i));
                }
                if (queue.Count > before) NativeSort(queue, index + 1);
            }
            else if (e.Kind == 3) Trigger(e.Side, effects[e.Side][e.Index]);
            else if (e.Kind == 4) modifiers[e.Side][e.Identity] = e.Modifier;
            else if (e.Kind == 5) modifiers[e.Side].Remove(e.Identity);
        }
        phase = current;
    }

    /// <summary>计算五阶段代表键；逐判定模式只用于保留普通键/暴击键顺序。</summary>
    /// <param name="exact">是否按原生判定顺序逐项累计。</param>
    /// <param name="trace">是否保存网页所需阶段和技能明细。</param>
    /// <returns>胜负、HP、进度及真正的遭遇分。</returns>
    public BattleResult Run(bool exact = false, bool trace = false)
    {
        fast = !exact; keepTrace = trace; const int count = Notes / 5, threshold = count * 4 / 5;
        for (int current = 0; current < 5; current++)
        {
            phase = current; double[] before = (double[])hp.Clone(), damageBefore = (double[])damage.Clone(), healBefore = (double[])healing.Clone();
            ProcessEvents(current, false); var values = Stats();
            double outgoing = values.Attack[0] / (values.Defense[1] == 0 ? 100 : values.Defense[1]) * (100d / Notes);
            double incoming = values.Attack[1] / (values.Defense[0] == 0 ? 100 : values.Defense[0]) * (100d / Notes);
            if (exact)
                for (int note = 0; note < count; note++) { Damage(1, outgoing * (note < threshold ? 1 : values.Critical[0])); Damage(0, incoming); notesDone++; }
            else { Damage(1, outgoing * (threshold + (count - threshold) * values.Critical[0])); Damage(0, incoming * count); notesDone += count; }
            ProcessEvents(current, true);
            if (trace) phases.Add(new PhaseResult { Phase = current + 1, Notes = count, Charge = threshold,
                Attack = values.Attack, Defense = values.Defense, Critical = values.Critical, Before = before, After = (double[])hp.Clone(),
                Damage = [damage[0] - damageBefore[0], damage[1] - damageBefore[1]], Healing = [healing[0] - healBefore[0], healing[1] - healBefore[1]] });
        }
        double progress = 100 - hp[1] - (100 - hp[0]);
        bool passed = encounter.Mode == "reflection" ? progress >= 100 : broken[1] && !broken[0];
        double modifier = encounter.Mode == "reflection" ? .5 : 1;
        // BattleScore的运算顺序；固定完整谱面时判定比例为1，有效回复不计溢出。
        double reduction = (double)notesDone / Notes;
        double offensive = Math.Max(1e-13, damage[1]) / (100 + healing[1]) / reduction * (modifier * 10000);
        double defensive = 10000 / (Math.Max(1e-13, damage[0]) / (100 + healing[0])) * reduction;
        double rawScore = offensive * defensive / 10000 * reduction;
        return new BattleResult { Mode = encounter.Mode, Passed = passed, Score = Math.Min(MaxScore, rawScore), RawScore = rawScore,
            OffensiveScore = offensive, DefensiveScore = defensive,
            ReductionFactor = reduction, Margin = encounter.Mode == "reflection" ? progress - 100 : Math.Min(-hp[1], minimum), Progress = progress,
            Hp = (double[])hp.Clone(), MinimumHp = minimum, Broken = (bool[])broken.Clone(), EnemyProgress = 100 - hp[1],
            PlayerRemaining = Math.Max(0, hp[0]), EnemyRemaining = Math.Max(0, hp[1]), Damage = (double[])damage.Clone(),
            Healing = (double[])healing.Clone(), Phases = phases.ToArray(), Events = events.ToArray(), ExactAccumulation = exact };
    }
}

/// <summary>战斗结果，失败方案只能作为搜索状态，不能发布成通关配队。</summary>
internal sealed class BattleResult
{
    /// <summary>连接或映像模式。</summary>
    public string Mode { get; init; } = "";
    /// <summary>是否满足游戏通关条件。</summary>
    public bool Passed { get; init; }
    /// <summary>游戏上限内的遭遇总分，是通关候选的首要排序目标。</summary>
    public double Score { get; init; }
    /// <summary>未封顶的公式结果，仅用于诊断封顶配置，不参与优先级。</summary>
    public double RawScore { get; init; }
    /// <summary>原生进攻得分。</summary>
    public double OffensiveScore { get; init; }
    /// <summary>原生防守得分。</summary>
    public double DefensiveScore { get; init; }
    /// <summary>原生已判定与总判定的比值。</summary>
    public double ReductionFactor { get; init; }
    /// <summary>尚未通关时的搜索引导，不取代已通关队伍的得分。</summary>
    public double Margin { get; init; }
    /// <summary>双方净受伤之差。</summary>
    public double Progress { get; init; }
    /// <summary>双方结束HP，保留负值用于超额伤害。</summary>
    public double[] Hp { get; init; } = [];
    /// <summary>己方全程最低HP。</summary>
    public double MinimumHp { get; init; }
    /// <summary>永久破损标记。</summary>
    public bool[] Broken { get; init; } = [];
    /// <summary>对方净击破进度。</summary>
    public double EnemyProgress { get; init; }
    /// <summary>己方显示用剩余HP。</summary>
    public double PlayerRemaining { get; init; }
    /// <summary>对方显示用剩余HP。</summary>
    public double EnemyRemaining { get; init; }
    /// <summary>双方受到的总伤害。</summary>
    public double[] Damage { get; init; } = [];
    /// <summary>双方实际有效回复。</summary>
    public double[] Healing { get; init; } = [];
    /// <summary>五阶段结算。</summary>
    public PhaseResult[] Phases { get; init; } = [];
    /// <summary>瞬时特性触发记录。</summary>
    public TraitEvent[] Events { get; init; } = [];
    /// <summary>是否逐判定累计。</summary>
    public bool ExactAccumulation { get; init; }
}

/// <summary>单阶段双方状态。</summary>
internal sealed class PhaseResult
{
    /// <summary>从1开始的阶段号。</summary>
    public int Phase { get; init; }
    /// <summary>该阶段判定数。</summary>
    public int Notes { get; init; }
    /// <summary>进入共鸣前所需EXACT数量。</summary>
    public int Charge { get; init; }
    /// <summary>双方本阶段整队攻击。</summary>
    public double[] Attack { get; init; } = [];
    /// <summary>双方本阶段整队防御。</summary>
    public double[] Defense { get; init; } = [];
    /// <summary>共鸣倍率。</summary>
    public double[] Critical { get; init; } = [];
    /// <summary>阶段开始前HP。</summary>
    public double[] Before { get; init; } = [];
    /// <summary>阶段结束后HP。</summary>
    public double[] After { get; init; } = [];
    /// <summary>本阶段受到的伤害。</summary>
    public double[] Damage { get; init; } = [];
    /// <summary>本阶段有效回复。</summary>
    public double[] Healing { get; init; } = [];
}

/// <summary>一次瞬时特性事件。</summary>
internal sealed class TraitEvent
{
    /// <summary>已完成判定数。</summary>
    public int At { get; init; }
    /// <summary>一基阶段号。</summary>
    public int Phase { get; init; }
    /// <summary>己方0或敌方1。</summary>
    public int Side { get; init; }
    /// <summary>特性编号。</summary>
    public int Trait { get; init; }
    /// <summary>damage或heal。</summary>
    public string Kind { get; init; } = "";
    /// <summary>实际生效的百分点。</summary>
    public double Amount { get; init; }
    /// <summary>触发后双方HP。</summary>
    public double[] Hp { get; init; } = [];
}
