import { readFileSync } from "node:fs";
import { defineConfig, type Plugin } from "vite";

// 曲目快照与曲绘随当前网页发布；构建仅更新程序和快照，不清空已归属的资源与说明。
const emitCatalogSnapshots: Plugin = {
  name: "infalsus-b50-catalog-snapshots",
  apply: "build",
  generateBundle() {
    for (const name of ["songlist.json", "generated-manifest.json"]) {
      this.emitFile({
        type: "asset",
        fileName: "catalog/" + name,
        source: readFileSync(new URL("./src/catalog/" + name, import.meta.url)),
      });
    }
  },
};

export default defineConfig({
  base: "./",
  publicDir: false,
  plugins: [emitCatalogSnapshots],
  build: {
    outDir: "../docs/b50",
    emptyOutDir: false,
  },
});
