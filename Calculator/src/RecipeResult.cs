namespace InFalsusCalc;

/// <summary>固定key下一个面板目标的界、证明状态与从零求解耗时。</summary>
internal sealed class KeyGoalState
{
    /// <summary>OPTIMAL、INFEASIBLE、CONFIDENCE或UNKNOWN；CONFIDENCE保留合法代表和未排除上界。</summary>
    public string Status { get; set; } = "UNKNOWN";
    /// <summary>实际开始搜索时的范围签名；旧数据为空时按原基础策略处理。</summary>
    public string? SearchPolicy
    {
        get; set;
    }
    /// <summary>尚未排除的最终面板值上界；最优完成时等于结果值。</summary>
    public int UpperBound
    {
        get; set;
    }
    /// <summary>尚未排除的次目标上界；旧检查点为空时仅证明了主面板。</summary>
    public int? SecondaryUpperBound
    {
        get; set;
    }
    /// <summary>主次字典序或最大总和全部面板拆分是否已经完整闭合。</summary>
    public bool ObjectiveComplete
    {
        get; set;
    }
    /// <summary>本目标求解耗时，秒；不包含共享几何预编译。</summary>
    public double Seconds
    {
        get; set;
    }
    /// <summary>未命中内存缓存的几何查询数量。</summary>
    public long GeometryQueries
    {
        get; set;
    }
}

/// <summary>同一槽数、范围与颜色集合的最多三个面板代表。</summary>
internal sealed class GroupState
{
    /// <summary>新制卡器为槽数、左范围、右范围；旧展示分组另含颜色掩码。</summary>
    public int[] Key { get; set; } = [];
    /// <summary>该组要求的精确净惩罚次数。</summary>
    public int Strikes
    {
        get; set;
    }
    /// <summary>每个主要目标引用的布局编号；total保留一个兼容主引用。</summary>
    public Dictionary<string, string> Best { get; set; } = [];
    /// <summary>最大攻防和下所有不同攻防面板对的代表布局。</summary>
    public string[] TotalBest { get; set; } = [];
    /// <summary>固定key求解器的面板证明状态。</summary>
    public Dictionary<string, KeyGoalState> Goals { get; set; } = [];
}

/// <summary>一张配方的可恢复固定key成果。</summary>
internal sealed class RecipeResult
{
    /// <summary>当前选择和计算策略。</summary>
    public string Policy { get; set; } = "";
    /// <summary>已由安全松弛或完整模型证明不可行的零惩罚区域位集，兼容旧检查点。</summary>
    public uint[] InfeasibleRegions { get; set; } = [];
    /// <summary>按精确净惩罚层保存的安全不可行区域位集。</summary>
    public Dictionary<int, uint[]> InfeasibleRegionLayers { get; set; } = [];
    /// <summary>用户明确排除的奖励区域编号；最优性结论限定在此范围内。</summary>
    public int[] ExcludedAreas { get; set; } = [];
    /// <summary>固定资源快照。</summary>
    public string Snapshot { get; set; } = "";
    /// <summary>配方编号。</summary>
    public int Recipe
    {
        get; set;
    }
    /// <summary>各固定key和惩罚层的搜索记录。</summary>
    public Dictionary<string, GroupState> Groups { get; set; } = [];
    /// <summary>各类别引用的实际布局。</summary>
    public Dictionary<string, CardTemplate> Cards { get; set; } = [];
    /// <summary>普通结果的选择范围，网页据此避免宣称全空间极值。</summary>
    public string SelectionScope { get; set; } = "from_scratch_bounded_candidates";
    /// <summary>找到全部正式固定key目标的结论。</summary>
    public bool Complete
    {
        get; set;
    }
    /// <summary>最近保存的UTC时间。</summary>
    public DateTimeOffset Updated
    {
        get; set;
    }
}
