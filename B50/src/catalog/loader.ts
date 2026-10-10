import bundledSnapshot from "./songlist.json";
import { CatalogError } from "./errors";
import { createCatalogIndex } from "./resolver";
import type { CatalogIndex, ChartMetadata } from "./types";
import { normalizeGameSongList } from "../../scripts/game-catalog.mjs";

const SUPPORTED_SCHEMA_VERSION = 1;

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

export function createCatalog(snapshot: unknown): CatalogIndex {
  if (!isRecord(snapshot)) {
    throw new CatalogError("invalid-catalog", "Catalog root must be an object.");
  }

  const schemaVersion = snapshot["schemaVersion"];
  if (schemaVersion !== SUPPORTED_SCHEMA_VERSION) {
    throw new CatalogError(
      "unsupported-schema",
      `Unsupported catalog schemaVersion: ${String(schemaVersion)}.`,
    );
  }
  let songList;
  try {
    songList = normalizeGameSongList(snapshot).songList;
  } catch (error) {
    const message = error instanceof Error ? error.message : "Game songlist is malformed.";
    const code = message.startsWith("Duplicate chart identity")
      ? "duplicate-identity"
      : message.startsWith("Duplicate chartId")
        ? "duplicate-chart-id"
        : "invalid-catalog";
    throw new CatalogError(code, message);
  }

  const fingerprint = songList.source?.fingerprint;
  const catalogVersion = fingerprint === undefined ? "game-songlist-v1" : "game-" + fingerprint.slice(0, 12);
  const charts: ChartMetadata[] = [];
  for (const song of songList.songs) {
    for (const chart of song.charts) {
      if (!chart.available) continue;
      const jacketPath = chart.jacket ?? song.jacket;
      charts.push({
        songId: song.songId,
        difficultyIndex: chart.difficultyIndex,
        chartId: chart.chartId,
        baseName: song.baseName,
        title: song.title,
        difficulty: chart.difficulty,
        constant: chart.rating,
        ...(chart.levelIndicator === undefined ? {} : { levelIndicator: chart.levelIndicator }),
        ...(chart.designer === undefined ? {} : { designer: chart.designer }),
        ...(song.artist === undefined ? {} : { artist: song.artist }),
        ...(jacketPath === undefined ? {} : { jacket: { thumbnail: jacketPath } }),
      });
    }
  }

  return createCatalogIndex(SUPPORTED_SCHEMA_VERSION, catalogVersion, charts);
}

let bundledCatalog: CatalogIndex | undefined;

export function loadCatalog(): CatalogIndex {
  bundledCatalog ??= createCatalog(bundledSnapshot);
  return bundledCatalog;
}
