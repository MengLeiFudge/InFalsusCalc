using System.Text.Json;
using System.Text.Json.Serialization;

namespace InFalsusCalc;

/// <summary>游戏六边格轴坐标，第三轴为-Q-R。</summary>
/// <param name="Q">横向轴坐标。</param>
/// <param name="R">斜向轴坐标。</param>
[JsonConverter(typeof(HexConverter))]
internal readonly record struct Hex(int Q, int R)
{
    /// <summary>共享边的六个相邻方向。</summary>
    public static readonly Hex[] Directions = [new(1, 0), new(1, -1), new(0, -1), new(-1, 0), new(-1, 1), new(0, 1)];
    /// <summary>将粒子局部坐标平移到配方坐标。</summary>
    /// <param name="other">平移向量。</param>
    /// <returns>平移后的坐标。</returns>
    public Hex Add(Hex other) => new(Q + other.Q, R + other.R);
}

/// <summary>兼容资源的Q/R对象和布局中的二元数组，输出沿用网页数组格式。</summary>
internal sealed class HexConverter : JsonConverter<Hex>
{
    /// <inheritdoc/>
    public override Hex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        JsonElement value = doc.RootElement;
        return value.ValueKind == JsonValueKind.Array ? new(value[0].GetInt32(), value[1].GetInt32())
            : new(value.GetProperty("Q").GetInt32(), value.GetProperty("R").GetInt32());
    }
    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Hex value, JsonSerializerOptions options)
    {
        writer.WriteStartArray(); writer.WriteNumberValue(value.Q); writer.WriteNumberValue(value.R); writer.WriteEndArray();
    }
}

/// <summary>Unity资源中的整数标识包装。</summary>
internal sealed class ResourceId
{
    /// <summary>资源表中的稳定编号。</summary>
    [JsonPropertyName("Value")] public int Value { get; init; }
}

/// <summary>奖励或安全区的一个格子。</summary>
internal sealed class BoardCell
{
    /// <summary>格子轴坐标。</summary>
    [JsonPropertyName("Hex")] public Hex Position { get; init; }
    /// <summary>奖励要求的颜色编号。</summary>
    [JsonPropertyName("Color")] public int Color { get; init; }
}

/// <summary>奖励效果的整数参数。</summary>
internal sealed class EffectArgument
{
    /// <summary>原生整数参数，不引入百分比换算。</summary>
    [JsonPropertyName("IntValue")] public int Value { get; init; }
}

/// <summary>一个区域被完全同色覆盖后得到的效果。</summary>
internal sealed class RegionEffect
{
    /// <summary>原生效果类型编号。</summary>
    [JsonPropertyName("EffectType")] public int Kind { get; init; }
    /// <summary>按原生顺序保存的参数。</summary>
    [JsonPropertyName("Parameters")] public EffectArgument[] Arguments { get; init; } = [];
}

/// <summary>必须全部同色覆盖才激活的奖励区域。</summary>
internal sealed class BonusArea
{
    /// <summary>要求覆盖的颜色格。</summary>
    [JsonPropertyName("Cells")] public BoardCell[] Cells { get; init; } = [];
    /// <summary>激活后同时生效的奖励。</summary>
    [JsonPropertyName("BonusEffects")] public RegionEffect[] Effects { get; init; } = [];
}

/// <summary>配方的固定几何和角色条件。</summary>
internal sealed class Recipe
{
    /// <summary>资源中的配方编号。</summary>
    [JsonPropertyName("Id")] public ResourceId Identity { get; init; } = new();
    /// <summary>内部配方整数编号。</summary>
    [JsonIgnore] public int Id => Identity.Value;
    /// <summary>中文配方名称。</summary>
    public string Name { get; init; } = "";
    /// <summary>同名卡判重编号，多个配方可能共享。</summary>
    public int BaseId { get; init; }
    /// <summary>角色编号，决定可解锁的拼图容忍。</summary>
    [JsonPropertyName("Character")] public int Character { get; init; }
    /// <summary>游戏中的配方等级，范围为1至5。</summary>
    [JsonPropertyName("RecipeTier")] public int Tier { get; init; }
    /// <summary>游戏资源中的高阶卡面标记，0为普通卡面。</summary>
    [JsonPropertyName("IsSR")] public int SrFlag { get; init; }
    /// <summary>是否使用游戏中的高阶卡面样式。</summary>
    [JsonIgnore] public bool IsSr => SrFlag != 0;
    /// <summary>默认卡牌颜色。</summary>
    [JsonPropertyName("BaseColor")] public int BaseColor { get; init; }
    /// <summary>配方自身的粒子数量上限。</summary>
    [JsonPropertyName("MaxIota")] public int MaxIota { get; init; }
    /// <summary>合法外框中的所有格子。</summary>
    [JsonPropertyName("AllSegments")] public BoardCell[] Board { get; init; } = [];
    /// <summary>完全探索时所有可作为锚点的安全格。</summary>
    [JsonPropertyName("SafeSegments")] public BoardCell[] Safe { get; init; } = [];
    /// <summary>奖励区域，顺序与结果的Active编号一致。</summary>
    [JsonPropertyName("BonusAreas")] public BonusArea[] Areas { get; init; } = [];
}

/// <summary>一种真实可刷材料来源。</summary>
internal sealed class MaterialSource
{
    /// <summary>自由选曲回想编号。</summary>
    public int Encounter { get; init; }
    /// <summary>同一来源池中的可掉落特性。</summary>
    public int[] Traits { get; init; } = [];
    /// <summary>一颗粒子最多携带的特性数量。</summary>
    public int Capacity { get; init; }
}

/// <summary>固定朝向的粒子形状；只能平移。</summary>
internal sealed class Shape
{
    /// <summary>原始粒子编号包装。</summary>
    [JsonPropertyName("Id")] public ResourceId Identity { get; init; } = new();
    /// <summary>整数粒子编号。</summary>
    [JsonIgnore] public int Id => Identity.Value;
    /// <summary>粒子颜色。</summary>
    [JsonPropertyName("Color")] public int Color { get; init; }
    /// <summary>材料阶数，1至3。</summary>
    [JsonPropertyName("Tier")] public int Tier { get; init; }
    /// <summary>原生固定朝向的局部格子。</summary>
    [JsonPropertyName("Segments")] public Hex[] Cells { get; init; } = [];
    /// <summary>相同掉落特性能力的分组编号。</summary>
    public int Profile { get; init; }
    /// <summary>能刷到999效能该形状的来源。</summary>
    public MaterialSource[] Sources { get; init; } = [];
}

/// <summary>同一颗材料可以选择的一个技能池。</summary>
internal sealed class TraitPool
{
    /// <summary>可随机获得的技能编号。</summary>
    public int[] Traits { get; init; } = [];
    /// <summary>该池单颗粒子的最大携带数。</summary>
    public int Capacity { get; init; }
}

/// <summary>供材料合法性匹配使用的等价能力组。</summary>
internal sealed class MaterialProfile
{
    /// <summary>能力组编号。</summary>
    public int Id { get; init; }
    /// <summary>互斥的单粒子来源池。</summary>
    public TraitPool[] Options { get; init; } = [];
}

/// <summary>一项特性的原生效果及触发条件。</summary>
internal sealed class TraitEffect
{
    /// <summary>效果类型，含瞬时攻击8096和回复8097。</summary>
    [JsonPropertyName("TraitEffect")] public int Kind { get; init; }
    /// <summary>原生阶段/范围条件。</summary>
    [JsonPropertyName("TraitActivationCondition")] public int Condition { get; init; }
    /// <summary>原生参数，百分比效果按各自规则换算。</summary>
    [JsonPropertyName("Parameter0")] public double Value { get; init; }
}

/// <summary>特性名称及全部效果。</summary>
internal sealed class SkillTrait
{
    /// <summary>稳定技能编号。</summary>
    public int Id { get; init; }
    /// <summary>中文名称。</summary>
    public string Name { get; init; } = "";
    /// <summary>原生本地化描述。</summary>
    public string Description { get; init; } = "";
    /// <summary>技能素材阶级；特殊敌方技能为0。</summary>
    public int Tier { get; init; }
    /// <summary>该技能同时提供的效果。</summary>
    public TraitEffect[] Effects { get; init; } = [];
}

/// <summary>完整游戏掉落表中的一项带权技能。</summary>
internal sealed class EncounterLootTrait
{
    /// <summary>技能编号。</summary>
    public int Id { get; init; }
    /// <summary>该技能在原始技能表中的权重。</summary>
    public int Weight { get; init; }
}

/// <summary>完整游戏掉落表中的一项粒子结果。</summary>
internal sealed class EncounterLootDrop
{
    /// <summary>粒子形状编号。</summary>
    public int Shape { get; init; }
    /// <summary>该粒子在原始粒子表中的权重。</summary>
    public int Weight { get; init; }
    /// <summary>最低效能。</summary>
    public int MinPotency { get; init; }
    /// <summary>最高效能。</summary>
    public int MaxPotency { get; init; }
    /// <summary>生成粒子时抽取技能的次数。</summary>
    public int TraitRolls { get; init; }
}

/// <summary>一个回想的完整游戏掉落表，仅供成果页展示。</summary>
internal sealed class EncounterLoot
{
    /// <summary>回想编号。</summary>
    public int Id { get; init; }
    /// <summary>原始掉落表编号。</summary>
    public int Table { get; init; }
    /// <summary>粒子表的总权重。</summary>
    public int TotalWeight { get; init; }
    /// <summary>技能表的总权重，包含未发布的“无技能”行。</summary>
    public int TraitTotalWeight { get; init; }
    /// <summary>技能表中实际技能的带权行。</summary>
    public EncounterLootTrait[] Traits { get; init; } = [];
    /// <summary>粒子表中的全部带权行。</summary>
    public EncounterLootDrop[] Drops { get; init; } = [];
}

/// <summary>从当前游戏版本提取的完整回想掉落展示快照。</summary>
internal sealed class EncounterLootSnapshot
{
    /// <summary>快照格式版本。</summary>
    public int Schema { get; init; }
    /// <summary>对应的游戏资源提交编号。</summary>
    public string Commit { get; init; } = "";
    /// <summary>31个自由选曲回想的完整掉落表。</summary>
    public EncounterLoot[] Encounters { get; init; } = [];
}

/// <summary>敌我战斗使用的固定卡牌数值。</summary>
internal sealed class BattleCard
{
    /// <summary>用于展示的卡牌名称。</summary>
    public string Name { get; init; } = "";
    /// <summary>选定的颜色编号。</summary>
    public int Color { get; init; }
    /// <summary>敌方或玩家的实战攻击，资源快照可能以JSON浮点表示。</summary>
    public double Power { get; init; }
    /// <summary>敌方或玩家的实战防御，资源快照可能以JSON浮点表示。</summary>
    public double Fortitude { get; init; }
    /// <summary>向左覆盖的槽位数。</summary>
    public int Left { get; init; }
    /// <summary>向右覆盖的槽位数。</summary>
    public int Right { get; init; }
    /// <summary>按装备顺序保存的特性编号。</summary>
    public int[] Traits { get; init; } = [];
}

/// <summary>当前版本一个自由选曲回想。</summary>
internal sealed class Encounter
{
    /// <summary>回想编号。</summary>
    public int Id { get; init; }
    /// <summary>中文名称。</summary>
    public string Name { get; init; } = "";
    /// <summary>connect或reflection。</summary>
    public string Mode { get; init; } = "";
    /// <summary>自由选曲列表中的零基顺序，用于划分章节。</summary>
    public int Order { get; init; }
    /// <summary>敌方按槽位排列的五张卡。</summary>
    public BattleCard[] Cards { get; init; } = [];
    /// <summary>敌方随机NEAR概率，当前支持版本为零。</summary>
    public int NearChance { get; init; }
    /// <summary>敌方随机BREAK概率，当前支持版本为零。</summary>
    public int MissChance { get; init; }
}

/// <summary>已从游戏资源提取并有安装文件哈希约束的版本快照。</summary>
internal sealed class GameSnapshot
{
    /// <summary>快照稳定编号。</summary>
    public string Id { get; init; } = "";
    /// <summary>资源提交编号。</summary>
    public string Commit { get; init; } = "";
    /// <summary>原生规则版本。</summary>
    public string Rules { get; init; } = "";
    /// <summary>原生程序的SHA-256。</summary>
    public string BinaryHash { get; init; } = "";
    /// <summary>数据是否已与规则版本核对。</summary>
    public bool Compatible { get; init; }
    /// <summary>有效配方。</summary>
    public Recipe[] Recipes { get; init; } = [];
    /// <summary>固定朝向的40种粒子。</summary>
    public Shape[] Shapes { get; init; } = [];
    /// <summary>玩家与敌方可能使用的特性。</summary>
    public SkillTrait[] Traits { get; init; } = [];
    /// <summary>材料能力分组。</summary>
    public MaterialProfile[] Profiles { get; init; } = [];
    /// <summary>31个自由选曲回想。</summary>
    public Encounter[] Encounters { get; init; } = [];
}

/// <summary>一块粒子的真实放置；坐标与格子用于复现和展示。</summary>
internal sealed class Placement
{
    /// <summary>形状编号。</summary>
    public int Id { get; init; }
    /// <summary>平移的Q坐标。</summary>
    public int Q { get; init; }
    /// <summary>平移的R坐标。</summary>
    public int R { get; init; }
    /// <summary>原生形状颜色。</summary>
    public int Color { get; init; }
    /// <summary>占用的真实格子。</summary>
    public Hex[] Cells { get; init; } = [];
}

/// <summary>从实际布局复算的卡牌模板，字段与独立网页数据兼容。</summary>
internal sealed class CardTemplate
{
    /// <summary>按快照和布局生成的稳定编号。</summary>
    public string Id { get; set; } = "";
    /// <summary>配方编号。</summary>
    public int Recipe { get; init; }
    /// <summary>同名卡判重编号。</summary>
    public int BaseId { get; init; }
    /// <summary>配方中文名。</summary>
    public string Name { get; init; } = "";
    /// <summary>最终卡牌攻击。</summary>
    public int Power { get; init; }
    /// <summary>最终卡牌防御。</summary>
    public int Fortitude { get; init; }
    /// <summary>最终攻防之和。</summary>
    public int Total => Power + Fortitude;
    /// <summary>仅区域求和的原始攻击。</summary>
    public int BasePower { get; init; }
    /// <summary>仅区域求和的原始防御。</summary>
    public int BaseFortitude { get; init; }
    /// <summary>该同一布局真实解锁的颜色。</summary>
    public int[] Colors { get; init; } = [];
    /// <summary>普通特性槽数，最多3。</summary>
    public int Slots { get; init; }
    /// <summary>左范围，最多4。</summary>
    public int Left { get; init; }
    /// <summary>右范围，最多4。</summary>
    public int Right { get; init; }
    /// <summary>Ⅰ/Ⅱ/Ⅲ阶粒子的真实数量。</summary>
    public int[] TierCounts { get; init; } = [0, 0, 0];
    /// <summary>材料可以提供的单项特性；联合装配另行检查。</summary>
    public int[] AvailableTraits { get; init; } = [];
    /// <summary>卡牌自身阶段与范围内槽位收益的粗略结构分。</summary>
    public double InnerStructure { get; init; }
    /// <summary>范围外其他卡阶段的槽位收益粗略结构分。</summary>
    public double OuterStructure { get; init; }
    /// <summary>惩罚后的保留百分比。</summary>
    public double RetainedPercent { get; init; }
    /// <summary>内部材料能力数量，仅在最多3个槽的匹配中截断。</summary>
    public int[] Carriers { get; init; } = [];
    /// <summary>按照形状、Q、R排序的实际放置。</summary>
    public Placement[] Placements { get; init; } = [];
    /// <summary>已激活奖励区域的零基编号。</summary>
    public int[] Active { get; init; } = [];
    /// <summary>扣除容忍后的惩罚次数。</summary>
    public int Strikes { get; init; }
    /// <summary>数量、越界、重叠、断连各项净惩罚。</summary>
    public int[] Penalties { get; init; } = [];
    /// <summary>对应四项未经容忍扣除的数量。</summary>
    public int[] RawPenalties { get; init; } = [];
    /// <summary>是否有独立簇完全不接触安全区。</summary>
    public bool Falsehood { get; init; }
    /// <summary>粒子总数量。</summary>
    public int Count => Placements.Length;
    /// <summary>是否有正攻防且不存在假象。</summary>
    public bool Valid { get; init; }
    /// <summary>槽数、规范化左右范围和颜色位集合。</summary>
    public int[] Group { get; init; } = [];
}

/// <summary>一个已装备技能的粒子与可刷来源。</summary>
internal sealed class AssignedMaterial
{
    /// <summary>布局清单中的一基粒子编号。</summary>
    public int Piece { get; init; }
    /// <summary>原始形状编号。</summary>
    public int Shape { get; init; }
    /// <summary>该粒子需要同时提供的技能。</summary>
    public int[] Traits { get; init; } = [];
    /// <summary>支持此技能组合的回想编号。</summary>
    public int[] Sources { get; init; } = [];
}
