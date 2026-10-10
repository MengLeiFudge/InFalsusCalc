import type { CatalogIndex } from "../catalog/types";
import type { ScoreRecord } from "../save/types";
import { calculateRating } from "./rating";
import type { B50Result, RatedScore, UnmatchedScoreDiagnostic } from "./types";

export function buildB50(records: readonly ScoreRecord[], catalog: CatalogIndex): B50Result {
  const rated: RatedScore[] = [];
  const diagnostics: UnmatchedScoreDiagnostic[] = [];

  for (const record of records) {
    const chart = catalog.resolveBySaveRecord(record.songId, record.difficultyIndex);
    if (chart === null || (record.chartId !== undefined && record.chartId !== chart.chartId)) {
      diagnostics.push({ songId: record.songId, difficultyIndex: record.difficultyIndex });
      continue;
    }

    rated.push({
      ...record,
      rank: 0,
      chartId: chart.chartId,
      constant: record.constant ?? chart.constant,
      rating: calculateRating(record.constant ?? chart.constant, record.score, record.clearStatus),
      title: chart.title,
      difficulty: chart.difficulty,
    });
  }

  rated.sort((left, right) => {
    if (left.rating !== right.rating) return right.rating - left.rating;
    if (left.score !== right.score) return left.score > right.score ? -1 : 1;
    if (left.constant !== right.constant) return right.constant - left.constant;
    if (left.songId !== right.songId) return left.songId - right.songId;
    return left.difficultyIndex - right.difficultyIndex;
  });

  const entries = rated.slice(0, 50).map((entry, index) => ({ ...entry, rank: index + 1 }));
  const totalRating = entries.reduce((sum, entry) => sum + entry.rating, 0);
  const b30Entries = entries.slice(0, 30);
  const b10Entries = entries.slice(0, 10);
  const b30TotalRating = b30Entries.reduce((sum, entry) => sum + entry.rating, 0);
  const b10TotalRating = b10Entries.reduce((sum, entry) => sum + entry.rating, 0);

  return {
    entries,
    totalRating,
    averageRating: totalRating / 50,
    b30TotalRating,
    b30AverageRating: b30TotalRating / 30,
    b10TotalRating,
    b10AverageRating: b10TotalRating / 10,
    overallPotential: (totalRating + b10TotalRating) / 60,
    totalParsedScores: records.length,
    matchedScores: rated.length,
    unmatchedScores: diagnostics.length,
    diagnostics,
  };
}
