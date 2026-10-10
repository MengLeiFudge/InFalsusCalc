import type { B50RenderEntry } from "./types";

export type JacketImageLoader = (url: string) => Promise<HTMLImageElement | null>;

const decodedJackets = new Map<string, Promise<HTMLImageElement | null>>();

function loadSameOriginImage(url: string): Promise<HTMLImageElement | null> {
  return new Promise((resolve) => {
    const image = new Image();
    image.decoding = "async";
    image.onload = () => {
      void image.decode().then(() => resolve(image), () => resolve(null));
    };
    image.onerror = () => resolve(null);
    image.src = url;
  });
}

export function jacketAssetUrl(thumbnail: string, baseUrl: string): string {
  const base = baseUrl.endsWith("/") ? baseUrl : baseUrl + "/";
  return base + thumbnail;
}

export async function preloadJackets(
  entries: readonly B50RenderEntry[],
  baseUrl: string,
  imageLoader: JacketImageLoader = loadSameOriginImage,
): Promise<ReadonlyMap<string, HTMLImageElement | null>> {
  const paths = [...new Set(entries.flatMap((entry) => entry.jacket ? [entry.jacket.thumbnail] : []))];
  const cacheDefaultLoader = imageLoader === loadSameOriginImage;
  const loaded = await Promise.all(paths.map(async (thumbnail) => {
    const url = jacketAssetUrl(thumbnail, baseUrl);
    let pending = cacheDefaultLoader ? decodedJackets.get(url) : undefined;
    if (pending === undefined) {
      pending = Promise.resolve(imageLoader(url)).catch(() => null);
      if (cacheDefaultLoader) decodedJackets.set(url, pending);
    }
    return [thumbnail, await pending] as const;
  }));
  return new Map(loaded);
}
