import type { CatalogIndex } from "../catalog/types";
import type { B50Result } from "../rating/types";
import type { B50RenderEntry, B50RenderModel, B50RenderOptions } from "./types";

function normalizePlayerName(value: string | undefined): string | undefined {
  if (value === undefined) return undefined;
  const normalized = value.trim().replace(/\s+/gu, " ").slice(0, 32);
  return normalized.length > 0 ? normalized : undefined;
}

export function buildB50RenderModel(
  result: B50Result,
  catalog: CatalogIndex,
  options: B50RenderOptions = {},
): B50RenderModel {
  if (result.entries.length > 50) throw new Error("B50 render model cannot contain more than 50 entries.");
  const entries: B50RenderEntry[] = result.entries.map((score) => {
    const chart = catalog.resolveBySaveRecord(score.songId, score.difficultyIndex);
    if (chart === null || chart.chartId !== score.chartId) {
      throw new Error("B50 result references chart metadata that is no longer in the catalog.");
    }
    // 保留输入采用的定数、完成等级、时间与来源，展示元数据由当前曲目快照补充。
    return {
      ...score,
      baseName: chart.baseName,
      title: chart.title,
      difficulty: chart.difficulty,
      ...(chart.artist === undefined ? {} : { artist: chart.artist }),
      ...(chart.levelIndicator === undefined ? {} : { levelIndicator: chart.levelIndicator }),
      ...(chart.designer === undefined ? {} : { designer: chart.designer }),
      ...(chart.jacket === undefined ? {} : { jacket: { ...chart.jacket } }),
    };
  });
  const playerName = normalizePlayerName(options.playerName);
  return {
    entries,
    averageRating: result.averageRating,
    totalRating: result.totalRating,
    b30AverageRating: result.b30AverageRating,
    b30TotalRating: result.b30TotalRating,
    b10AverageRating: result.b10AverageRating,
    b10TotalRating: result.b10TotalRating,
    overallPotential: result.overallPotential,
    parsedCharts: result.totalParsedScores,
    matchedCharts: result.matchedScores,
    ...(playerName === undefined ? {} : { playerName }),
  };
}
