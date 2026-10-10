import { formatPotential } from "../rating/rating";
import { preloadJackets, type JacketImageLoader } from "./assets";
import { renderB50Card } from "./card-renderer";
import { calculateB50Layout } from "./layout";
import type { B50RenderModel } from "./types";

const PALETTE = {
  background: "#111015",
  surface: "#19161d",
  card: "#1d1a20",
  edge: "#342d35",
  text: "#f6f1f3",
  secondary: "#c4bac4",
  muted: "#938994",
  accent: "#f36d70",
};

export interface PngRenderResult {
  blob: Blob;
  width: number;
  height: number;
}

export interface PngRenderOptions {
  baseUrl?: string;
  createCanvas?: () => HTMLCanvasElement;
  loadImage?: JacketImageLoader;
}

function roundedRect(
  context: CanvasRenderingContext2D,
  x: number,
  y: number,
  width: number,
  height: number,
  radius: number,
): void {
  const r = Math.min(radius, width / 2, height / 2);
  context.beginPath();
  context.moveTo(x + r, y);
  context.lineTo(x + width - r, y);
  context.quadraticCurveTo(x + width, y, x + width, y + r);
  context.lineTo(x + width, y + height - r);
  context.quadraticCurveTo(x + width, y + height, x + width - r, y + height);
  context.lineTo(x + r, y + height);
  context.quadraticCurveTo(x, y + height, x, y + height - r);
  context.lineTo(x, y + r);
  context.quadraticCurveTo(x, y, x + r, y);
  context.closePath();
}

function drawHeader(context: CanvasRenderingContext2D, model: B50RenderModel, width: number): void {
  context.fillStyle = PALETTE.surface;
  context.fillRect(0, 0, width, 300);
  context.fillStyle = PALETTE.accent;
  context.fillRect(0, 0, width, 4);
  context.fillRect(48, 46, 3, 112);

  context.textAlign = "left";
  context.textBaseline = "middle";
  context.font = "750 18px system-ui, sans-serif";
  context.fillStyle = PALETTE.accent;
  context.fillText("IN FALSUS", 66, 58);

  context.font = "800 78px system-ui, sans-serif";
  context.fillStyle = PALETTE.text;
  context.fillText("BEST 50", 62, 132);
  if (model.playerName) {
    const titleWidth = context.measureText("BEST 50").width;
    const badgeX = 62 + titleWidth + 28;
    const badgeMaxWidth = width - badgeX - 48;
    context.font = "750 28px system-ui, sans-serif";
    const badgeWidth = Math.min(badgeMaxWidth, context.measureText(model.playerName).width + 48);
    if (badgeWidth > 72) {
      roundedRect(context, badgeX, 105, badgeWidth, 54, 10);
      context.fillStyle = "#2b2027";
      context.fill();
      context.strokeStyle = PALETTE.accent;
      context.lineWidth = 1.5;
      context.stroke();
      context.fillStyle = PALETTE.accent;
      context.fillRect(badgeX + 14, 117, 3, 30);
      context.textAlign = "left";
      context.font = "750 28px system-ui, sans-serif";
      context.fillStyle = PALETTE.text;
      context.fillText(model.playerName, badgeX + 28, 132, badgeWidth - 42);
    }
  }

  const labels = ["B50 AVERAGE POTENTIAL", "B30 AVERAGE POTENTIAL", "B10 AVERAGE POTENTIAL", "OVERALL POTENTIAL"];
  const values = [
    formatPotential(model.averageRating),
    formatPotential(model.b30AverageRating),
    formatPotential(model.b10AverageRating),
    formatPotential(model.overallPotential),
  ];
  const left = 48;
  const gap = 12;
  const cellWidth = (width - 2 * left - 3 * gap) / 4;
  const y = 211;
  for (let index = 0; index < labels.length; index += 1) {
    const x = left + index * (cellWidth + gap);
    roundedRect(context, x, y, cellWidth, 58, 9);
    context.fillStyle = PALETTE.card;
    context.fill();
    context.strokeStyle = PALETTE.edge;
    context.lineWidth = 1;
    context.stroke();
    context.textAlign = "left";
    context.font = "650 10px system-ui, sans-serif";
    context.fillStyle = PALETTE.muted;
    context.fillText(labels[index] ?? "", x + 14, y + 17);
    context.font = "750 23px system-ui, sans-serif";
    context.fillStyle = index === 3 ? PALETTE.accent : PALETTE.text;
    context.fillText(values[index] ?? "", x + 14, y + 42, cellWidth - 28);
  }
}

function canvasToBlob(canvas: HTMLCanvasElement): Promise<Blob> {
  return new Promise((resolve, reject) => {
    canvas.toBlob((blob) => {
      if (blob === null) {
        reject(new Error("浏览器没能把这张图编码成 PNG。"));
        return;
      }
      resolve(blob);
    }, "image/png");
  });
}

export async function renderB50Png(
  model: B50RenderModel,
  options: PngRenderOptions = {},
): Promise<PngRenderResult> {
  if (model.entries.length === 0) throw new Error("还没有可以导出的成绩，先读一个存档。");
  const layout = calculateB50Layout(model.entries.length);
  const canvas = options.createCanvas?.() ?? document.createElement("canvas");
  canvas.width = layout.width;
  canvas.height = layout.height;
  const context = canvas.getContext("2d");
  if (context === null) throw new Error("这个浏览器给不了 2D 画布，没法生成图片。");

  context.fillStyle = PALETTE.background;
  context.fillRect(0, 0, layout.width, layout.height);
  drawHeader(context, model, layout.width);
  const images = await preloadJackets(model.entries, options.baseUrl ?? "/", options.loadImage);
  model.entries.forEach((entry, index) => {
    const position = layout.cardPosition(index);
    const image = entry.jacket ? images.get(entry.jacket.thumbnail) ?? null : null;
    renderB50Card(context, entry, image, position.x, position.y, layout.cardWidth, layout.cardHeight);
  });

  const blob = await canvasToBlob(canvas);
  if (blob.type !== "image/png" || blob.size < 8) {
    throw new Error("生成的图片是空的，请再生成一次。");
  }
  return { blob, width: layout.width, height: layout.height };
}
