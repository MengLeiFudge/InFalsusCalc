export type SaveParseErrorCode =
  | "file-too-large"
  | "file-too-small"
  | "unsupported-format"
  | "ambiguous-record-array";

export class SaveParseError extends Error {
  constructor(
    readonly code: SaveParseErrorCode,
    message: string,
  ) {
    super(message);
    this.name = "SaveParseError";
  }
}
