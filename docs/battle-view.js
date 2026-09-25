import { roundEven } from "./battle.js";

const format = (value, digits = 0) => Number(value).toLocaleString("zh-CN", { maximumFractionDigits: digits });
const percent = (value) => `${format(value, 2)}%`;

/** 渲染五阶段战斗过程；图仅在阶段头尾提供节点交互，所有值来自同一次本地结算。
 * 双方HP分别使用左右纵轴，避免敌方超额伤害压平己方曲线；横轴为判定进度。
 * renderTrait输出受信任的技能图标、名称和说明HTML，由页面复用已有资源与转义规则。
 */
export function renderBattleProcess(root, battle, renderTrait) {
  const phases = battle.phases;
  let anchor = null;
  const width = 760,
    height = 350;
  const x = (at) => 64 + (at / battle.notes) * 628;
  const allPoints = phases.flatMap((p) => [
    { at: p.start.at, hp: p.start.before.hp },
    ...p.start.events.map((e) => ({ at: e.at, hp: e.after })),
    ...p.curve,
    ...p.end.events.map((e) => ({ at: e.at, hp: e.after })),
    { at: p.end.at, hp: p.end.after.hp }
  ]);
  const lows = [0, 1].map((side) => Math.min(0, ...allPoints.map((p) => p.hp[side])));
  const y = (hp, side) => 28 + ((100 - hp) / (100 - lows[side])) * 274;
  const lines = [0, 1]
    .map(
      (side) =>
        `<polyline class="battle-line side-${side}" points="${allPoints.map((p) => `${x(p.at)},${y(p.hp[side], side)}`).join(" ")}"/>`
    )
    .join("");
  const grid = Array.from({ length: 5 }, (_, i) => {
    const ratio = i / 4,
      top = 28 + ratio * 274;
    return `<line class="battle-gridline" x1="64" x2="692" y1="${top}" y2="${top}"/><text class="axis side-0" x="56" y="${top + 4}" text-anchor="end">${format(100 - ratio * (100 - lows[0]))}</text><text class="axis side-1" x="700" y="${top + 4}">${format(100 - ratio * (100 - lows[1]))}</text>`;
  }).join("");
  const bands = phases
    .map(
      (p, i) =>
        `<rect class="phase-band" data-phase="${i}" x="${x(p.start.at)}" y="24" width="${x(p.end.at) - x(p.start.at)}" height="284"/><text class="phase-caption" x="${(x(p.start.at) + x(p.end.at)) / 2}" y="330" text-anchor="middle">阶段${i + 1}</text>`
    )
    .join("");
  const nodes = phases
    .flatMap((p, i) =>
      ["start", "end"].flatMap((edge) =>
        [0, 1].map((side) => {
          const node = p[edge];
          return `<circle class="battle-node side-${side}" data-boundary="${i}:${edge}" cx="${x(node.at) + (edge === "start" ? 5 : -5)}" cy="${y(node.after.hp[side], side) + (side === 0 ? -3 : 3)}" r="5"/>`;
        })
      )
    )
    .join("");
  root.innerHTML = `<div class="battle-chart-column"><div class="battle-legend"><span class="side-0">己方HP · 左轴</span><span class="side-1">对方HP · 右轴</span></div><div class="battle-chart-scroll"><svg class="battle-chart" viewBox="0 0 ${width} ${height}" role="img" aria-label="双方HP随五阶段判定进度的变化，阶段头尾详情见下方节点按钮">${bands}${grid}${lines}${nodes}</svg></div><div class="phase-controls">${phases.map((_, i) => `<div><button type="button" data-phase="${i}" class="phase-select">阶段${i + 1}</button><div class="phase-endpoints"><button type="button" data-boundary="${i}:start" aria-label="阶段${i + 1}开始节点">开始</button><button type="button" data-boundary="${i}:end" aria-label="阶段${i + 1}结束节点">结束</button></div></div>`).join("")}</div><p class="help">横轴：判定进度 0–${battle.notes}。悬停或点击阶段头尾节点查看技能变化；双方纵轴独立，负HP表示超额伤害。</p></div><aside class="battle-stage-panel" aria-label="阶段数值面板"><div class="battle-stage-summary">${phases.map((p, i) => `<button type="button" data-phase="${i}"><span>阶段${i + 1}</span><strong>${format(roundEven(p.scores.score))}</strong></button>`).join("")}</div><div class="battle-stage-detail"></div></aside><div class="battle-node-tooltip" role="tooltip" popover="manual"></div>`;
  const tooltip = root.querySelector(".battle-node-tooltip");

  /** 阶段末分数是截至该阶段的累计成绩；差值可为负，不是把各阶段得分相加。 */
  function selectPhase(index) {
    const p = phases[index],
      previous = phases[index - 1];
    root.querySelectorAll("[data-phase]").forEach((element) => {
      const active = Number(element.getAttribute("data-phase")) === index;
      element.classList.toggle("selected", active);
      if (element.tagName === "BUTTON") element.setAttribute("aria-pressed", String(active));
    });
    const scores = [
      ["进攻分", "offensive_score"],
      ["防守分", "defensive_score"],
      ["遭遇总分", "score"]
    ]
      .map(([label, key]) => {
        const value = roundEven(p.scores[key]);
        const delta = previous ? value - roundEven(previous.scores[key]) : null;
        return `<div><span>${label}</span><strong>${format(value)}</strong><small>${delta === null ? "首阶段" : `较上阶段 ${delta > 0 ? "+" : ""}${format(delta)}`}</small></div>`;
      })
      .join("");
    root.querySelector(".battle-stage-detail").innerHTML =
      `<h3>阶段${index + 1}${index === 4 ? " · 最终结算" : ""}</h3><p class="help">判定 ${p.start.at + 1}–${p.end.at} · 普通 ${p.charge} · 暴击 ${p.notes - p.charge}</p><table class="stage-stats"><thead><tr><th></th><th>己方</th><th>对方</th></tr></thead><tbody><tr><th>攻击</th><td>${format(p.attack[0])}</td><td>${format(p.attack[1])}</td></tr><tr><th>防御</th><td>${format(p.defense[0])}</td><td>${format(p.defense[1])}</td></tr><tr><th>结束HP</th><td>${percent(p.after[0])}</td><td>${percent(p.after[1])}</td></tr></tbody></table><p class="help">攻防为阶段开始技能结算后数值；HP包含阶段结束技能。</p><div class="stage-damage"><span>本阶段造成伤害 <strong>${percent(p.damage[1])}</strong></span><span>本阶段受到伤害 <strong>${percent(p.damage[0])}</strong></span><span>${battle.mode === "reflection" ? "映像净推进" : "对方击破进度"} <strong>${percent(battle.mode === "reflection" ? p.after[0] - p.after[1] : 100 - p.after[1])}</strong></span></div><div class="stage-scores">${scores}</div><p class="help">截至阶段${index + 1}结束的累计分数。</p>`;
  }

  /** 关闭节点浮窗；离开、切换阶段及按Escape时释放入口关联。 */
  function hideNode() {
    anchor?.removeAttribute("aria-describedby");
    anchor = null;
    if (tooltip.matches(":popover-open")) tooltip.hidePopover();
  }

  /** 每个节点给出技能结算前后HP和攻防，事件列表保留实际执行次序。 */
  function showNode(target) {
    if (anchor === target) return;
    hideNode();
    const [index, edge] = target.dataset.boundary.split(":");
    const node = phases[Number(index)][edge];
    const values = [0, 1]
      .map(
        (side) =>
          `<tr><th>${side === 0 ? "己方" : "对方"}</th><td>${percent(node.before.hp[side])} → ${percent(node.after.hp[side])}</td><td>${format(node.before.attack[side])} → ${format(node.after.attack[side])}</td><td>${format(node.before.defense[side])} → ${format(node.after.defense[side])}</td></tr>`
      )
      .join("");
    tooltip.innerHTML = `<strong>阶段${Number(index) + 1} · ${edge === "start" ? "开始" : "结束"} · 判定${node.at}/${battle.notes}</strong><table><thead><tr><th></th><th>HP</th><th>攻击</th><th>防御</th></tr></thead><tbody>${values}</tbody></table><div class="node-effects">${node.events.length ? node.events.map((e) => `<div class="node-effect">${renderTrait(e.trait)}<small>${e.side === 0 ? "己方" : "对方"}卡牌${e.owner + 1} · ${e.kind === "modifier" ? (e.active ? "效果开始" : "效果结束") : `${e.kind === "heal" ? "有效回复" : "造成伤害"} ${percent(e.amount)}`}</small></div>`).join("") : '<p class="help">此节点没有技能触发。</p>'}</div>`;
    tooltip.id = "battle-node-tooltip";
    anchor = target;
    anchor.setAttribute("aria-describedby", tooltip.id);
    tooltip.showPopover();
    const rect = target.getBoundingClientRect(),
      box = tooltip.getBoundingClientRect();
    tooltip.style.left = `${Math.max(8, Math.min(rect.left, innerWidth - box.width - 8))}px`;
    const top = rect.top - box.height - 10;
    tooltip.style.top = `${Math.max(8, Math.min(top >= 8 ? top : rect.bottom + 10, innerHeight - box.height - 8))}px`;
  }

  root.onclick = (event) => {
    const node = event.target.closest("[data-boundary]");
    if (node) {
      selectPhase(Number(node.dataset.boundary.split(":")[0]));
      showNode(node);
      return;
    }
    const phase = event.target.closest("[data-phase]");
    if (phase) {
      hideNode();
      selectPhase(Number(phase.dataset.phase));
    }
  };
  root.onpointerover = (event) => {
    const node = event.target.closest("[data-boundary]");
    if (node) showNode(node);
  };
  root.onpointerout = (event) => {
    if (
      anchor &&
      !anchor.contains(event.relatedTarget) &&
      !tooltip.contains(event.relatedTarget) &&
      !tooltip.contains(event.target) &&
      !anchor.contains(document.activeElement)
    )
      hideNode();
  };
  root.onfocusin = (event) => {
    const node = event.target.closest("[data-boundary]");
    if (node) showNode(node);
    else hideNode();
  };
  root.onfocusout = (event) => {
    if (anchor && !anchor.contains(event.relatedTarget) && !tooltip.contains(event.relatedTarget)) hideNode();
  };
  root.onkeydown = (event) => {
    if (event.key === "Escape") hideNode();
  };
  // 自身滚动时隐藏，避免浮窗停留在已经离开的曲线节点。
  root.querySelector(".battle-chart-scroll").onscroll = hideNode;
  selectPhase(4);
}
