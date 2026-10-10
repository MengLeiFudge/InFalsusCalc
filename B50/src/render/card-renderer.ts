import { formatPotential } from "../rating/rating";
import { getDifficultyTheme } from "./difficulty-theme";
import { fitText } from "./text-fit";
import type { B50RenderEntry } from "./types";

const CARD = {
  fill: "#1d1a20",
  edge: "#3a3139",
  text: "#f5f1f2",
  secondary: "#b7afb8",
  muted: "#817984",
  accent: "#f36d70",
  imageFill: "#302931",
};

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

function drawPlaceholder(
  context: CanvasRenderingContext2D,
  x: number,
  y: number,
  size: number,
  accent: string,
): void {
  context.save();
  roundedRect(context, x, y, size, size, 8);
  context.fillStyle = CARD.imageFill;
  context.fill();
  context.save();
  context.beginPath();
  context.rect(x + 1, y + 1, size - 2, size - 2);
  context.clip();
  context.strokeStyle = accent;
  context.globalAlpha = 0.44;
  context.lineWidth = 3;
  context.beginPath();
  context.moveTo(x - 6, y + size * 0.82);
  context.lineTo(x + size * 0.72, y - 5);
  context.stroke();
  context.beginPath();
  context.moveTo(x + size * 0.2, y + size + 4);
  context.lineTo(x + size + 4, y + size * 0.15);
  context.stroke();
  context.restore();
  context.fillStyle = CARD.text;
  context.globalAlpha = 0.9;
  context.font = "800 24px system-ui, sans-serif";
  context.textAlign = "center";
  context.textBaseline = "middle";
  context.fillText("IF", x + size / 2, y + size / 2);
  context.restore();
}

function formatConstant(value: number): string {
  return Number.isInteger(value) ? String(value) : value.toFixed(1);
}

function formatNumber(value: number | bigint): string {
  return new Intl.NumberFormat("en-US", { maximumFractionDigits: 0 }).format(value);
}

export function renderB50Card(
  context: CanvasRenderingContext2D,
  entry: B50RenderEntry,
  image: HTMLImageElement | null,
  x: number,
  y: number,
  width: number,
  height: number,
): void {
  const theme = getDifficultyTheme(entry.difficultyIndex);
  context.save();
  roundedRect(context, x, y, width, height, 12);
  context.fillStyle = CARD.fill;
  context.fill();
  context.strokeStyle = CARD.edge;
  context.lineWidth = 1;
  context.stroke();

  context.fillStyle = CARD.accent;
  context.fillRect(x + 1, y + 14, 3, 30);
  context.textAlign = "left";
  context.textBaseline = "middle";
  context.font = "750 17px system-ui, sans-serif";
  context.fillStyle = CARD.text;
  context.fillText("#" + String(entry.rank).padStart(2, "0"), x + 18, y + 29);

  context.font = "700 11px system-ui, sans-serif";
  const badgeWidth = Math.min(width - 110, Math.max(74, context.measureText(theme.label.toUpperCase()).width + 22));
  roundedRect(context, x + width - badgeWidth - 14, y + 15, badgeWidth, 27, 7);
  context.fillStyle = theme.wash;
  context.fill();
  context.strokeStyle = theme.accent;
  context.globalAlpha = 0.72;
  context.stroke();
  context.globalAlpha = 1;
  context.textAlign = "center";
  context.fillStyle = theme.accent;
  context.fillText(theme.label.toUpperCase(), x + width - badgeWidth / 2 - 14, y + 29);

  const artX = x + 18;
  const artY = y + 51;
  const artSize = 90;
  if (image !== null) {
    roundedRect(context, artX, artY, artSize, artSize, 8);
    context.save();
    context.clip();
    context.drawImage(image, artX, artY, artSize, artSize);
    context.restore();
    context.strokeStyle = "rgba(255,255,255,0.14)";
    context.lineWidth = 1;
    roundedRect(context, artX, artY, artSize, artSize, 8);
    context.stroke();
  } else {
    drawPlaceholder(context, artX, artY, artSize, theme.accent);
  }

  const textX = x + 122;
  const textWidth = width - (textX - x) - 16;
  const titleMeasure = (text: string, fontSize: number): number => {
    context.font = "700 " + fontSize + "px system-ui, sans-serif";
    return context.measureText(text).width;
  };
  const fittedTitle = fitText(entry.title, textWidth, titleMeasure, {
    maxFontSize: 20,
    minFontSize: 15,
    maxLines: 2,
  });
  context.font = "700 " + fittedTitle.fontSize + "px system-ui, sans-serif";
  context.fillStyle = CARD.text;
  context.textAlign = "left";
  context.textBaseline = "top";
  const titleTop = y + 56 + (fittedTitle.lines.length === 1 ? 11 : 0);
  fittedTitle.lines.forEach((line, index) => context.fillText(line, textX, titleTop + index * 24, textWidth));

  if (entry.artist) {
    context.font = "500 13px system-ui, sans-serif";
    context.fillStyle = CARD.secondary;
    context.textBaseline = "middle";
    context.fillText(entry.artist, textX, y + 117, textWidth);
  }

  context.font = "650 13px system-ui, sans-serif";
  context.fillStyle = theme.accent;
  context.textBaseline = "middle";
  context.fillText("CONST  " + formatConstant(entry.constant), textX, y + 140, textWidth);

  context.strokeStyle = CARD.edge;
  context.beginPath();
  context.moveTo(x + 18, y + 157);
  context.lineTo(x + width - 18, y + 157);
  context.stroke();

  context.textAlign = "left";
  context.textBaseline = "middle";
  context.font = "600 9px system-ui, sans-serif";
  context.fillStyle = CARD.muted;
  context.fillText("SCORE", x + 18, y + 173);
  context.textAlign = "right";
  context.fillText("RATING", x + width - 18, y + 173);

  context.font = "750 19px system-ui, sans-serif";
  context.fillStyle = CARD.text;
  context.textAlign = "left";
  context.fillText(formatNumber(entry.score), x + 18, y + 198);
  context.textAlign = "right";
  context.fillStyle = theme.accent;
  context.fillText(formatPotential(entry.rating), x + width - 18, y + 198);
  context.restore();
}
