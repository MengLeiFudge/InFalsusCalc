import type { ScoreRecord } from "../save/types";

export interface RatedScore extends ScoreRecord {
  rank: number;
  chartId: string;
  constant: number;
  rating: number;
  title: string;
  difficulty: string;
}

export interface UnmatchedScoreDiagnostic {
  songId: number;
  difficultyIndex: number;
}

export interface B50Result {
  entries: RatedScore[];
  totalRating: number;
  averageRating: number;
  b30TotalRating: number;
  b30AverageRating: number;
  b10TotalRating: number;
  b10AverageRating: number;
  overallPotential: number;
  totalParsedScores: number;
  matchedScores: number;
  unmatchedScores: number;
  diagnostics: UnmatchedScoreDiagnostic[];
}
