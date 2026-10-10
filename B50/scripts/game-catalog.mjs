const DIFFICULTIES = ["MIN", "EVO", "ULT", "FBD"];
const DIFFICULTY_INDEX = new Map(DIFFICULTIES.map((name, index) => [name, index]));
const SHA256 = /^[a-f0-9]{64}$/u;

function compareText(left, right) {
  return left < right ? -1 : left > right ? 1 : 0;
}

function isRecord(value) {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function cleanOptionalString(value, field) {
  if (value === undefined || value === null || value === "") return undefined;
  if (typeof value !== "string") throw new Error(field + " must be a string when provided.");
  const cleaned = value.normalize("NFC").trim();
  return cleaned || undefined;
}

function requiredString(value, field) {
  const cleaned = cleanOptionalString(value, field);
  if (cleaned === undefined) throw new Error(field + " must be a non-empty string.");
  return cleaned;
}

function optionalJacketPath(value, field) {
  const jacket = cleanOptionalString(value, field);
  if (jacket === undefined) return undefined;
  if (!/^assets\/jackets\/[A-Za-z0-9._-]+\.webp$/u.test(jacket)) {
    throw new Error(field + " must be a local assets/jackets/*.webp path.");
  }
  return jacket;
}

function safeStem(value) {
  const stem = value.normalize("NFKC").replace(/[^A-Za-z0-9._-]+/gu, "_").replace(/^[._]+|[._]+$/gu, "");
  return stem || "song";
}

function normalizeSource(value) {
  if (value === undefined) return undefined;
  if (!isRecord(value)) throw new Error("source must be an object when provided.");
  const source = {};
  for (const field of ["gameVersion", "gameDataCommitId", "steamBuildId", "addressablesVersion"]) {
    const text = cleanOptionalString(value[field], "source." + field);
    if (text !== undefined) source[field] = text;
  }
  const fingerprint = cleanOptionalString(value.fingerprint, "source.fingerprint");
  if (fingerprint !== undefined) {
    if (!SHA256.test(fingerprint)) throw new Error("source.fingerprint must be a lowercase SHA-256 hex digest.");
    source.fingerprint = fingerprint;
  }
  return source;
}

function imageSourceIndex(value) {
  if (value === undefined) return new Map();
  if (!Array.isArray(value)) throw new Error("jacketSources must be an array when provided.");
  const sources = new Map();
  for (const [index, item] of value.entries()) {
    if (!isRecord(item)) throw new Error(`jacketSources[${index}] must be an object.`);
    const key = requiredString(item.key, `jacketSources[${index}].key`);
    const sha256 = requiredString(item.sha256, `jacketSources[${index}].sha256`);
    if (!SHA256.test(key) || !SHA256.test(sha256)) throw new Error(`jacketSources[${index}] must contain SHA-256 digests.`);
    if (item.width !== 320 || item.height !== 320) throw new Error(`jacketSources[${index}] must be 320×320.`);
    if (!Number.isSafeInteger(item.sizeBytes) || item.sizeBytes <= 0) throw new Error(`jacketSources[${index}].sizeBytes must be a positive integer.`);
    if (sources.has(key)) throw new Error("Duplicate jacket source key " + key + ".");
    sources.set(key, { key, sha256, sizeBytes: item.sizeBytes, width: 320, height: 320 });
  }
  return sources;
}

function optionalSourceKey(value, field, sources) {
  const key = cleanOptionalString(value, field);
  if (key === undefined) return undefined;
  if (!SHA256.test(key)) throw new Error(field + " must be a SHA-256 key.");
  // Missing image bytes are a supported jacket fallback. The chart metadata remains usable.
  return sources.has(key) ? key : undefined;
}

function parseSongRows(input, sources) {
  if (!isRecord(input) || input.schemaVersion !== 1 || !Array.isArray(input.songs)) {
    throw new Error("Game songlist must have schemaVersion 1 and a songs array.");
  }
  const songs = [];
  const songIds = new Set();
  const chartIds = new Set();
  const chartIdentities = new Set();

  for (const [songIndex, row] of input.songs.entries()) {
    if (!isRecord(row)) throw new Error(`songs[${songIndex}] must be an object.`);
    const songId = row.songId;
    if (!Number.isInteger(songId) || songId < 1 || songId > 0xffff) {
      throw new Error(`songs[${songIndex}].songId must be a non-zero u16 integer.`);
    }
    if (songIds.has(songId)) throw new Error("Duplicate songId " + songId + ".");
    songIds.add(songId);

    const baseName = requiredString(row.baseName, `songs[${songIndex}].baseName`);
    const title = cleanOptionalString(row.title, `songs[${songIndex}].title`) ?? baseName;
    const artist = cleanOptionalString(row.artist, `songs[${songIndex}].artist`);
    const jacket = optionalJacketPath(row.jacket, `songs[${songIndex}].jacket`);
    const jacketSourceKey = optionalSourceKey(row.jacketSourceKey, `songs[${songIndex}].jacketSourceKey`, sources);
    if (!Array.isArray(row.charts)) throw new Error(`songs[${songIndex}].charts must be an array.`);

    const charts = [];
    for (const [chartIndex, chartRow] of row.charts.entries()) {
      const field = `songs[${songIndex}].charts[${chartIndex}]`;
      if (!isRecord(chartRow)) throw new Error(field + " must be an object.");
      const difficultyIndex = chartRow.difficultyIndex;
      if (!Number.isInteger(difficultyIndex) || difficultyIndex < 0 || difficultyIndex >= DIFFICULTIES.length) {
        throw new Error(field + ".difficultyIndex must be an integer from 0 through 3.");
      }
      const difficulty = requiredString(chartRow.difficulty, field + ".difficulty");
      if (DIFFICULTY_INDEX.get(difficulty) !== difficultyIndex) {
        throw new Error(field + ".difficulty does not match its difficultyIndex.");
      }
      const chartId = requiredString(chartRow.chartId, field + ".chartId");
      const identity = songId + ":" + difficultyIndex;
      if (chartIdentities.has(identity)) throw new Error("Duplicate chart identity " + identity + ".");
      if (chartIds.has(chartId)) throw new Error("Duplicate chartId " + chartId + ".");
      chartIdentities.add(identity);
      chartIds.add(chartId);
      if (typeof chartRow.available !== "boolean") throw new Error(field + ".available must be a boolean.");
      if (typeof chartRow.rating !== "number" || !Number.isFinite(chartRow.rating) || chartRow.rating < 0) {
        throw new Error(field + ".rating must be a finite non-negative number.");
      }
      const levelIndicator = cleanOptionalString(chartRow.levelIndicator, field + ".levelIndicator");
      const designer = cleanOptionalString(chartRow.designer, field + ".designer");
      const chartJacket = optionalJacketPath(chartRow.jacket, field + ".jacket");
      const chartJacketSourceKey = optionalSourceKey(chartRow.jacketSourceKey, field + ".jacketSourceKey", sources);
      charts.push({
        difficultyIndex,
        difficulty,
        chartId,
        available: chartRow.available,
        rating: chartRow.rating,
        ...(levelIndicator === undefined ? {} : { levelIndicator }),
        ...(designer === undefined ? {} : { designer }),
        ...(chartJacket === undefined ? {} : { jacket: chartJacket }),
        ...(chartJacketSourceKey === undefined ? {} : { jacketSourceKey: chartJacketSourceKey }),
      });
    }
    charts.sort((left, right) => left.difficultyIndex - right.difficultyIndex || compareText(left.chartId, right.chartId));
    songs.push({
      songId,
      baseName,
      title,
      ...(artist === undefined ? {} : { artist }),
      ...(jacket === undefined ? {} : { jacket }),
      ...(jacketSourceKey === undefined ? {} : { jacketSourceKey }),
      charts,
    });
  }
  songs.sort((left, right) => left.songId - right.songId);
  return songs;
}

function planJackets(songs, sources) {
  const baseNameCounts = new Map();
  for (const song of songs) {
    const key = safeStem(song.baseName).toLowerCase();
    baseNameCounts.set(key, (baseNameCounts.get(key) ?? 0) + 1);
  }

  const filenames = new Map();
  const assets = new Map();
  const pathFor = (song, sourceKey, chartId, hasVariants) => {
    const source = sources.get(sourceKey);
    if (source === undefined) return undefined;
    const duplicateBase = (baseNameCounts.get(safeStem(song.baseName).toLowerCase()) ?? 0) > 1;
    let stem = safeStem(song.baseName);
    if (duplicateBase) stem += "--" + song.songId;
    if (hasVariants) stem += "--" + safeStem(chartId);
    let filename = stem + ".webp";
    const collision = filenames.get(filename.toLocaleLowerCase("en-US"));
    if (collision !== undefined && collision !== source.sha256.toLowerCase()) {
      filename = stem + "--" + source.sha256.slice(0, 8) + ".webp";
    }
    const folded = filename.toLowerCase();
    const priorHash = filenames.get(folded);
    if (priorHash !== undefined && priorHash !== source.sha256.toLowerCase()) {
      throw new Error("Jacket filename collision for " + filename + ".");
    }
    filenames.set(folded, source.sha256.toLowerCase());
    const path = "assets/jackets/" + filename;
    assets.set(filename, { sourceKey, targetFilename: filename, sha256: source.sha256, sizeBytes: source.sizeBytes, width: 320, height: 320 });
    return path;
  };

  const normalizedSongs = songs.map((song) => {
    const effectiveKeys = song.charts.map((chart) => chart.jacketSourceKey ?? song.jacketSourceKey);
    const uniqueKeys = [...new Set(effectiveKeys.filter((key) => key !== undefined && sources.has(key)))].sort();
    const hasVariants = uniqueKeys.length > 1;
    let songJacket = hasVariants ? undefined : song.jacket;
    if (!hasVariants) {
      const key = uniqueKeys[0] ?? song.jacketSourceKey;
      if (key !== undefined) songJacket = pathFor(song, key, "", false) ?? songJacket;
    }
    const pathByKey = new Map();
    const charts = song.charts.map((chart) => {
      const key = chart.jacketSourceKey ?? song.jacketSourceKey;
      let path = chart.jacket;
      if (hasVariants && key !== undefined) {
        path = pathByKey.get(key);
        if (path === undefined) {
          path = pathFor(song, key, chart.chartId, true);
          if (path !== undefined) pathByKey.set(key, path);
        }
      }
      return {
        ...chart,
        ...(path === undefined ? {} : { jacket: path }),
        jacketSourceKey: undefined,
      };
    }).map(({ jacketSourceKey: _sourceKey, ...chart }) => chart);
    return {
      ...song,
      ...(songJacket === undefined ? {} : { jacket: songJacket }),
      jacketSourceKey: undefined,
      charts,
    };
  }).map(({ jacketSourceKey: _sourceKey, ...song }) => song);

  return { songs: normalizedSongs, assets: [...assets.values()].sort((left, right) => compareText(left.targetFilename, right.targetFilename)) };
}

export function normalizeGameSongList(input) {
  const sources = imageSourceIndex(input?.jacketSources);
  const source = normalizeSource(input?.source);
  const songs = parseSongRows(input, sources);
  const jacketPlan = planJackets(songs, sources);
  const songList = {
    schemaVersion: 1,
    ...(source === undefined ? {} : { source }),
    songs: jacketPlan.songs,
  };
  // Re-validate the public output after temporary jacket keys are removed.
  parseSongRows(songList, new Map());
  return { songList, assets: jacketPlan.assets };
}

function normalizeRaCatalog(input) {
  if (!isRecord(input) || !Array.isArray(input.resources)) throw new Error("Rhythm Archive catalog must include a resources array.");
  const songs = new Map();
  const charts = new Map();
  for (const resource of input.resources) {
    if (!isRecord(resource) || resource.game !== "infalsus" || resource.resourceType !== "jacket") continue;
    const metadata = resource.metadata;
    if (!isRecord(metadata)) throw new Error("Rhythm Archive In Falsus resource metadata must be an object.");
    const songIdText = requiredString(metadata.songId, "Rhythm Archive metadata.songId");
    if (!/^(0|[1-9][0-9]*)$/u.test(songIdText)) throw new Error("Rhythm Archive metadata.songId must be a decimal integer.");
    const songId = Number(songIdText);
    if (!Number.isSafeInteger(songId) || songId < 1 || songId > 0xffff) throw new Error("Rhythm Archive metadata.songId must be a non-zero u16 integer.");
    if (songs.has(songId)) throw new Error("Duplicate Rhythm Archive songId " + songId + ".");
    const baseName = requiredString(metadata.baseName, "Rhythm Archive metadata.baseName");
    const title = cleanOptionalString(resource.title ?? resource.displayTitle ?? metadata.title, "Rhythm Archive title") ?? baseName;
    const artist = cleanOptionalString(metadata.artist, "Rhythm Archive artist");
    songs.set(songId, { songId, baseName, title, ...(artist === undefined ? {} : { artist }) });
    if (!Array.isArray(metadata.charts)) throw new Error("Rhythm Archive metadata.charts must be an array.");
    for (const chart of metadata.charts) {
      if (!isRecord(chart) || chart.available !== true) continue;
      const index = new Map([[1, 0], [2, 1], [4, 2], [8, 3]]).get(chart.difficulty);
      if (index === undefined) throw new Error("Unsupported Rhythm Archive difficulty flag.");
      const chartId = requiredString(chart.chartId, "Rhythm Archive chartId");
      const rating = chart.rating;
      if (typeof rating !== "number" || !Number.isFinite(rating) || rating < 0) throw new Error("Rhythm Archive chart rating must be a finite non-negative number.");
      const identity = songId + ":" + index;
      if (charts.has(identity)) throw new Error("Duplicate Rhythm Archive chart identity " + identity + ".");
      if ([...charts.values()].some((row) => row.chartId === chartId)) throw new Error("Duplicate Rhythm Archive chartId " + chartId + ".");
      charts.set(identity, { songId, difficultyIndex: index, chartId, rating });
    }
  }
  return { songs, charts };
}

export function diffGameCatalogAgainstRa(gameInput, raInput) {
  const gameList = normalizeGameSongList(gameInput).songList;
  const gameSongs = new Map(gameList.songs.map((song) => [song.songId, song]));
  const gameCharts = new Map();
  for (const song of gameList.songs) {
    for (const chart of song.charts) gameCharts.set(song.songId + ":" + chart.difficultyIndex, { ...chart, songId: song.songId });
  }
  const ra = normalizeRaCatalog(raInput);
  const gameOnlySongs = [...gameSongs.keys()].filter((id) => !ra.songs.has(id)).sort((a, b) => a - b).map((id) => gameSongs.get(id));
  const raOnlySongs = [...ra.songs.keys()].filter((id) => !gameSongs.has(id)).sort((a, b) => a - b).map((id) => ra.songs.get(id));
  const gameOnlyCharts = [...gameCharts.entries()].filter(([identity]) => !ra.charts.has(identity)).sort(([a], [b]) => compareText(a, b)).map(([, chart]) => chart);
  const raOnlyCharts = [...ra.charts.entries()].filter(([identity]) => !gameCharts.has(identity)).sort(([a], [b]) => compareText(a, b)).map(([, chart]) => chart);
  const chartIdMismatch = [];
  const ratingMismatch = [];
  for (const [identity, gameChart] of gameCharts) {
    const raChart = ra.charts.get(identity);
    if (raChart === undefined) continue;
    if (gameChart.chartId !== raChart.chartId) chartIdMismatch.push({ identity, gameChartId: gameChart.chartId, raChartId: raChart.chartId });
    if (gameChart.rating !== raChart.rating) ratingMismatch.push({ identity, gameRating: gameChart.rating, raRating: raChart.rating });
  }
  const titleMismatch = [];
  const artistMismatch = [];
  for (const [songId, gameSong] of gameSongs) {
    const raSong = ra.songs.get(songId);
    if (raSong === undefined) continue;
    if (gameSong.title !== raSong.title) titleMismatch.push({ songId, gameTitle: gameSong.title, raTitle: raSong.title });
    if ((gameSong.artist ?? "") !== (raSong.artist ?? "")) artistMismatch.push({ songId, gameArtist: gameSong.artist ?? null, raArtist: raSong.artist ?? null });
  }
  return { gameOnlySongs, raOnlySongs, gameOnlyCharts, raOnlyCharts, chartIdMismatch, ratingMismatch, titleMismatch, artistMismatch };
}
