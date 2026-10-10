import "./style.css";
import { loadCatalog } from "./catalog/loader";
import { buildB50 } from "./rating/b50";
import type { B50Result, RatedScore } from "./rating/types";
import { jacketAssetUrl } from "./render/assets";
import { getDifficultyTheme } from "./render/difficulty-theme";
import { buildB50RenderModel } from "./render/model";
import { renderB50Png } from "./render/b50-renderer";
import { MAX_SAVE_FILE_BYTES, parseSaveFile } from "./save/parser";
import { MAX_PTT_FILE_BYTES, parsePttFile } from "./save/ptt-parser";
import { formatPotential } from "./rating/rating";
import type { ScoreSource } from "./save/types";

const catalog = loadCatalog();
const fileInput = getElement<HTMLInputElement>("file-input");
const dropZone = getElement<HTMLDivElement>("drop-zone");
const browseButton = getElement<HTMLButtonElement>("browse-button");
const status = getElement<HTMLParagraphElement>("status");
const resultsSection = getElement<HTMLElement>("results");
const pngButton = getElement<HTMLButtonElement>("export-png-button");
const jsonButton = getElement<HTMLButtonElement>("export-json-button");
const playerNameInput = getElement<HTMLInputElement>("player-name");
const exportStatus = getElement<HTMLParagraphElement>("export-status");
const cardGrid = getElement<HTMLDivElement>("b50-cards");
const tableBody = getElement<HTMLTableSectionElement>("table-body");
const diagnosticsSection = getElement<HTMLDivElement>("diagnostics");
const diagnosticList = getElement<HTMLUListElement>("diagnostic-list");
const copyPathButton = getElement<HTMLButtonElement>("copy-path-button");
const copyDirectoryButton = getElement<HTMLButtonElement>("copy-directory-button");
const saveDirectoryValue = getElement<HTMLElement>("save-directory-value");
const savePathValue = getElement<HTMLElement>("save-path-value");

let currentResult: B50Result | null = null;
let fileRequestSequence = 0;
let pngRenderSequence = 0;
let activePngRenderSequence: number | null = null;
const copyPathResetTimers = new WeakMap<HTMLButtonElement, number>();

function getElement<T extends HTMLElement>(id: string): T {
  const element = document.getElementById(id);
  if (element === null) throw new Error("Missing required page element #" + id + ".");
  return element as T;
}

function formatInteger(value: number | bigint): string {
  return new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 }).format(value);
}

function formatRating(value: number): string {
  return formatPotential(value);
}

const sourceLabels: Record<ScoreSource, string> = { history: "原生历史", play: "模组纪录" };
const clearLabels = ["None", "DiveFailed", "DiveCleared", "FullLink", "PerfectDive"];

function formatTime(value: string | null): string {
  return value === null ? "未知" : new Intl.DateTimeFormat("zh-CN", {
    year: "numeric", month: "2-digit", day: "2-digit",
    hour: "2-digit", minute: "2-digit", second: "2-digit", hour12: false,
  }).format(new Date(value));
}

function addCell(row: HTMLTableRowElement, value: string, className?: string): void {
  const cell = document.createElement("td");
  cell.textContent = value;
  if (className !== undefined) cell.className = className;
  row.append(cell);
}

function makeCard(entry: ReturnType<typeof buildB50RenderModel>["entries"][number], index: number): HTMLElement {
  const theme = getDifficultyTheme(entry.difficultyIndex);
  const card = document.createElement("article");
  card.className = "b50-card";
  card.dataset["difficulty"] = theme.className;
  card.setAttribute("role", "listitem");
  card.style.setProperty("--difficulty-accent", theme.accent);
  card.style.setProperty("--difficulty-wash", theme.wash);

  const top = document.createElement("div");
  top.className = "card-topline";
  const rank = document.createElement("span");
  rank.className = "card-rank";
  rank.textContent = "#" + String(entry.rank).padStart(2, "0");
  const difficulty = document.createElement("span");
  difficulty.className = "difficulty-badge";
  difficulty.textContent = theme.label;
  top.append(rank, difficulty);

  const song = document.createElement("div");
  song.className = "card-song";
  const jacket = document.createElement("div");
  jacket.className = "card-jacket";
  const placeholder = document.createElement("span");
  placeholder.className = "cover-placeholder";
  placeholder.setAttribute("aria-hidden", "true");
  placeholder.textContent = "IF";
  jacket.append(placeholder);
  if (entry.jacket !== undefined) {
    const image = document.createElement("img");
    image.src = jacketAssetUrl(entry.jacket.thumbnail, import.meta.env.BASE_URL);
    image.alt = entry.title + " jacket";
    image.decoding = "async";
    image.loading = index < 10 ? "eager" : "lazy";
    image.addEventListener("load", () => jacket.classList.add("cover-loaded"), { once: true });
    image.addEventListener("error", () => jacket.classList.add("cover-failed"), { once: true });
    jacket.append(image);
  } else {
    jacket.classList.add("cover-failed");
  }

  const songText = document.createElement("div");
  songText.className = "card-song-copy";
  const title = document.createElement("h3");
  title.className = "card-title";
  title.textContent = entry.title;
  const artist = document.createElement("p");
  artist.className = "card-artist";
  artist.textContent = entry.artist ?? " ";
  const constant = document.createElement("p");
  constant.className = "card-constant";
  constant.textContent = "CONST " + (Number.isInteger(entry.constant) ? String(entry.constant) : entry.constant.toFixed(1));
  songText.append(title, artist, constant);
  song.append(jacket, songText);

  const score = document.createElement("div");
  score.className = "card-scoreline";
  const scoreValue = document.createElement("div");
  scoreValue.className = "card-score";
  const scoreLabel = document.createElement("span");
  scoreLabel.textContent = "SCORE";
  const scoreNumber = document.createElement("strong");
  scoreNumber.textContent = formatInteger(entry.score);
  scoreValue.append(scoreLabel, scoreNumber);
  const rating = document.createElement("div");
  rating.className = "card-rating";
  const ratingLabel = document.createElement("span");
  ratingLabel.textContent = "POTENTIAL";
  const ratingNumber = document.createElement("strong");
  ratingNumber.textContent = formatRating(entry.rating);
  rating.append(ratingLabel, ratingNumber);
  score.append(scoreValue, rating);

  card.append(top, song, score);
  return card;
}

function renderCards(result: B50Result): void {
  cardGrid.replaceChildren();
  const model = buildB50RenderModel(result, catalog);
  if (model.entries.length === 0) {
    const empty = document.createElement("div");
    empty.className = "empty-b50";
    const mark = document.createElement("span");
    mark.className = "empty-mark";
    mark.textContent = "—";
    const title = document.createElement("h3");
    title.textContent = "存档读到了，但一张谱面都没匹配上";
    const message = document.createElement("p");
    message.textContent = "展开下面的明细，看看是哪些谱面没认出来。";
    empty.append(mark, title, message);
    cardGrid.append(empty);
    return;
  }
  model.entries.forEach((entry, index) => cardGrid.append(makeCard(entry, index)));
}

function renderTable(entries: RatedScore[]): void {
  tableBody.replaceChildren();
  if (entries.length === 0) {
    const row = document.createElement("tr");
    const cell = document.createElement("td");
    cell.colSpan = 10;
    cell.className = "empty-cell";
    cell.textContent = "没有匹配上的谱面。";
    row.append(cell);
    tableBody.append(row);
    return;
  }

  for (const entry of entries) {
    const row = document.createElement("tr");
    addCell(row, String(entry.rank), "rank-cell");
    addCell(row, entry.title);
    addCell(row, entry.difficulty);
    addCell(row, String(entry.constant));
    addCell(row, formatInteger(entry.score), "number-cell");
    addCell(row, formatRating(entry.rating), "number-cell");
    addCell(row, clearLabels[entry.resultClear] ?? "未知");
    addCell(row, formatTime(entry.achievedAt));
    addCell(row, sourceLabels[entry.source]);
    addCell(row, entry.chartId, "chart-id-cell");
    tableBody.append(row);
  }
}

function renderDiagnostics(result: B50Result): void {
  diagnosticList.replaceChildren();
  diagnosticsSection.hidden = result.diagnostics.length === 0;
  for (const diagnostic of result.diagnostics.slice(0, 50)) {
    const item = document.createElement("li");
    item.textContent = "songId " + diagnostic.songId + " · 难度 " + diagnostic.difficultyIndex;
    diagnosticList.append(item);
  }
  if (result.diagnostics.length > 50) {
    const more = document.createElement("li");
    more.textContent = "还有 " + formatInteger(result.diagnostics.length - 50) + " 条没列出来。";
    diagnosticList.append(more);
  }
}

function renderResult(result: B50Result): void {
  const model = buildB50RenderModel(result, catalog);
  getElement<HTMLElement>("unmatched-count").textContent = formatInteger(result.unmatchedScores);
  getElement<HTMLElement>("b50-count").textContent = formatInteger(model.entries.length);
  getElement<HTMLElement>("b50-average-rating").textContent = formatRating(model.averageRating);
  getElement<HTMLElement>("b30-average-rating").textContent = formatRating(model.b30AverageRating);
  getElement<HTMLElement>("b10-average-rating").textContent = formatRating(model.b10AverageRating);
  getElement<HTMLElement>("overall-potential").textContent = formatRating(model.overallPotential);
  renderDiagnostics(result);
  renderTable(result.entries);
  renderCards(result);
  resultsSection.hidden = false;
  pngButton.disabled = result.entries.length === 0;
  jsonButton.disabled = false;
  exportStatus.textContent = "";
}

function showError(message: string): void {
  currentResult = null;
  resultsSection.hidden = true;
  status.textContent = message;
  status.className = "status status-error";
  pngButton.disabled = true;
  jsonButton.disabled = true;
}

async function copyPath(button: HTMLButtonElement, path: string, defaultLabel: string): Promise<void> {
  let label = "已复制";
  try {
    await navigator.clipboard.writeText(path);
  } catch {
    label = "请手动复制";
  }
  button.textContent = label;
  const previousTimer = copyPathResetTimers.get(button);
  if (previousTimer !== undefined) window.clearTimeout(previousTimer);
  const resetTimer = window.setTimeout(() => {
    button.textContent = defaultLabel;
    copyPathResetTimers.delete(button);
  }, 2200);
  copyPathResetTimers.set(button, resetTimer);
}

async function processFile(file: File | undefined): Promise<void> {
  if (file === undefined) return;
  const requestSequence = ++fileRequestSequence;
  pngRenderSequence += 1;
  activePngRenderSequence = null;
  const filename = file.name.toLowerCase();
  const isPtt = filename.endsWith(".json");
  if (!isPtt && !filename.endsWith(".sav")) {
    showError("请选择 savestate_ptt_v1.json 或 savestate_V3.sav。");
    fileInput.value = "";
    return;
  }
  const maximumBytes = isPtt ? MAX_PTT_FILE_BYTES : MAX_SAVE_FILE_BYTES;
  if (file.size > maximumBytes) {
    showError("文件超过 " + maximumBytes / (1024 * 1024) + " MiB，已停止读取。");
    fileInput.value = "";
    return;
  }

  status.textContent = "正在读取存档…";
  status.className = "status";
  exportStatus.textContent = "";
  pngButton.disabled = true;
  jsonButton.disabled = true;
  resultsSection.hidden = true;
  dropZone.setAttribute("aria-busy", "true");

  try {
    const bytes = await file.arrayBuffer();
    if (requestSequence !== fileRequestSequence) return;
    const records = isPtt
      ? parsePttFile(new TextDecoder("utf-8", { fatal: true }).decode(bytes))
      : parseSaveFile(bytes);
    const result = buildB50(records, catalog);
    currentResult = result;
    renderResult(result);
    status.textContent = "已读取 " + records.length + " 张谱面的真实最佳成绩，下面是你的 Best 50。";
    status.className = "status status-success";
  } catch (error) {
    if (requestSequence !== fileRequestSequence) return;
    const message = error instanceof Error ? error.message : "存档没读出来，换一个文件再试试。";
    showError(message);
  } finally {
    if (requestSequence === fileRequestSequence) {
      dropZone.removeAttribute("aria-busy");
      fileInput.value = "";
    }
  }
}

function downloadBlob(blob: Blob, filename: string): void {
  const objectUrl = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = objectUrl;
  link.download = filename;
  document.body.append(link);
  link.click();
  link.remove();
  window.setTimeout(() => URL.revokeObjectURL(objectUrl), 1000);
}

function currentRenderModel() {
  if (currentResult === null) return null;
  return buildB50RenderModel(currentResult, catalog, { playerName: playerNameInput.value });
}

async function exportPng(): Promise<void> {
  const sourceResult = currentResult;
  if (sourceResult === null || sourceResult.entries.length === 0) return;
  const sourceFileSequence = fileRequestSequence;
  const sourcePlayerName = playerNameInput.value;
  const model = buildB50RenderModel(sourceResult, catalog, { playerName: sourcePlayerName });
  const exportSequence = ++pngRenderSequence;
  activePngRenderSequence = exportSequence;
  pngButton.disabled = true;
  exportStatus.textContent = "正在加载曲绘并绘制图片…";
  try {
    const rendered = await renderB50Png(model, { baseUrl: import.meta.env.BASE_URL });
    if (
      exportSequence !== pngRenderSequence
      || activePngRenderSequence !== exportSequence
      || sourceFileSequence !== fileRequestSequence
      || currentResult !== sourceResult
      || playerNameInput.value !== sourcePlayerName
    ) return;
    downloadBlob(rendered.blob, "in-falsus-b50.png");
    exportStatus.textContent = "PNG 已开始下载，尺寸 " + rendered.width + " × " + rendered.height + "。";
  } catch (error) {
    if (
      exportSequence !== pngRenderSequence
      || activePngRenderSequence !== exportSequence
      || sourceFileSequence !== fileRequestSequence
      || currentResult !== sourceResult
      || playerNameInput.value !== sourcePlayerName
    ) return;
    exportStatus.textContent = error instanceof Error ? error.message : "图片没能生成，过一会儿再试一次。";
  } finally {
    if (activePngRenderSequence === exportSequence) {
      activePngRenderSequence = null;
      pngButton.disabled = resultsSection.hasAttribute("hidden") || currentResult?.entries.length === 0 || currentResult === null;
    }
  }
}

function exportJson(): void {
  if (currentResult === null) return;
  const model = currentRenderModel();
  if (model === null) return;
  const exportData = {
    schemaVersion: 4,
    catalogVersion: catalog.catalogVersion,
    ratingRule: "integer-rating-v1: best performance per chart from actual play history; resultClear>=DiveCleared receives +0.2; overall=(Best 50 total+Best 10 total)/60; fixed 50/30/10 averages.",
    ...currentResult,
    presentation: {
      ...(model.playerName === undefined ? {} : { playerName: model.playerName }),
      summary: {
        b50AverageRating: model.averageRating,
        b50TotalRating: model.totalRating,
        b30AverageRating: model.b30AverageRating,
        b30TotalRating: model.b30TotalRating,
        b10AverageRating: model.b10AverageRating,
        b10TotalRating: model.b10TotalRating,
        overallPotential: model.overallPotential,
      },
      entries: model.entries.map((entry) => ({ ...entry, score: entry.score.toString() })),
    },
  };
  const json = JSON.stringify(exportData, (_key, value: unknown) =>
    typeof value === "bigint" ? value.toString() : value, 2);
  downloadBlob(new Blob([json], { type: "application/json" }), "in-falsus-b50.json");
  exportStatus.textContent = "JSON 已生成，里面有成绩明细和 B50 的排序数据。";
}

fileInput.addEventListener("change", () => {
  void processFile(fileInput.files?.[0]);
});
browseButton.addEventListener("click", () => fileInput.click());
dropZone.addEventListener("dragover", (event) => {
  event.preventDefault();
  dropZone.classList.add("drop-zone-active");
});
dropZone.addEventListener("dragleave", (event) => {
  if (!(event.relatedTarget instanceof Node) || !dropZone.contains(event.relatedTarget)) {
    dropZone.classList.remove("drop-zone-active");
  }
});
dropZone.addEventListener("drop", (event: DragEvent) => {
  event.preventDefault();
  dropZone.classList.remove("drop-zone-active");
  void processFile(event.dataTransfer?.files[0]);
});
pngButton.addEventListener("click", () => void exportPng());
jsonButton.addEventListener("click", exportJson);
copyPathButton.addEventListener("click", () => void copyPath(
  copyPathButton,
  savePathValue.textContent ?? "",
  "复制文件路径",
));
copyDirectoryButton.addEventListener("click", () => void copyPath(
  copyDirectoryButton,
  saveDirectoryValue.textContent ?? "",
  "复制目录路径",
));
playerNameInput.addEventListener("input", () => {
  if (activePngRenderSequence !== null) {
    pngRenderSequence += 1;
    activePngRenderSequence = null;
    exportStatus.textContent = "名字改了，需要重新生成图片。";
    pngButton.disabled = resultsSection.hasAttribute("hidden") || currentResult?.entries.length === 0 || currentResult === null;
  } else if (exportStatus.textContent.startsWith("PNG 已")) {
    exportStatus.textContent = "名字已更新；再次点击“下载 PNG”会使用新名字。";
  }
});
