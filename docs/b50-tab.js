/** 首次进入 Best 50 时载入同源页面；独立文档保留原版样式，切换 Tab 保留已导入的成绩。 */
export function loadB50() {
  const frame = document.getElementById("b50-frame");
  if (frame.hasAttribute("src")) return;
  const status = document.getElementById("b50-loading");
  let observer = null;

  frame.addEventListener("load", () => {
    observer?.disconnect();
    const shell = frame.contentDocument?.querySelector(".site-shell");
    if (!shell) {
      status.hidden = false;
      status.textContent = "Best 50 页面加载失败，请刷新重试，或点击“独立打开”。";
      return;
    }

    // 原页面的 body 至少为视口高度；只观察内容容器，避免 iframe 高度参与自己的测量。
    observer = new ResizeObserver(() => {
      const height = Math.ceil(shell.getBoundingClientRect().height);
      if (height > 0) frame.style.height = `${height}px`;
    });
    observer.observe(shell);
    status.hidden = true;
  });

  frame.src = new URL("./b50/index.html", import.meta.url).href;
}
