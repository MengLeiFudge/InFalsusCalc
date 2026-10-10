# In Falsus Best 50 来源与读取规则

本站第三个「Best 50」Tab 基于 [unknnownnn003/infalsus-b50](https://github.com/unknnownnn003/infalsus-b50) 修改，原网页为 https://unknnownnn003.github.io/infalsus-b50/。上游源码固定于 [aef251e0ab3fab99e8d6984af5af0b6558114f7c](https://github.com/unknnownnn003/infalsus-b50/tree/aef251e0ab3fab99e8d6984af5af0b6558114f7c)，版本 0.2.0。

保留上游界面、B50 排序、PNG／分享 JSON 导出、曲目快照和 78 张曲绘，修正原生存档的成绩配对，并增加潜力值模组的独立 JSON 输入。所有成绩仅在浏览器处理。原版第三方声明见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。

主页面通过 `../b50-tab.js` 按需加载本目录的 `index.html`，使用同源 iframe 隔离全局 CSS 与元素 ID。高度跟随 `.site-shell` 内容变化，切换 Tab 保留导入结果。

## 输入与计算

推荐选择 `savestate_ptt_v1.json`。此文件由 PotentialSystem 模组保存在 `savestate_V3.sav` 相同目录；每个歌曲／难度保存一项最佳表现，附带定数、未截断 PTT、完成等级、时间和来源。网页验证格式及计算一致性，采用文件中的定数；曲名、作者和曲绘仍来自本地曲目快照。未知或身份不同的谱面会明确列出，暂不计入 B50。

读取原生 `savestate_V3.sav` 时，遍历实际 `history` 中的每一次游玩。每条记录的分数与完成等级属于同一局，以歌曲／难度分组取最高表现；较低分的通关成绩也可能比更高分的未通关成绩有更高 PTT。相同表现优先已知的较早时间，原生时间为零或无效时显示“未知”。

通关以该局完成等级 >= DiveCleared（含 FullLink、PerfectDive）判定，加 0.2。原生 Lamp 仅看 Pace，可能与未判定音符导致的 DiveFailed 不一致。最高分字典仅用于定位其后的序列化 history 边界。

当前游戏的历史列表初始容量为 1024，写满时会扩容。解析器按序列化数组容量和实际条数读取，不使用固定 1024 上限，也不依靠空记录数量猜测结束位置。

原始分数为 S、定数为 C、通关奖励为 b（通关 0.2，否则 0）：

```text
S >= 100,000,000：C + 2 + b
98,000,000 <= S < 100,000,000：C + 1 + (S - 98,000,000) / 2,000,000 + b
S < 98,000,000：max(0, C + (S - 95,000,000) / 3,000,000 + b)
整体 PTT = (B50 总和 + B10 总和) / 60
```

均值固定除以 50／30／10，缺项按零计。页面与 PNG 显示截断到三位，排序保留完整计算精度。成绩明细显示完成等级、纪录时间（浏览器本地时区）及来源；分享 JSON 保留这些信息。分享文件 `in-falsus-b50.json` 是 B50 报告，不能替代模组的独立成绩文件。

## 独立 JSON 协议

根字段为 `schemaVersion: 1`、`rule: "integer-rating-v1"`、UTC `importedAt` 和 `bestPlays` 数组。导入时间与取得纪录的时间分别记录。

| 每条成绩字段 | 含义 |
| --- | --- |
| `songId`、`difficulty`、`chartId` | 原生歌曲编号、难度位标记（1／2／4／8）和谱面身份 |
| `score` | 原始打击分数的十进制字符串，避免 ulong 精度丢失 |
| `lamp`、`resultClear` | 用于 PTT 的通关状态（0／1／2）与完成等级（0～4） |
| `constant`、`potential` | 整数定数和未截断单曲 PTT |
| `achievedAt` | UTC ISO 时间；无法确定时为 null |
| `source` | `history`：原生真实历史；`play`：模组捕获的真实结算及时间 |

模组每次载入账号均从完整历史校准当前谱面的最佳纪录，游戏历史增加时同步；JSON 为匹配的真实记录提供已知时间，关闭模组期间的成绩也会从历史补入，未知时间保持 null。

旧 v1 JSON 若含 `source: "summary"`，新版模组会从 history 重建后替换该条目。网页单独读取这种旧 JSON 时会提示改用原生 `.sav` 或经新版模组同步后的文件。

JSON 最大 8 MiB，原生存档最大 64 MiB。损坏、重复谱面、未知规则或计算不一致会停止导入。

## 重新构建

修改仓库根目录的 `B50/` TypeScript 源码，在项目根目录用 Node.js 22.12 或更新版本执行：

```sh
npm --prefix B50 ci --ignore-scripts --no-audit --no-fund
npm --prefix B50 run build
```

构建先执行 TypeScript 检查，再由 Vite 直接更新 `docs/b50/` 的 HTML、脚本、样式和曲目快照，不运行测试、不清空现有曲绘及说明。不应手工修改生成脚本；更新上游素材时同步来源与第三方声明，并退役 HTML 不再引用的旧构建文件。
