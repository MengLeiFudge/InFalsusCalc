# InFalsusCalc

《In Falsus》的离线配方计算、回想配队和遭遇分复算工具。项目使用 C# / .NET 10 和 Google OR-Tools，生产计算不启动游戏、不读取玩家存档，也不依赖 Python 或网络服务。

## 直接使用

在 GitHub Releases 下载 `InFalsusCalc-win-x64.zip`，解压后直接运行：

```bat
InFalsusCalc.exe
```

程序会依次完成制卡、允许罚分 0/1/2 的配队、谱面等级复算和独立 HTML。完成后打开：

```text
output\配方与回想成果.html
```

仓库中的 `docs/index.html` 是已经计算完成的独立成果页。它内嵌全部数据、样式和脚本，可直接离线打开。

## 主要功能

- 读取固定的游戏资源快照，计算 42 张卡的合法粒子布局。
- 固定槽位、范围和净惩罚层内，攻击按“攻击最高、并列时防御最高”保留，防御按“防御最高、并列时攻击最高”保留；攻防总和保留最大总和下所有不同的攻击/防御面板拆分。
- 正式计算并保留净惩罚 0、1、2 的候选；三次惩罚会令面板归零，不进入卡库。
- 结构置信度采用 80% 优先、70% 截断、每张卡至少三种结构。
- 配队阶段先做跨惩罚安全支配，再保留高价值结构卡和面板卡；当前成果为 106 个模板、10 种卡名。
- 技能先按对手、位置和范围组合，再检查粒子材料能否合法携带。
- 配队分别按单卡允许罚分上限 0、1、2 和谱面等级 10 的 RawScore 搜索；游戏 Score 显示上限为 999999999。
- 每套最终配队额外预计算谱面等级 1 至 20，成果页可切换允许罚分与谱面等级，查看分数、HP、阶段和特性事件。
- 输出每张卡的粒子位置、技能材料来源、敌我卡组和逐阶段战斗明细；回想按章节标注，并展示完整游戏掉落表中的粒子权重、效能、技能抽取及技能池。
- 推荐配队使用游戏原始立绘、卡框、攻防图标和技能图标组合卡面；技能悬停可查看完整效果说明。

## 构建

需要 Windows、.NET 10 SDK 和 Visual Studio MSBuild：

```bat
"C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe" InFalsusCalc.sln /restore /t:Build /p:Configuration=Release /m
```

普通构建输出：

```text
bin\Release\net10.0\win-x64\InFalsusCalc.exe
```

生成自包含 Windows x64 包：

```bat
"C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe" InFalsusCalc.csproj /restore /t:Publish /p:Configuration=Release /p:RuntimeIdentifier=win-x64 /p:SelfContained=true /p:PublishDir=.release\InFalsusCalc-win-x64\
```

## 命令

普通使用不需要参数：

```bat
:: 完整执行：制卡 → 三档允许罚分配队 → 独立 HTML
InFalsusCalc.exe

:: 不限总时长公平续算制卡CONFIDENCE：每个目标按相同墙钟量子循环轮转，直到全部闭合或stop
InFalsusCalc.exe --prove-confidence

:: 可选的 CPU 和普通阶段预算
InFalsusCalc.exe --threads 20 --seconds 28800

:: 查看状态或请求停止
InFalsusCalc.exe status
InFalsusCalc.exe stop
```

开发定位参数只在 `debug run` 下开放；它仍会完整执行后续阶段：

```bat
:: 只限制制卡阶段为指定配方和 key
InFalsusCalc.exe debug run --recipe 66 --key 3,0,4 --threads 20

:: 配队阶段强制重算指定回想；其他回想复用或补齐缓存
InFalsusCalc.exe debug run --encounter 105 --threads 20

:: 单独生成结构置信度分析报告
InFalsusCalc.exe debug confidence
```

其他调试参数包括 `--goal`、`--output`、`--slice-seconds` 和 `--recipe-seconds`。制卡定位参数只影响制卡阶段，`--encounter`只影响配队阶段；只有31个回想在允许罚分0、1、2下的93套配队全部齐全后，程序才会替换最终HTML。

## 计算范围

制卡阶段分别计算净惩罚 0、1、2 的合法布局。固定 key 以槽数、左范围和右范围分组：攻击目标按攻击、防御字典序取唯一最优面板，防御目标按防御、攻击字典序取唯一最优面板；总和目标先证明最大攻击与防御之和，再保留该总和下所有不同的攻击/防御面板拆分，同一面板拆分只保留一种实际拼法。同一棋盘格最多叠放3个粒子；数量、越界、重叠和断连共享同一个净惩罚预算，保存前仍由完整规则重新核验。低收益奖励区域按当前策略排除；零槽范围折叠，但区域绑定的攻防仍计入面板。

跨惩罚层删除候选时，同一卡只有在槽位数、左范围和右范围完全相同，且另一候选的最终攻击、防御均不低、至少一项更高时才被支配；颜色集合、材料承载能力和罚分层不阻止这种面板支配。成果页“卡牌一览”先按等级、最终颜色和“包含可变更色彩的卡牌”限定范围，再搜索多选基础卡，最后叠加槽位、左范围、右范围和总罚分条件。结果使用与配队相同的游戏卡面，默认按基础卡发布顺序升序排列，也可按三级字段与升降序依次排序；列表默认每页20张并可切换为50张，只渲染当前页。点击卡面后才展示完整罚分、粒子、特性和拼法。旧目标代表仅保留为历史报告兼容数据。

配队阶段先保留以下模板：

- 3 槽且左范围与右范围之和至少为 4 的结构卡。
- 攻击、防御或总和达到全库对应最高值 80% 的面板卡。

卡名和位置骨架完整枚举，模板、颜色和技能使用有界 beam、随机多起点、单卡替换和前 24 邻居双卡联合搜索。结果属于当前模板和搜索策略内的 `best_found`，不声明所有底层模板、颜色和技能组合的数学全局最优。

战斗条件固定为联觉开启、25 个代表性全 EXACT 键、初始 HP 100。主程序分别搜索单卡允许罚分上限 0、1、2 的配队，较高上限会保留较低上限的更高分结果。配队选择固定使用谱面等级 10；谱面等级 1 至 20 只复算同一推荐配队，不重新选择卡牌。

## 数据与目录

- `Data/catalog.json`：当前兼容的求解资源快照；制卡和配队的材料来源只使用其中 `shapes.sources` 的最高效能材料回想。
- `Data/encounter-loot.json`：从同版本游戏资源提取的31个回想完整掉落表，只用于成果页展示，不参与求解。
- `Data/Seeds`：制卡求解的布局种子。
- `Evidence`：原生规则静态分析证据，不作为旧候选输入。
- `src`：制卡、配队、战斗和报告生成代码。
- `Web`：独立成果页模板，以及从当前游戏资源提取的技能图标、卡牌立绘与卡面图层。
- `docs/index.html`：当前已发布的静态成果页。

运行时产生的 `state`、`logs`、`output`、求解检查点和本地缓存不会提交到仓库。
