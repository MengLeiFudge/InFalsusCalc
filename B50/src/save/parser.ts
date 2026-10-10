import { performanceUnits } from "../rating/rating";
import { BinaryReader } from "./binary-reader";
import { SaveParseError } from "./errors";
import type { ScoreRecord } from "./types";

const RESULT_BYTES = 96;
const KEY_BYTES = 3;
const PAYLOAD_BYTES = KEY_BYTES + RESULT_BYTES;
const MAX_RECORDS = 10_000;
const MAX_BASENAME_BYTES = 256;
const MIN_ELEMENT_BYTES = 1 + 8 + 1 + PAYLOAD_BYTES;
export const MAX_SAVE_FILE_BYTES = 64 * 1024 * 1024;
const VALID_DIFFICULTIES = new Set([1, 2, 4, 8]);

/** 单条原生成绩的分数、完成等级与 UTC 秒值。 */
interface NativeRecord {
  songId: number;
  difficultyIndex: number;
  score: bigint;
  resultClear: number;
  seconds: bigint;
}

/** 完整 GameResultsV4 结构内的实际逐局历史。 */
interface Candidate {
  history: NativeRecord[];
}

function readAsciiBaseName(reader: BinaryReader, offset: number): string | null {
  if (!reader.hasRange(offset, 8)) return null;
  const encodedLength = reader.readInt32LE(offset);
  const length = reader.readInt32LE(offset + 4);
  if (length < 1 || length > MAX_BASENAME_BYTES || encodedLength !== -(length + 1)
    || !reader.hasRange(offset + 8, length)) return null;
  let name = "";
  for (const byte of reader.readBytes(offset + 8, length)) {
    if (byte < 0x20 || byte > 0x7e) return null;
    name += String.fromCharCode(byte);
  }
  return name;
}

function readResult(reader: BinaryReader, offset: number): NativeRecord | null {
  if (!reader.hasRange(offset, RESULT_BYTES)) return null;
  const songId = reader.readUint16LE(offset);
  const difficulty = reader.readUint8(offset + 2);
  const lamp = reader.readUint8(offset + 3);
  const resultClear = reader.readUint8(offset + 4);
  if (songId === 0 || !VALID_DIFFICULTIES.has(difficulty) || lamp > 2 || resultClear > 4) return null;
  return {
    songId,
    difficultyIndex: Math.log2(difficulty),
    score: reader.readBigUint64LE(offset + 0x50),
    resultClear,
    seconds: reader.readBigUint64LE(offset + 0x58),
  };
}

/** history 的数组容量和实际 length 分开保存；空历史合法，缺失或损坏结构非法。 */
function readHistory(reader: BinaryReader, offset: number): NativeRecord[] | null {
  if (!reader.hasRange(offset, 5) || reader.readUint8(offset) !== 2) return null;
  const capacity = reader.readInt32LE(offset + 1);
  const bufferOffset = offset + 5;
  if (capacity < 0 || !reader.hasRange(bufferOffset, capacity * RESULT_BYTES + 4)) return null;
  const count = reader.readInt32LE(bufferOffset + capacity * RESULT_BYTES);
  if (count < 0 || count > capacity) return null;
  const history: NativeRecord[] = [];
  for (let index = 0; index < count; index += 1) {
    const record = readResult(reader, bufferOffset + index * RESULT_BYTES);
    if (record === null) return null;
    history.push(record);
  }
  return history;
}

/** 通过四字段对象头和字典边界定位 history；最高分条目仅用于验证序列化结构。 */
function parseCandidate(reader: BinaryReader, firstOffset: number, count: number): Candidate | null {
  if (!reader.hasRange(firstOffset - 5, 1) || reader.readUint8(firstOffset - 5) !== 4
    || count < 1 || count > MAX_RECORDS || count * MIN_ELEMENT_BYTES > reader.length - firstOffset) return null;
  const identities = new Set<string>();
  let offset = firstOffset;
  for (let index = 0; index < count; index += 1) {
    const baseName = readAsciiBaseName(reader, offset + 1);
    if (baseName === null) return null;
    const payloadOffset = offset + 1 + 8 + baseName.length;
    if (!reader.hasRange(payloadOffset, PAYLOAD_BYTES)) return null;
    const record = readResult(reader, payloadOffset + KEY_BYTES);
    if (record === null || reader.readUint16LE(payloadOffset) !== record.songId
      || reader.readUint8(payloadOffset + 2) !== 2 ** record.difficultyIndex) return null;
    const key = `${record.songId}:${record.difficultyIndex}`;
    if (identities.has(key)) return null;
    identities.add(key);
    offset = payloadOffset + PAYLOAD_BYTES;
  }
  const history = readHistory(reader, offset);
  return history === null ? null : { history };
}

function historyTime(seconds: bigint): string | null {
  return seconds > 0n && seconds <= 253_402_300_799n
    ? new Date(Number(seconds) * 1000).toISOString() : null;
}

/** 遍历每一次真实游玩，每个谱面取最高表现，同表现优先已知的较早时间。 */
function bestFromHistory(history: readonly NativeRecord[]): ScoreRecord[] {
  const best = new Map<string, ScoreRecord>();
  for (const record of history) {
    const next: ScoreRecord = {
      songId: record.songId,
      difficultyIndex: record.difficultyIndex,
      score: record.score,
      clearStatus: record.resultClear >= 2 ? "cleared" : "failed",
      resultClear: record.resultClear,
      achievedAt: historyTime(record.seconds),
      source: "history",
    };
    const key = `${record.songId}:${record.difficultyIndex}`;
    const previous = best.get(key);
    if (previous !== undefined) {
      const difference = performanceUnits(next.score, next.clearStatus) - performanceUnits(previous.score, previous.clearStatus);
      const earlier = next.achievedAt !== null
        && (previous.achievedAt === null || next.achievedAt < previous.achievedAt);
      if (difference < 0n || (difference === 0n && !earlier)) continue;
    }
    best.set(key, next);
  }
  return [...best.values()];
}

export function parseSaveFile(input: ArrayBuffer | Uint8Array): ScoreRecord[] {
  const bytes = input instanceof Uint8Array ? input : new Uint8Array(input);
  if (bytes.byteLength > MAX_SAVE_FILE_BYTES) {
    throw new SaveParseError("file-too-large", "原生存档超过 64 MiB，已停止读取。");
  }
  if (bytes.byteLength < 14) {
    throw new SaveParseError("file-too-small", "文件太小，未找到完整的游玩历史结构。");
  }
  const reader = new BinaryReader(bytes);
  const candidates: Candidate[] = [];
  // 对象头、字典条数与连续记录、其后的历史容量和 length 必须同时匹配。
  for (let offset = 5; offset <= reader.length - 9; offset += 1) {
    if (reader.readUint8(offset - 5) !== 4) continue;
    const count = reader.readInt32LE(offset - 4);
    if (count < 1 || count > MAX_RECORDS || count * MIN_ELEMENT_BYTES > reader.length - offset) continue;
    const candidate = parseCandidate(reader, offset, count);
    if (candidate !== null) candidates.push(candidate);
  }
  if (candidates.length === 0) {
    throw new SaveParseError("unsupported-format", "未识别到完整的游玩历史结构。请检查存档版本，或使用模组同步生成的 savestate_ptt_v1.json。");
  }
  if (candidates.length !== 1) {
    throw new SaveParseError("ambiguous-record-array", "存在多组可能的成绩结构，已停止读取以免使用错误记录。");
  }
  return bestFromHistory(candidates[0]!.history);
}
