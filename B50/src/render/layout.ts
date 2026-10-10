export interface B50CardPosition {
  x: number;
  y: number;
}

export interface B50Layout {
  width: number;
  height: number;
  columns: number;
  rows: number;
  padding: number;
  headerHeight: number;
  cardWidth: number;
  cardHeight: number;
  gapX: number;
  gapY: number;
  cardPosition(index: number): B50CardPosition;
}

export const B50_EXPORT_WIDTH = 1800;
const COLUMN_COUNT = 5;
const PADDING = 48;
const HEADER_HEIGHT = 300;
const CARD_HEIGHT = 216;
const GAP_X = 18;
const GAP_Y = 18;
const BOTTOM_PADDING = 48;

export function calculateB50Layout(entryCount: number): B50Layout {
  if (!Number.isInteger(entryCount) || entryCount < 0 || entryCount > 50) {
    throw new Error("B50 layout entry count must be an integer from 0 through 50.");
  }

  const rows = Math.ceil(entryCount / COLUMN_COUNT);
  const cardWidth = Math.floor((B50_EXPORT_WIDTH - 2 * PADDING - (COLUMN_COUNT - 1) * GAP_X) / COLUMN_COUNT);
  const cardAreaHeight = rows === 0 ? 0 : rows * CARD_HEIGHT + (rows - 1) * GAP_Y;
  const height = HEADER_HEIGHT + cardAreaHeight + BOTTOM_PADDING;

  return {
    width: B50_EXPORT_WIDTH,
    height,
    columns: COLUMN_COUNT,
    rows,
    padding: PADDING,
    headerHeight: HEADER_HEIGHT,
    cardWidth,
    cardHeight: CARD_HEIGHT,
    gapX: GAP_X,
    gapY: GAP_Y,
    cardPosition(index) {
      if (!Number.isInteger(index) || index < 0 || index >= entryCount) {
        throw new Error("Card index is outside the B50 layout.");
      }
      const column = index % COLUMN_COUNT;
      const row = Math.floor(index / COLUMN_COUNT);
      return {
        x: PADDING + column * (cardWidth + GAP_X),
        y: HEADER_HEIGHT + row * (CARD_HEIGHT + GAP_Y),
      };
    },
  };
}
