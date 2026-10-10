export type CatalogErrorCode =
  | "unsupported-schema"
  | "invalid-catalog"
  | "duplicate-identity"
  | "duplicate-chart-id";

export class CatalogError extends Error {
  constructor(
    readonly code: CatalogErrorCode,
    message: string,
  ) {
    super(message);
    this.name = "CatalogError";
  }
}
