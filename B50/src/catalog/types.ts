export interface ChartIdentity {
  songId: number;
  difficultyIndex: number;
}

export interface ChartMetadata extends ChartIdentity {
  chartId: string;
  baseName: string;
  title: string;
  artist?: string;
  difficulty: string;
  constant: number;
  levelIndicator?: string;
  designer?: string;
  jacket?: {
    thumbnail: string;
  };
}

export interface InFalsusChartSource {
  difficultyIndex: number;
  difficulty: string;
  chartId: string;
  available: boolean;
  rating: number;
  levelIndicator?: string;
  designer?: string;
  jacket?: string;
}

export interface InFalsusSongSource {
  songId: number;
  baseName: string;
  title: string;
  artist?: string;
  jacket?: string;
  charts: InFalsusChartSource[];
}

export interface InFalsusSongList {
  schemaVersion: 1;
  source?: {
    gameVersion?: string;
    gameDataCommitId?: string;
    steamBuildId?: string;
    addressablesVersion?: string;
    fingerprint?: string;
  };
  songs: InFalsusSongSource[];
}

export interface CatalogIndex {
  readonly schemaVersion: number;
  readonly catalogVersion: string;
  readonly charts: readonly ChartMetadata[];
  resolveBySaveRecord(songId: number, difficultyIndex: number): ChartMetadata | null;
  resolveByChartId(chartId: string): ChartMetadata | null;
}
