import { calculateRating } from "../rating/rating";
import type { ScoreRecord } from "./types";

export const MAX_PTT_FILE_BYTES = 8 * 1024 * 1024;
const MAX_SCORE = 18_446_744_073_709_551_615n;
const REQUIRED_PLAY_FIELDS = [
  "songId", "difficulty", "chartId", "score", "lamp", "resultClear",
  "constant", "potential", "achievedAt", "source",
];

function invalid(message: string): never {
  throw new Error("PTT 文件无效：" + message);
}

function object(value: unknown): Record<string, unknown> {
  if (value === null || typeof value !== "object" || Array.isArray(value)) invalid("预期 JSON 对象。");
  return value as Record<string, unknown>;
}

function integer(value: unknown, maximum: number): value is number {
  return typeof value === "number" && Number.isInteger(value) && value >= 0 && value <= maximum;
}

/** 接受模组输出的 UTC ISO 时间，拒绝日期自动进位、时区偏移或占位时间。 */
function utcTime(value: unknown): value is string {
  if (typeof value !== "string" || !/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,7})?(?:Z|\+00:00)$/.test(value)) return false;
  const time = new Date(value);
  return Number.isFinite(time.getTime()) && time.toISOString().slice(0, 19) === value.slice(0, 19);
}

/** 读取模组从逐局历史同步的真实最佳纪录及已知时间。 */
export function parsePttFile(text: string): ScoreRecord[] {
  const document = object(JSON.parse(text.replace(/^\uFEFF/, "")) as unknown);
  if (document["schemaVersion"] !== 1 || document["rule"] !== "integer-rating-v1"
    || !utcTime(document["importedAt"]) || !Array.isArray(document["bestPlays"])
    || document["bestPlays"].length > 10_000) invalid("格式版本、规则、导入时间或成绩数组不受支持。");
  const identities = new Set<string>();
  return document["bestPlays"].map((value: unknown): ScoreRecord => {
    const play = object(value);
    if (REQUIRED_PLAY_FIELDS.some((field) => !Object.hasOwn(play, field))) invalid("成绩缺少必要字段。");
    const songId = play["songId"];
    const difficulty = play["difficulty"];
    const lamp = play["lamp"];
    const resultClear = play["resultClear"];
    const constant = play["constant"];
    const chartId = play["chartId"];
    const scoreText = play["score"];
    const potential = play["potential"];
    const achievedAt = play["achievedAt"];
    const source = play["source"];
    if (source === "summary") invalid("包含旧版组合成绩。请读取 savestate_V3.sav，或使用新版模组从历史同步后的 JSON。");
    if (!integer(songId, 65_535) || songId === 0 || !integer(difficulty, 8)
      || ![1, 2, 4, 8].includes(difficulty) || !integer(lamp, 2) || !integer(resultClear, 4)
      || (resultClear >= 2) !== (lamp === 2) || !integer(constant, 2_147_483_647)
      || typeof chartId !== "string" || chartId.trim().length === 0
      || typeof scoreText !== "string" || !/^(0|[1-9]\d*)$/.test(scoreText)
      || scoreText.length > 20 || typeof potential !== "number" || !Number.isFinite(potential)
      || potential < 0 || (achievedAt !== null && !utcTime(achievedAt))
      || (source !== "history" && source !== "play") || (source === "play" && achievedAt === null)) {
      invalid("成绩的身份、分数、完成等级、来源或时间不符合协议。");
    }
    const score = BigInt(scoreText);
    const clearStatus = lamp === 2 ? "cleared" : "failed";
    const expected = calculateRating(constant, score, clearStatus);
    if (score > MAX_SCORE || Math.abs(potential - expected) > 1e-8) invalid("保存的 PTT 与分数、通关状态及定数不一致。");
    const key = `${songId}:${difficulty}`;
    if (identities.has(key)) invalid("同一个谱面出现多条最佳记录。");
    identities.add(key);
    return {
      songId,
      difficultyIndex: Math.log2(difficulty),
      chartId,
      constant,
      score,
      clearStatus,
      resultClear,
      achievedAt,
      source,
    };
  });
}
