export interface TextFitOptions {
  maxFontSize: number;
  minFontSize: number;
  maxLines: number;
}

export interface TextFitResult {
  lines: string[];
  fontSize: number;
  truncated: boolean;
}

type MeasureText = (text: string, fontSize: number) => number;

function splitGraphemes(value: string): string[] {
  const segmenter = new Intl.Segmenter("en", { granularity: "grapheme" });
  return Array.from(segmenter.segment(value), (segment) => segment.segment);
}

function wrapLongWord(word: string, width: number, fontSize: number, measureText: MeasureText): string[] {
  const lines: string[] = [];
  let line = "";
  for (const grapheme of splitGraphemes(word)) {
    const candidate = line + grapheme;
    if (line.length > 0 && measureText(candidate, fontSize) > width) {
      lines.push(line);
      line = grapheme;
    } else {
      line = candidate;
    }
  }
  if (line.length > 0) lines.push(line);
  return lines;
}

function wrapText(value: string, width: number, fontSize: number, measureText: MeasureText): string[] {
  const words = value.split(/\s+/u).filter(Boolean);
  const lines: string[] = [];
  let line = "";
  for (const word of words) {
    const candidate = line.length === 0 ? word : line + " " + word;
    if (measureText(candidate, fontSize) <= width) {
      line = candidate;
      continue;
    }
    if (line.length > 0) lines.push(line);
    if (measureText(word, fontSize) <= width) {
      line = word;
      continue;
    }
    const parts = wrapLongWord(word, width, fontSize, measureText);
    if (parts.length > 1) lines.push(...parts.slice(0, -1));
    line = parts.at(-1) ?? "";
  }
  if (line.length > 0) lines.push(line);
  return lines;
}

function ellipsize(value: string, width: number, fontSize: number, measureText: MeasureText): string {
  const graphemes = splitGraphemes(value);
  while (graphemes.length > 0) {
    const candidate = graphemes.join("") + "…";
    if (measureText(candidate, fontSize) <= width) return candidate;
    graphemes.pop();
  }
  return measureText("…", fontSize) <= width ? "…" : "";
}

export function fitText(
  value: string,
  width: number,
  measureText: MeasureText,
  options: TextFitOptions,
): TextFitResult {
  if (!Number.isFinite(width) || width <= 0) throw new Error("Text width must be positive.");
  const text = value.trim().replace(/\s+/gu, " ");
  if (text.length === 0) return { lines: [""], fontSize: options.minFontSize, truncated: false };
  for (let fontSize = options.maxFontSize; fontSize >= options.minFontSize; fontSize -= 1) {
    const lines = wrapText(text, width, fontSize, measureText);
    if (lines.length <= options.maxLines) return { lines, fontSize, truncated: false };
  }

  const fontSize = options.minFontSize;
  const lines = wrapText(text, width, fontSize, measureText);
  const fittedLines = lines.slice(0, options.maxLines);
  if (fittedLines.length === 0) return { lines: [""], fontSize, truncated: true };
  fittedLines[fittedLines.length - 1] = ellipsize(fittedLines.at(-1) ?? "", width, fontSize, measureText);
  return { lines: fittedLines, fontSize, truncated: true };
}
