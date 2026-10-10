import type { ChartIdentity, ChartMetadata, CatalogIndex } from "./types";

export function chartIdentityKey(identity: ChartIdentity): string {
  return `${identity.songId}:${identity.difficultyIndex}`;
}

export function createCatalogIndex(
  schemaVersion: number,
  catalogVersion: string,
  charts: ChartMetadata[],
): CatalogIndex {
  const byIdentity = new Map<string, ChartMetadata>();
  const byChartId = new Map<string, ChartMetadata>();

  for (const chart of charts) {
    byIdentity.set(chartIdentityKey(chart), chart);
    byChartId.set(chart.chartId, chart);
  }

  return {
    schemaVersion,
    catalogVersion,
    charts,
    resolveBySaveRecord: (songId, difficultyIndex) =>
      byIdentity.get(chartIdentityKey({ songId, difficultyIndex })) ?? null,
    resolveByChartId: (chartId) => byChartId.get(chartId) ?? null,
  };
}
