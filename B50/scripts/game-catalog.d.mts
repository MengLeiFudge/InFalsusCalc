export interface GameCatalogSource {
  gameVersion?: string;
  gameDataCommitId?: string;
  steamBuildId?: string;
  addressablesVersion?: string;
  fingerprint?: string;
}

export interface GameChartRow {
  difficultyIndex: number;
  difficulty: string;
  chartId: string;
  available: boolean;
  rating: number;
  levelIndicator?: string;
  designer?: string;
  jacket?: string;
}

export interface GameSongRow {
  songId: number;
  baseName: string;
  title: string;
  artist?: string;
  jacket?: string;
  charts: GameChartRow[];
}

export interface GameSongList {
  schemaVersion: 1;
  source?: GameCatalogSource;
  songs: GameSongRow[];
}

export interface JacketAssetPlan {
  sourceKey: string;
  targetFilename: string;
  sha256: string;
  sizeBytes: number;
  width: 320;
  height: 320;
}

export interface RhythmArchiveDiff {
  gameOnlySongs: Array<{ songId: number; baseName: string; title: string; artist?: string }>;
  raOnlySongs: Array<{ songId: number; baseName: string; title: string; artist?: string }>;
  gameOnlyCharts: Array<GameChartRow & { songId: number }>;
  raOnlyCharts: Array<{ songId: number; difficultyIndex: number; chartId: string; rating: number }>;
  chartIdMismatch: Array<{ identity: string; gameChartId: string; raChartId: string }>;
  ratingMismatch: Array<{ identity: string; gameRating: number; raRating: number }>;
  titleMismatch: Array<{ songId: number; gameTitle: string; raTitle: string }>;
  artistMismatch: Array<{ songId: number; gameArtist: string | null; raArtist: string | null }>;
}

export function normalizeGameSongList(input: unknown): { songList: GameSongList; assets: JacketAssetPlan[] };
export function diffGameCatalogAgainstRa(gameInput: unknown, raInput: unknown): RhythmArchiveDiff;
