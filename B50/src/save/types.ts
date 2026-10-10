/** 真实成绩来源：原生逐局历史，或模组捕获的结算及时间。 */
export type ScoreSource = "history" | "play";

/** 每个歌曲与难度最多一条的最佳表现，供计算、明细和导出共用。 */
export interface ScoreRecord {
  songId: number;
  difficultyIndex: number;
  score: bigint;
  clearStatus: "failed" | "cleared";
  resultClear: number;
  achievedAt: string | null;
  source: ScoreSource;
  /** 独立 JSON 带有谱面身份和定数，原生 .sav 使用网页曲库补充。 */
  chartId?: string;
  constant?: number;
}
