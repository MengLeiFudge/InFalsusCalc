namespace InFalsusCalc;

/// <summary>一个固定惩罚预算的证明状态；所有界都是主要面板值。</summary>
internal sealed class PenaltyCase
{
    /// <summary>攻击、防御、总值的安全上界。</summary>
    public int[] Bounds { get; set; } = [];
    /// <summary>已证不可行时才置位。</summary>
    public bool Infeasible { get; set; }
    /// <summary>各目标的奖励覆盖松弛是否已处理，可继续完整桥接模型。</summary>
    public bool[] CoverDone { get; set; } = [false, false, false];
    /// <summary>已发现且必须拥有安全根或向外连接的簇。</summary>
    public List<Hex[]> Cuts { get; set; } = [];
    /// <summary>最近一次CP-SAT结论，UNKNOWN仅表示预算内未判定。</summary>
    public string? LastStatus { get; set; }
    /// <summary>最近模型的放置变量数，用于识别几何域膨胀。</summary>
    public int LastCandidates { get; set; }
    /// <summary>最近求解的墙钟秒数。</summary>
    public double LastSeconds { get; set; }
    /// <summary>最近模型是否为忽略连通的奖励覆盖松弛。</summary>
    public bool LastRelaxed { get; set; }
}

/// <summary>固定key下一个面板目标的界、证明状态与从零求解耗时。</summary>
internal sealed class KeyGoalState
{
    /// <summary>OPTIMAL、INFEASIBLE、CONFIDENCE或UNKNOWN；CONFIDENCE保留合法代表和未排除上界。</summary>
    public string Status { get; set; } = "UNKNOWN";
    /// <summary>实际开始搜索时的范围签名；旧数据为空时按原基础策略处理。</summary>
    public string? SearchPolicy { get; set; }
    /// <summary>尚未排除的最终面板值上界；最优完成时等于结果值。</summary>
    public int UpperBound { get; set; }
    /// <summary>本目标求解耗时，秒；不包含共享几何预编译。</summary>
    public double Seconds { get; set; }
    /// <summary>未命中内存缓存的几何查询数量。</summary>
    public long GeometryQueries { get; set; }
}

/// <summary>同一槽数、范围与颜色集合的最多三个面板代表。</summary>
internal sealed class GroupState
{
    /// <summary>新制卡器为槽数、左范围、右范围；旧展示分组另含颜色掩码。</summary>
    public int[] Key { get; set; } = [];
    /// <summary>每个主要目标引用的布局编号。</summary>
    public Dictionary<string, string> Best { get; set; } = [];
    /// <summary>固定key求解器的面板证明状态；旧内外求解器不使用此字段。</summary>
    public Dictionary<string, KeyGoalState> Goals { get; set; } = [];
    /// <summary>0、1、2净惩罚的分支。</summary>
    public Dictionary<int, PenaltyCase> Cases { get; set; } = [];
    /// <summary>三个主要面板目标是否已经闭合。</summary>
    public bool Complete { get; set; }
    /// <summary>所有惩罚分支是否都已证不可行。</summary>
    public bool Infeasible { get; set; }
}

/// <summary>一张配方的可恢复成果，全覆盖策略只发布一个代表。</summary>
internal sealed class RecipeResult
{
    /// <summary>当前选择和计算策略。</summary>
    public string Policy { get; set; } = Storage.Policy;
    /// <summary>已由安全松弛或完整模型证明不可行的精确区域位集，供三目标与续算共享。</summary>
    public uint[] InfeasibleRegions { get; set; } = [];
    /// <summary>用户明确排除的奖励区域编号；最优性结论限定在此范围内。</summary>
    public int[] ExcludedAreas { get; set; } = [];
    /// <summary>固定资源快照。</summary>
    public string Snapshot { get; set; } = "";
    /// <summary>配方编号。</summary>
    public int Recipe { get; set; }
    /// <summary>各确定类别的搜索记录；全覆盖后保留记录但不继续搜索或发布。</summary>
    public Dictionary<string, GroupState> Groups { get; set; } = [];
    /// <summary>内向、外向已经由逐分不可行证明确定的实际最高结构分。</summary>
    public Dictionary<string, double> StructureScores { get; set; } = [];
    /// <summary>全覆盖存在时，保存的唯一布局；否则为各类别的面板代表。</summary>
    public Dictionary<string, CardTemplate> Cards { get; set; } = [];
    /// <summary>已找到的唯一全覆盖代表。</summary>
    public string? FullCover { get; set; }
    /// <summary>只针对全部奖励都激活的独立可行性分支，不污染普通类别的不可行性结论。</summary>
    public Dictionary<int, PenaltyCase> FullCases { get; set; } = [];
    /// <summary>全部三个惩罚预算已证不能全覆盖；未知不能标记为不可行。</summary>
    public bool FullInfeasible { get; set; }
    /// <summary>按用户要求采用已有合法候选，不再进入全空间证明。</summary>
    public bool BoundedFinalized { get; set; }
    /// <summary>普通结果的选择范围，网页据此避免宣称全空间极值。</summary>
    public string SelectionScope { get; set; } = "from_scratch_bounded_candidates";
    /// <summary>已进入的计算时间片数，用于变化搜索种子与预算。</summary>
    public int Round { get; set; }
    /// <summary>普通目标的公平调度位置。</summary>
    public int Cursor { get; set; }
    /// <summary>找到全覆盖代表，或全部普通类别计算结束。</summary>
    public bool Complete { get; set; }
    /// <summary>最近保存的UTC时间。</summary>
    public DateTimeOffset Updated { get; set; }
    /// <summary>累计真实求解用时，单位秒。</summary>
    public double SolveSeconds { get; set; }

    /// <summary>供网页与配队共同使用的选择范围，避免两端采用不同模板。</summary>
    /// <returns>全覆盖时仅一个布局，否则为各类别的三代表并集。</returns>
    public IEnumerable<CardTemplate> SelectedCards()
    {
        if (FullCover is not null) return [Cards[FullCover]];
        return Groups.Values.SelectMany(g => g.Best.Values).Distinct().Select(id => Cards[id]);
    }
}
