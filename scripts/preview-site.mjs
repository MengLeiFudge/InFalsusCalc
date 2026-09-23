import { spawn } from "node:child_process";
import { readFile, realpath } from "node:fs/promises";
import { createServer } from "node:http";
import { dirname, extname, isAbsolute, relative, resolve, sep } from "node:path";
import { fileURLToPath } from "node:url";

// 预览仅提供 docs 中的静态文件，绑定本机回环地址；终端关闭或 Ctrl+C 后停止。
const root = await realpath(resolve(dirname(fileURLToPath(import.meta.url)), "../docs"));
const types = {
  ".html": "text/html; charset=utf-8",
  ".js": "text/javascript; charset=utf-8",
  ".css": "text/css; charset=utf-8",
  ".json": "application/json; charset=utf-8",
  ".ico": "image/x-icon",
  ".webp": "image/webp",
  ".png": "image/png",
  ".svg": "image/svg+xml"
};
const server = createServer(async (request, response) => {
  if (request.method !== "GET" && request.method !== "HEAD") {
    response.writeHead(405, { Allow: "GET, HEAD" });
    response.end();
    return;
  }
  try {
    const pathname = decodeURIComponent(new URL(request.url, "http://127.0.0.1").pathname);
    const path = await realpath(resolve(root, `.${pathname === "/" ? "/index.html" : pathname}`));
    const local = relative(root, path);
    if (local === ".." || local.startsWith(`..${sep}`) || isAbsolute(local)) {
      response.writeHead(403);
      response.end();
      return;
    }
    const data = await readFile(path);
    response.writeHead(200, {
      "Content-Type": types[extname(path)] || "application/octet-stream",
      "Content-Length": data.length,
      "Cache-Control": "no-cache",
      "X-Content-Type-Options": "nosniff"
    });
    response.end(request.method === "HEAD" ? undefined : data);
  } catch (error) {
    const status = error instanceof URIError ? 400 : ["ENOENT", "ENOTDIR", "EISDIR"].includes(error.code) ? 404 : 500;
    if (status === 500) console.error("文件读取失败：", error.message);
    response.writeHead(status, { "Content-Type": "text/plain; charset=utf-8" });
    response.end(request.method === "HEAD" ? undefined : "无法读取此页面资源。");
  }
});
server.requestTimeout = 15000;
server.headersTimeout = 10000;

const deadline = setTimeout(() => {
  console.error("本地预览启动超过10秒，请关闭窗口后重试。");
  process.exit(1);
}, 10000);
server.on("error", (error) => {
  clearTimeout(deadline);
  console.error("本地预览启动失败：", error.message);
  process.exitCode = 1;
});
server.listen(0, "127.0.0.1", () => {
  clearTimeout(deadline);
  const url = `http://127.0.0.1:${server.address().port}/`;
  console.log(`本地预览已就绪：${url}\n进程：${process.pid}\n保留此窗口；查看结束后按 Ctrl+C 或关闭窗口。`);
  if (process.argv.includes("--open") && process.platform === "win32") {
    const browser = spawn("rundll32.exe", ["url.dll,FileProtocolHandler", url], { stdio: "ignore", windowsHide: true });
    browser.on("error", () => console.error(`无法自动打开浏览器，请手动访问：${url}`));
    browser.unref();
  }
});

/** 给正在读取的资源一秒退出宽限，随后关闭连接，避免留下后台服务。 */
function stop() {
  server.close(() => process.exit(0));
  setTimeout(() => server.closeAllConnections(), 1000).unref();
}
process.on("SIGINT", stop);
process.on("SIGTERM", stop);
