import type { RatedScore } from "../rating/types";

export interface B50RenderEntry extends RatedScore {
  baseName: string;
  artist?: string;
  levelIndicator?: string;
  designer?: string;
  jacket?: {
    thumbnail: string;
  };
}

export interface B50RenderModel {
  entries: B50RenderEntry[];
  averageRating: number;
  totalRating: number;
  b30AverageRating: number;
  b30TotalRating: number;
  b10AverageRating: number;
  b10TotalRating: number;
  overallPotential: number;
  parsedCharts: number;
  matchedCharts: number;
  playerName?: string;
}

export interface B50RenderOptions {
  playerName?: string;
}
