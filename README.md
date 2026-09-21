# InFalsusCalc

《In Falsus》的离线配方计算、回想配队和遭遇分复算工具。项目使用 C# / .NET 10 和 Google OR-Tools，生产计算不启动游戏、不读取玩家存档，也不依赖 Python 或网络服务。

## 直接使用

在 GitHub Releases 下载 `InFalsusCalc-win-x64.zip`，解压后运行：

```bat
InFalsusCalc.exe compute --threads 20 --seconds 28800
```

完整计算结束后打开：

```text
output\配方与回想成果.html
```

仓库中的 `docs/index.html` 是已经计算完成的独立成果页。它内嵌全部数据、样式和脚本，可直接离线打开。

## 主要功能

- 读取固定的游戏资源快照，计算 42 张卡的合法粒子布局。
- 按攻击、防御、攻防总和保留固定槽位、范围和净惩罚层下的代表配方。
- 正式计算并保留净惩罚 0、1、2 的候选；三次惩罚会令面板归零，不进入卡库。
- 结构置信度采用 80% 优先、70% 截断、每张卡至少三种结构。
- 配队阶段先做跨惩罚安全支配，再保留高价值结构卡和面板卡；当前成果为 106 个模板、10 种卡名。
- 技能先按对手、位置和范围组合，再检查粒子材料能否合法携带。
- 配队固定按等级 10 的 RawScore 搜索；游戏 Score 显示上限为 999999999。
- 最终配队额外预计算等级 1 至 20，成果页可实时切换等级观察分数、HP、阶段和特性事件。
- 输出每张卡的粒子位置、技能材料来源、敌我卡组和逐阶段战斗明细。

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

```bat
:: 计算固定 key 配方库
InFalsusCalc.exe craft --threads 20 --seconds 28800

:: 生成全部配队和独立 HTML
InFalsusCalc.exe compute --threads 20 --seconds 28800

:: 只重新计算指定回想
InFalsusCalc.exe compute --encounter 105 --threads 1 --seconds 900

:: 输出结构置信度报告
InFalsusCalc.exe confidence

:: 查看状态或请求停止
InFalsusCalc.exe status
InFalsusCalc.exe stop
```

`compute` 在全部 31 个回想完成后才替换最终 HTML。单回想计算只更新对应检查点，不发布不完整网页。

## 计算范围

制卡阶段分别计算净惩罚 0、1、2 的合法布局。固定 key 以槽数、左范围和右范围分组，每个惩罚层的攻击、防御和总和分别保留代表；数量、越界、重叠和断连共享同一个净惩罚预算，保存前仍由完整规则重新核验。低收益奖励区域按当前策略排除；零槽范围折叠，但区域绑定的攻防仍计入面板。

跨惩罚层删除候选时，只有同一卡的最终攻击、防御、精确结构、可选颜色和材料承载能力均可替代，才视为安全支配。成果页仍展示各惩罚层的正式目标代表；配队只使用支配后的候选。

配队阶段先保留以下模板：

- 3 槽且左范围与右范围之和至少为 4 的结构卡。
- 攻击、防御或总和达到全库对应最高值 80% 的面板卡。

卡名和位置骨架完整枚举，模板、颜色和技能使用有界 beam、随机多起点、单卡替换和前 24 邻居双卡联合搜索。结果属于当前模板和搜索策略内的 `best_found`，不声明所有底层模板、颜色和技能组合的数学全局最优。

战斗条件固定为联觉开启、25 个代表性全 EXACT 键、初始 HP 100。配队选择固定使用等级 10；等级 1 至 20 只复算同一推荐配队，不重新选择卡牌。

## 数据与目录

- `Data/catalog.json`：当前兼容的游戏资源快照。
- `Data/Seeds`：制卡求解的布局种子。
- `Evidence`：原生规则静态分析证据，不作为旧候选输入。
- `src`：制卡、配队、战斗和报告生成代码。
- `Web`：独立成果页模板。
- `docs/index.html`：当前已发布的静态成果页。

运行时产生的 `state`、`logs`、`output`、求解检查点和本地缓存不会提交到仓库。
