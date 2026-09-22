/* 成果页读取本站静态数据与图片，不发起求解或后台计算。 */
"use strict";
const $ = (id) => document.getElementById(id);
/** 同一路径共享请求与已加载数据；失败后移除缓存，允许用户重新选择重试。 */
const requests = new Map();
async function loadJson(path) {
  if (!requests.has(path)) {
    const pending = fetch(path)
      .then((response) => {
        if (!response.ok) throw new Error(`数据读取失败（${response.status}）`);
        return response.json();
      })
      .catch((error) => {
        requests.delete(path);
        throw error;
      });
    requests.set(path, pending);
  }
  return requests.get(path);
}

/** 首屏只读取卡牌摘要及文件索引，不加载拼法或任何回想战斗。 */
async function loadReport() {
  const notice = $("notice");
  notice.hidden = false;
  notice.textContent = "正在加载卡牌列表…";
  try {
    const report = await loadJson("./data/index.json");
    if (report.schema !== 1 || !report.catalog || !report.templates || !report.recipes)
      throw new Error("网页数据版本不兼容，请刷新页面。");
    notice.hidden = true;
    return report;
  } catch (error) {
    notice.textContent =
      location.protocol === "file:"
        ? "请通过 GitHub Pages 访问本页，本地双击无法读取独立数据文件。"
        : `无法加载卡牌列表，请稍后刷新。${error.message}`;
    throw error;
  }
}
const DATA = await loadReport();

/** 技能描述和粒子形状只在卡牌详情或回想页面需要时读取。 */
async function loadCommon() {
  Object.assign(DATA.catalog, await loadJson(`./data/${DATA.common}`));
}
const colors = { 0: "#94a3b0", 1: "#ed8585", 2: "#e5c65d", 3: "#83ce9c", 4: "#7caee9", 5: "#b499e8", 6: "#415362" };
const colorNames = { 0: "无色", 1: "红", 2: "黄", 3: "绿", 4: "蓝", 5: "紫" };
const goalNames = {
  power: "攻击最高（同攻取防御最高）",
  fortitude: "防御最高（同防取攻击最高）",
  total: "攻防之和最高"
};
function goalLabel(goal, card) {
  const parts = String(goal).split(":");
  const direction = parts.length > 1 ? parts[0] : "";
  const name = parts.length > 1 ? parts[1] : parts[0];
  if (direction === "inner") return `内向 · ${goalNames[name]} · 结构分${num(card.inner_structure)}`;
  if (direction === "outer") return `外向 · ${goalNames[name]} · 结构分${num(card.outer_structure)}`;
  return goalNames[name] || name;
}
const state = {
  recipeIds: new Set(),
  tiers: new Set(),
  colors: new Set(),
  includeChromatic: false,
  libraryResults: [],
  libraryPage: 0,
  result: null,
  layout: null,
  piece: 0,
  layoutScale: 1,
  layoutX: 0,
  layoutY: 0,
  layoutDrag: null,
  layoutRequest: 0,
  encounterRequest: 0,
  board: null,
  battle: null,
  details: null
};
const libraryCards = Object.values(DATA.templates);
const recipeOrder = new Map(DATA.catalog.recipes.map((recipe, index) => [recipe.id, index]));
const esc = (value) =>
  String(value ?? "").replace(
    /[&<>"']/g,
    (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[c]
  );
const num = (value, digits = 0) =>
  Number(value).toLocaleString("zh-CN", { minimumFractionDigits: digits, maximumFractionDigits: digits });
const plain = (text) => String(text || "").replace(/<[^>]*>/g, "");
const palette = (mask) =>
  Object.keys(colorNames)
    .map(Number)
    .filter((c) => mask & (1 << c));
const trait = (id) => DATA.catalog.traits.find((t) => t.id === id);
const traitName = (id) => trait(id)?.name || `特性${id}`;

/** 所有技能入口共用游戏原始触发外框、效果图形和阶级标记，并显示完整说明。 */
function traitTag(id, iconOnly = false) {
  const item = trait(id),
    icon = item && DATA.catalog.trait_icons[item.icon],
    border = DATA.catalog.trait_border;
  const frame = item && DATA.catalog.trait_frames[String(item.condition)],
    tierIcon = item && DATA.catalog.trait_tiers[String(item.tier)];
  const details = item?.description ? plain(item.description) : "没有可用的效果说明。";
  const picture = icon
    ? `<span class="trait-icon"><img class="trait-border" src="${border}" alt="">${frame ? `<img class="trait-frame" src="${frame}" alt="">` : ""}<img class="trait-effect" src="${icon}" alt="">${tierIcon ? `<img class="trait-tier" src="${tierIcon}" alt="">` : ""}</span>`
    : "";
  return `<span class="trait-tag${iconOnly ? " icon-only" : ""}" tabindex="0">${picture}<span class="trait-label">${esc(traitName(id))}</span><span class="trait-tooltip" role="tooltip"><strong>${esc(traitName(id))}</strong>${item?.tier ? `<small>技能等级 ${item.tier}</small>` : ""}<span>${esc(details)}</span></span></span>`;
}

/** 按游戏六边坐标绘制掉落粒子的固定形状缩略图。 */
function shapeIcon(id) {
  const shape = DATA.catalog.shapes.find((item) => item.id === id),
    xy = (q, r) => [1.5 * q, Math.sqrt(3) * (r + q / 2)];
  if (!shape) return "";
  const points = shape.cells.map(([q, r]) => xy(q, r));
  const minX = Math.min(...points.map((point) => point[0])) - 1.1,
    maxX = Math.max(...points.map((point) => point[0])) + 1.1;
  const minY = Math.min(...points.map((point) => point[1])) - 1.1,
    maxY = Math.max(...points.map((point) => point[1])) + 1.1;
  return `<svg viewBox="${minX} ${minY} ${maxX - minX} ${maxY - minY}" aria-hidden="true">${points.map(([x, y]) => `<polygon points="${hexPoints(x, y)}" fill="${colors[shape.color]}" stroke="#d7e4e9" stroke-width=".08"/>`).join("")}</svg>`;
}

/** 按同一效能范围和技能抽取次数归组，完整展示游戏掉落表。 */
function encounterDrops(encounter) {
  const loot = encounter.drops,
    groups = new Map();
  for (const drop of loot.items) {
    const key = `${drop.min_potency}:${drop.max_potency}:${drop.trait_rolls}`;
    if (!groups.has(key)) groups.set(key, { ...drop, drops: [] });
    groups.get(key).drops.push(drop);
  }
  const pool = loot.traits
    .map(
      (item) =>
        `<span class="loot-trait">${traitTag(item.id)}<small>权重 ${item.weight} · ${num((item.weight / loot.trait_total_weight) * 100, 2)}%</small></span>`
    )
    .join("");
  const noTrait = loot.no_trait_weight
    ? `<span class="loot-empty">无技能结果 · 权重 ${loot.no_trait_weight} · ${num((loot.no_trait_weight / loot.trait_total_weight) * 100, 2)}%</span>`
    : "";
  const content = [...groups.values()]
    .map(
      (group) =>
        `<div class="drop-group"><div class="drop-group-title"><strong>效能 ${group.min_potency}-${group.max_potency}</strong><span>${group.trait_rolls ? `抽取技能 ${group.trait_rolls} 次` : "不抽取技能"}</span></div><div class="drop-shapes">${group.drops.map((drop) => `<span class="drop-shape" title="粒子${drop.shape} · ${colorNames[drop.color]} · ${drop.tier}阶 · ${drop.size}格"><span class="shape-picture">${shapeIcon(drop.shape)}</span><span>${colorNames[drop.color]} ${drop.tier}阶 · ${drop.size}格<small>权重 ${drop.weight} · ${num((drop.weight / loot.total_weight) * 100, 2)}%</small></span></span>`).join("")}</div></div>`
    )
    .join("");
  $("encounter-drops").innerHTML =
    `<div class="encounter-chapter"><span class="badge">${esc(encounter.chapter)}</span><span>掉落表 ${loot.table} · ${loot.items.length} 项粒子配置 · 总权重 ${loot.total_weight}</span></div><section class="loot-pool"><h4>对应技能池</h4><p>每次技能抽取独立按总权重 ${loot.trait_total_weight} 结算。</p><div class="loot-traits">${pool}${noTrait}</div></section>${content}`;
}

/** 展示当前材料能提供的技能，并用游戏描述解释触发条件。 */
function traitList(card) {
  if (!card.slots) return '<p class="help">没有特性槽。</p>';
  return `<div class="trait-list">${card.available_traits.map(traitTag).join("")}</div><p class="help" style="margin-top:10px">最多装备${card.slots}项；具体组合还须由实际粒子同时提供，配队结果已处理该约束。</p>`;
}

/** 忽略大小写与空白后，名称包含关键词或按顺序出现全部关键词字符即视为匹配。 */
function fuzzyMatch(name, query) {
  const text = String(name).normalize("NFKC").toLocaleLowerCase("zh-CN").replace(/\s/g, "");
  const keyword = String(query).normalize("NFKC").toLocaleLowerCase("zh-CN").replace(/\s/g, "");
  if (!keyword || text.includes(keyword)) return true;
  let position = 0;
  for (const character of keyword) {
    position = text.indexOf(character, position);
    if (position < 0) return false;
    position++;
  }
  return true;
}

/** 第一层等级、最终颜色和变色开关共同限定可选基础卡。 */
function matchesPrimary(card, recipe) {
  if (state.tiers.size && !state.tiers.has(recipe.tier)) return false;
  if (!state.includeChromatic && card.colors.length > 1) return false;
  return !state.colors.size || card.colors.some((color) => state.colors.has(color));
}

/** 名称搜索只在第一层范围内收窄指定卡牌候选。 */
function updateRecipeChoices() {
  const recipeMap = new Map(DATA.catalog.recipes.map((recipe) => [recipe.id, recipe]));
  const eligible = new Set(
    libraryCards.filter((card) => matchesPrimary(card, recipeMap.get(card.recipe))).map((card) => card.recipe)
  );
  for (const id of [...state.recipeIds]) if (!eligible.has(id)) state.recipeIds.delete(id);
  const query = $("recipe-search").value,
    choices = DATA.catalog.recipes.filter((recipe) => eligible.has(recipe.id) && fuzzyMatch(recipe.name, query));
  $("selected-recipes").innerHTML = DATA.catalog.recipes
    .filter((recipe) => state.recipeIds.has(recipe.id))
    .map(
      (recipe) =>
        `<button type="button" class="multi-select-chip" data-remove-recipe="${recipe.id}" title="移除${esc(recipe.name)}"><span>${esc(recipe.name)}</span><b aria-hidden="true">&times;</b></button>`
    )
    .join("");
  $("recipe-options").innerHTML =
    choices
      .map((recipe) => {
        const selected = state.recipeIds.has(recipe.id);
        return `<button type="button" class="multi-select-option${selected ? " selected" : ""}" role="option" aria-selected="${selected}" data-recipe="${recipe.id}"><span>${esc(recipe.name)}<small>等级${recipe.tier}</small></span><b aria-hidden="true">&#10003;</b></button>`;
      })
      .join("") || '<p class="multi-select-empty">没有匹配的卡牌</p>';
}

/** 切换指定卡牌，不改变名称搜索内容。 */
function toggleRecipe(id) {
  if (state.recipeIds.has(id)) state.recipeIds.delete(id);
  else state.recipeIds.add(id);
  updateRecipeChoices();
  filterLibrary();
  $("recipe-search").focus();
}

/** 打开或关闭指定卡牌候选列表。 */
function setRecipeOptions(open) {
  $("recipe-options").hidden = !open;
  $("recipe-search").setAttribute("aria-expanded", String(open));
}

/** 对启用的数值比较条件执行统一判断。 */
function compareValue(value, operator, target) {
  if (!operator) return true;
  if (operator === "eq") return value === target;
  if (operator === "gt") return value > target;
  if (operator === "gte") return value >= target;
  if (operator === "lt") return value < target;
  return value <= target;
}

/** 等级和两类颜色共用常驻多选按钮组。 */
const enumFilters = { tiers: { options: "tier-options" }, colors: { options: "color-options" } };
function enumChoices(key) {
  return key === "tiers"
    ? [...new Set(DATA.catalog.recipes.map((recipe) => recipe.tier))].sort((a, b) => a - b)
    : Object.keys(colorNames)
        .map(Number)
        .filter((color) => color > 0);
}
function updateEnumFilter(key) {
  const selected = state[key];
  $(enumFilters[key].options).innerHTML = enumChoices(key)
    .map((value) => {
      const active = selected.has(value),
        content =
          key === "tiers"
            ? `<img src="${DATA.catalog.card_assets.levels[String(value)]}" alt="">`
            : `<img src="${DATA.catalog.card_assets.icons[`${value}-0`]}" alt="">`;
      return `<button type="button" class="filter-tile${active ? " selected" : ""}" aria-label="${key === "tiers" ? `等级${value}` : colorNames[value]}" aria-pressed="${active}" data-filter="${key}" data-value="${value}">${content}</button>`;
    })
    .join("");
}
function toggleEnumValue(key, value) {
  const selected = state[key];
  if (selected.has(value)) selected.delete(value);
  else selected.add(value);
  updateEnumFilter(key);
  updateRecipeChoices();
  filterLibrary();
}

/** 后级排序只有在前一级存在时才可用。 */
function updateSortControls() {
  for (let index = 1; index <= 3; index++) $("sort-direction-" + index).disabled = !$("sort-field-" + index).value;
  const thirdEnabled = Boolean($("sort-field-2").value);
  $("sort-field-3").disabled = !thirdEnabled;
  if (!thirdEnabled) $("sort-field-3").value = "";
  $("sort-direction-3").disabled = !thirdEnabled || !$("sort-field-3").value;
}

/** 按三个有序条件依次比较卡牌，模板编号提供稳定末级顺序。 */
function sortLibraryCards(cards, recipeMap) {
  const criteria = [];
  for (let index = 1; index <= 3; index++) {
    const name = $("sort-field-" + index).value;
    if (!name) break;
    criteria.push([name, $("sort-direction-" + index).value]);
  }
  const field = (card, name) =>
    name === "order"
      ? recipeOrder.get(card.recipe)
      : name === "range"
        ? card.left + card.right
        : name === "tier"
          ? recipeMap.get(card.recipe).tier
          : card[name];
  return cards.sort((a, b) => {
    for (const [name, direction] of criteria) {
      const av = field(a, name),
        bv = field(b, name),
        difference = typeof av === "string" ? av.localeCompare(bv, "zh-CN") : av - bv;
      if (difference) return direction === "desc" ? -difference : difference;
    }
    return a.id.localeCompare(b.id);
  });
}

/** 只创建当前页卡面，避免大结果集同步解码数千个图片节点。 */
function renderLibraryPage() {
  const size = Number($("library-page-size").value),
    total = state.libraryResults.length,
    pages = Math.max(1, Math.ceil(total / size));
  state.libraryPage = Math.min(state.libraryPage, pages - 1);
  const start = state.libraryPage * size,
    cards = state.libraryResults.slice(start, start + size);
  $("results").innerHTML = cards.length
    ? cards.map(libraryCard).join("")
    : '<p class="empty-results">没有符合全部条件的卡牌。</p>';
  $("library-pagination").hidden = !total;
  $("library-page-prev").disabled = state.libraryPage === 0;
  $("library-page-next").disabled = state.libraryPage >= pages - 1;
  $("library-page-status").textContent = total
    ? `${state.libraryPage + 1} / ${pages} · ${start + 1}-${start + cards.length}张，共${total}张`
    : "";
}

/** 所有启用条件以AND关系从统一最终模板库中筛选。 */
function filterLibrary(resetPage = true) {
  const numeric = [
    ["slots", (card) => card.slots],
    ["left", (card) => card.left],
    ["right", (card) => card.right],
    ["strikes", (card) => card.strikes]
  ];
  const recipeMap = new Map(DATA.catalog.recipes.map((recipe) => [recipe.id, recipe]));
  const cards = sortLibraryCards(
    libraryCards.filter((card) => {
      const recipe = recipeMap.get(card.recipe);
      if (state.recipeIds.size && !state.recipeIds.has(card.recipe)) return false;
      if (!matchesPrimary(card, recipe)) return false;
      return !numeric.some(
        ([id, value]) => !compareValue(value(card), $(id + "-op").value, Number($(id + "-value").value))
      );
    }),
    recipeMap
  );
  $("recipe-count").textContent = `${cards.length}张`;
  state.libraryResults = cards;
  if (resetPage) state.libraryPage = 0;
  renderLibraryPage();
}

/** 恢复全部卡牌范围并清除所有并列条件。 */
function clearLibraryFilters() {
  $("recipe-search").value = "";
  state.recipeIds.clear();
  state.tiers.clear();
  state.colors.clear();
  state.includeChromatic = false;
  $("include-chromatic").classList.remove("selected");
  $("include-chromatic").setAttribute("aria-pressed", "false");
  for (const id of ["slots", "left", "right", "strikes"]) {
    $(id + "-op").value = "";
    $(id + "-value").value = "0";
  }
  $("sort-field-1").value = "order";
  $("sort-direction-1").value = "asc";
  $("sort-field-2").value = "";
  $("sort-direction-2").value = "desc";
  $("sort-field-3").value = "";
  $("sort-direction-3").value = "desc";
  $("library-page-size").value = "20";
  updateSortControls();
  for (const key of Object.keys(enumFilters)) updateEnumFilter(key);
  updateRecipeChoices();
  setRecipeOptions(false);
  filterLibrary();
}

/** 用同一套游戏资源合成玩家卡面；技能数组为空时保留空槽。 */
function cardVisual(card, color, traits = [], interactive = false) {
  const recipe = DATA.catalog.recipes.find((item) => item.id === card.recipe),
    sr = recipe?.is_sr ? 1 : 0,
    slots = Math.max(1, Math.min(3, card.slots));
  const assets = DATA.catalog.card_assets,
    assetKey = `${color}-${sr}`,
    frame = assets.frames[`${assetKey}-${slots}`];
  const behavior = interactive
    ? ` role="button" tabindex="0" data-library-layout="${card.id}" aria-label="查看${esc(card.name)}详情与拼法"`
    : "";
  const slotsHtml = Array.from({ length: card.slots }, (_, index) =>
    traits[index]
      ? traitTag(traits[index], true)
      : `<span class="trait-empty"><img src="${DATA.catalog.trait_border}" alt="空技能槽"></span>`
  ).join("");
  const pip = (side, index, x, value) =>
    `<img class="range-pip ${side}" style="left:${x}%" src="${assets.common[index < value ? "medium-range-active" : "medium-range-inactive"]}" alt="">`;
  const leftPips = [22.55, 26.53, 30.5, 34.48].map((x, index) => pip("left", index, x, card.left)).join("");
  const rightPips = [54.36, 58.34, 62.31, 66.3].map((x, index) => pip("right", index, x, card.right)).join("");
  return `<article class="game-card-visual${interactive ? " interactive" : ""}" style="--card-color:${colors[color]}"${behavior}><img class="card-art" loading="lazy" decoding="async" src="${assets.art[card.recipe]}" alt="${esc(card.name)}立绘"><img class="card-frame" src="${frame}" alt=""><img class="card-inner-frame" src="${assets.common["art-frame"]}" alt=""><img class="card-top-connector" src="${assets.common["connector-top"]}" alt=""><img class="card-color-icon" src="${assets.icons[assetKey]}" alt="${colorNames[color]}色"><img class="card-level-icon" src="${assets.levels[String(recipe.tier)]}" alt="等级${recipe.tier}"><img class="card-tier-backing" src="${assets.tiers[assetKey]}" alt=""><img class="card-bottom-drawer" src="${assets.common["medium-bottom-drawer"]}" alt=""><img class="card-bottom-connector" src="${assets.common["connector-top"]}" alt=""><h4>${esc(card.name)}</h4><div class="card-stats"><span class="power"><img src="${assets.stat_icons[`${assetKey}-power`]}" alt="攻击"><strong>${num(card.power)}</strong></span><span class="fortitude"><img src="${assets.stat_icons[`${assetKey}-fortitude`]}" alt="防御"><strong>${num(card.fortitude)}</strong></span></div><div class="card-range">${leftPips}<img class="range-center" src="${assets.common["medium-range-center"]}" alt="">${rightPips}</div><div class="card-traits">${slotsHtml}</div></article>`;
}

/** 卡牌一览按当前颜色条件选取一种可用颜色，点击卡面打开详情。 */
function libraryCard(card) {
  const color = [...state.colors].sort().find((value) => card.colors.includes(value)) || card.colors[0];
  return `<div class="player-card library-card">${cardVisual(card, color, [], true)}</div>`;
}

/** 推荐配队复用卡牌一览的卡面，敌方卡保持紧凑信息布局。 */
function gameCard(card, slot, player = false) {
  const skills = card.traits.filter((id) => id > 1);
  if (!player)
    return `<article class="game-card enemy-card" style="--card-color:${colors[card.color]}"><div class="slot">第${slot + 1}槽 · ${colorNames[card.color]}</div><h4>${esc(card.name)}</h4><div class="stat"><small>卡牌攻击</small><span>${num(card.power)}</span></div><div class="stat"><small>卡牌防御</small><span>${num(card.fortitude)}</span></div><p class="range">左${card.left} · 右${card.right}</p><ul>${skills.length ? skills.map((id) => `<li>${traitTag(id)}</li>`).join("") : "<li>无特性</li>"}</ul></article>`;
  return `<div class="player-card"><div class="player-slot">第${slot + 1}槽 · ${colorNames[card.color]} · 罚分${card.strikes}</div>${cardVisual(card, card.color, skills)}<p class="card-meta">${card.slots ? `${card.slots}槽 · 左${card.left}右${card.right}` : "0槽 · 范围无效"} · 内${num(card.inner_structure)} / 外${num(card.outer_structure)}</p><button class="secondary" data-player-layout="${slot}">查看拼法</button></div>`;
}

/** 结算条显示真实数值，超过100%的击破进度仍保留数值，只将条形长度封顶。 */
function gauge(caption, value, left, right, enemy = false) {
  return `<div class="gauge ${enemy ? "enemy" : ""}"><div class="caption">${esc(caption)}</div><div class="percent">${num(value, 2)}%</div><div class="bar"><div class="fill" style="width:${Math.max(0, Math.min(100, value))}%"></div></div><div class="details"><span>${esc(left)}</span><span>${esc(right)}</span></div></div>`;
}

/** 进入回想页或切换条件时读取当前回想，旧请求完成后不得覆盖新选择。 */
async function selectEncounter() {
  const request = ++state.encounterRequest;
  const eid = Number($("encounter-select").value),
    maxStrikes = Number($("encounter-strikes").value),
    rating = Number($("encounter-rating").value) || DATA.defaults.search_rating;
  state.result = null;
  state.battle = null;
  state.details = null;
  $("encounter-content").hidden = true;
  $("encounter-loading").hidden = false;
  $("encounter-loading").textContent = "正在加载当前回想…";
  $("encounter-rating-value").textContent = rating;
  $("encounter-strikes-value").textContent = maxStrikes;
  try {
    const entry = DATA.catalog.encounters.find((e) => e.id === eid);
    const [payload] = await Promise.all([loadJson(`./data/${entry.file}`), loadCommon()]);
    if (request !== state.encounterRequest) return;
    const encounter = { ...entry, ...payload };
    const result = payload.decks[maxStrikes];
    const b = result?.ratings[rating];
    if (!b) throw new Error("这个回想缺少所选条件的预计算成果。");
    Object.assign(DATA.templates, payload.templates);
    state.result = result;
    state.battle = b;
    $("encounter-rating-value").value = rating;
    $("encounter-rating-value").textContent = rating;
    $("encounter-strikes-value").value = maxStrikes;
    $("encounter-strikes-value").textContent = maxStrikes;
    $("encounter-title").textContent =
      `${encounter.chapter} · ${encounter.name} · ${encounter.mode === "connect" ? "连接" : "映像"}`;
    encounterDrops(encounter);
    $("battle-condition").textContent =
      `允许罚分${maxStrikes} · 谱面等级${rating} · 配队搜索等级10 · 联觉开启 · 25个代表性全EXACT键 · 初始HP100`;
    $("battle-status").textContent = b.passed ? "可通关" : "此推荐尚未通关";
    $("battle-status").className = `badge ${b.passed ? "good" : "bad"}`;
    $("encounter-score").innerHTML =
      `<div><span>遭遇总分</span><strong>${num(b.score)}</strong></div><div><span>进攻分</span><strong>${num(b.offensive_score)}</strong></div><div><span>防守分</span><strong>${num(b.defensive_score)}</strong></div>`;
    if (encounter.mode === "connect") {
      $("settlement").innerHTML =
        gauge(
          "己方剩余HP",
          b.player_remaining,
          `全程最低 ${num(Math.max(0, b.minimum_hp), 2)}%`,
          b.broken[0] ? "曾破损" : "全程未破损"
        ) +
        gauge(
          "对方击破进度",
          b.enemy_progress,
          `剩余HP ${num(b.enemy_remaining, 2)}%`,
          b.broken[1] ? `已击破 · 超额 ${num(Math.max(0, b.enemy_progress - 100), 2)}%` : "尚未击破",
          true
        );
    } else {
      $("settlement").innerHTML =
        gauge("映像净推进", b.progress, "目标 100%", `超过目标 ${num(b.progress - 100, 2)}%`) +
        gauge(
          "对方净受伤",
          b.enemy_progress,
          `己方净受伤 ${num(100 - b.hp[0], 2)}%`,
          "净推进＝对方净受伤－己方净受伤",
          true
        );
    }
    $("player-deck").innerHTML = result.cards
      .map((c, i) => gameCard({ ...DATA.templates[c.template], color: c.color, traits: c.traits }, i, true))
      .join("");
    $("enemy-deck").innerHTML = encounter.cards.map((c, i) => gameCard(c, i)).join("");

    $("materials").innerHTML = result.cards
      .map(
        (c, i) =>
          `<div class="materials-group"><strong>第${i + 1}槽 · ${esc(DATA.templates[c.template].name)}</strong>${c.materials.length ? c.materials.map((m) => `<p>拼图第${m.piece}块：<span class="inline-traits">${m.traits.map(traitTag).join("")}</span><br>可刷来源：${m.sources.map((eid) => esc(DATA.catalog.encounters.find((e) => e.id === eid)?.name || `回想${eid}`)).join("／")}</p>`).join("") : "<p>无需携带特性的粒子。</p>"}</div>`
      )
      .join("");
    $("phase-table").querySelector("tbody").innerHTML = "";
    $("events").textContent = "";
    $("encounter-content").hidden = false;
    $("encounter-loading").hidden = true;
    loadBattleDetails();
  } catch (error) {
    if (request !== state.encounterRequest) return;
    state.result = null;
    state.battle = null;
    $("encounter-loading").textContent = `加载失败，请重新选择回想或切换页面重试。${error.message}`;
  }
}

/** 阶段或特性明细展开后才读取所选等级的详情，重复查看复用缓存。 */
async function loadBattleDetails() {
  if (
    !state.battle ||
    state.details ||
    $("encounters-tab").hidden ||
    (!$("phase-details").open && !$("event-details").open)
  )
    return;
  const request = state.encounterRequest;
  const battle = state.battle;
  const body = $("phase-table").querySelector("tbody");
  body.innerHTML = '<tr><td colspan="7">正在加载阶段明细…</td></tr>';
  $("events").textContent = "正在加载特性明细…";
  try {
    const details = await loadJson(`./data/${battle.details}`);
    if (request !== state.encounterRequest) return;
    state.details = details;
    body.innerHTML = details.phases
      .map(
        (p) =>
          `<tr><td>${p.phase}</td><td>${num(p.attack[0])}／${num(p.defense[0])}</td><td>${num(p.attack[1])}／${num(p.defense[1])}</td><td>${num(p.damage[1], 2)}%</td><td>${num(p.damage[0], 2)}%</td><td>${num(Math.max(0, p.after[0]), 2)}%</td><td>${num(100 - p.after[1], 2)}%</td></tr>`
      )
      .join("");
    $("events").innerHTML = details.events.length
      ? details.events
          .map(
            (e) =>
              `<div class="event-row"><span>代表键进度 ${e.at}/25 · ${e.side === 0 ? "己方" : "对方"}</span>${traitTag(e.trait)}<span>· ${e.kind === "heal" ? "有效回复" : "造成伤害"} ${num(e.amount, 4)}%</span></div>`
          )
          .join("")
      : '<p class="help">没有瞬时攻击或回复事件。</p>';
  } catch (error) {
    if (request !== state.encounterRequest) return;
    const message = `明细加载失败，收起后重新展开可重试。${error.message}`;
    body.innerHTML = `<tr><td colspan="7">${esc(message)}</td></tr>`;
    $("events").textContent = message;
  }
}

/** 点击卡牌后读取该基础卡的布局；关闭弹窗或选择另一张卡会使旧请求失效。 */
async function showLayout(identity, assignment = null, showGoals = true) {
  const request = ++state.layoutRequest;
  const summary = DATA.templates[identity];
  state.layout = null;
  state.board = null;
  $("layout-title").textContent = summary.name;
  $("layout-goals").textContent = "";
  $("layout-stats").textContent = "";
  for (const id of ["layout-card-details", "piece-list", "layout-traits"]) $(id).textContent = "";
  $("layout-board").textContent = "正在加载卡牌详情…";
  $("layout-flip").disabled = true;
  $("save-svg").disabled = true;
  if (!$("layout-dialog").open) $("layout-dialog").showModal();
  try {
    const [detail] = await Promise.all([loadJson(`./data/${DATA.recipes[summary.recipe]}`), loadCommon()]);
    if (request !== state.layoutRequest || !$("layout-dialog").open) return;
    if (!detail.cards[identity]) throw new Error("缺少这张卡的拼法数据。");
    const card = { ...summary, ...detail.cards[identity] };
    state.layout = card;
    state.board = detail.board;
    state.piece = 0;
    state.assignment = assignment;
    state.layoutScale = 1;
    state.layoutX = 0;
    state.layoutY = 0;
    state.layoutDrag = null;
    const goalText = showGoals ? (card.goals || []).map((g) => goalNames[g]).join(" · ") : "";
    $("layout-goals").textContent = goalText;
    $("layout-goals").hidden = !goalText;
    $("layout-title").textContent = card.name;
    $("layout-stats").textContent =
      `原始攻击 ${num(card.base_power)} · 原始防御 ${num(card.base_fortitude)} ｜ 卡牌攻击 ${num(card.power)} · 卡牌防御 ${num(card.fortitude)}`;
    const penaltyNames = ["数量超限", "越界", "重叠", "断连"],
      penaltyText =
        penaltyNames
          .map((name, index) => [name, card.penalties[index]])
          .filter(([, value]) => value > 0)
          .map(([name, value]) => `${name}${value}`)
          .join(" · ") || "无";
    $("layout-card-details").innerHTML =
      `<div><span>颜色</span><strong>${card.colors.map((color) => colorNames[color]).join("、")}</strong></div><div><span>范围与槽位</span><strong>左${card.left} · 右${card.right} · ${card.slots}槽</strong></div><div><span>总罚分</span><strong>${card.strikes} · 保留${num(card.retained_percent, card.strikes ? 2 : 0)}%</strong></div><div><span>实际罚分</span><strong>${penaltyText}</strong></div><div><span>粒子实际用量</span><strong>Ⅰ ${card.tier_counts[0]} · Ⅱ ${card.tier_counts[1]} · Ⅲ ${card.tier_counts[2]}</strong></div><div><span>粒子总数</span><strong>${card.count}块</strong></div>`;
    $("layout-flip").checked = false;
    $("layout-traits").innerHTML = traitList(card);
    drawLayout();
    $("layout-flip").disabled = false;
    $("save-svg").disabled = false;
  } catch (error) {
    if (request !== state.layoutRequest || !$("layout-dialog").open) return;
    $("layout-board").textContent = `加载失败，关闭后再次点击卡牌可重试。${error.message}`;
  }
}

/** 按游戏Q、R坐标绘制平顶六边格，翻转只改变阅读方向。 */
function hexPoints(x, y) {
  return Array.from(
    { length: 6 },
    (_, i) => `${x + Math.cos((i * Math.PI) / 3)},${y + Math.sin((i * Math.PI) / 3)}`
  ).join(" ");
}

/** 编号、形状小图和原始坐标共同描述拼法，重叠格保留全部粒子编号。 */
function drawLayout() {
  if (!state.layout) return;
  const card = state.layout,
    recipe = state.board,
    flip = $("layout-flip").checked ? 1 : -1;
  const xy = (q, r) => [1.5 * q, flip * Math.sqrt(3) * (r + q / 2)];
  const safe = new Set(recipe.safe.map(([q, r]) => `${q},${r}`));
  const occupied = new Map();
  card.placements.forEach((p, i) => {
    p.cells.forEach(([q, r]) => {
      const key = `${q},${r}`;
      if (!occupied.has(key)) occupied.set(key, []);
      occupied.get(key).push(i + 1);
    });
  });
  const relevant = [...recipe.safe, ...card.placements.flatMap((p) => p.cells)].map(([q, r]) => xy(q, r));
  const minX = Math.min(...relevant.map((p) => p[0])) - 2,
    maxX = Math.max(...relevant.map((p) => p[0])) + 2,
    minY = Math.min(...relevant.map((p) => p[1])) - 2,
    maxY = Math.max(...relevant.map((p) => p[1])) + 2;
  const targets = new Map(recipe.targets.map(([q, r, color]) => [`${q},${r}`, color]));
  const cells = [];
  for (const [q, r] of recipe.cells) {
    const key = `${q},${r}`,
      [x, y] = xy(q, r);
    if (x < minX - 1 || x > maxX + 1 || y < minY - 1 || y > maxY + 1) continue;
    const pieces = occupied.get(key) || [],
      target = targets.get(key),
      fill = pieces.length ? colors[card.placements[pieces[0] - 1].color] : target ? colors[target] : "#14202b";
    const dim = state.piece && !pieces.includes(state.piece),
      high = state.piece && pieces.includes(state.piece);
    cells.push(
      `<g class="layout-cell ${dim ? "dim" : ""} ${high ? "highlight" : ""}"><title>Q=${q}, R=${r}${pieces.length ? ` · 粒子${pieces.join("、")}` : ""}</title><polygon points="${hexPoints(x, y)}" fill="${fill}" fill-opacity="${pieces.length ? 0.86 : target ? 0.23 : 0.45}" stroke="${safe.has(key) ? "#657b8b" : "#2a3b49"}" stroke-width=".045" ${safe.has(key) ? "" : 'stroke-dasharray=".13 .1"'}/>${pieces.length ? `<text x="${x}" y="${y + 0.16}" text-anchor="middle" font-size="${pieces.length > 2 ? 0.34 : 0.48}" font-family="sans-serif" font-weight="600" fill="#10202a">${pieces.join("/")}</text>` : ""}</g>`
    );
  }
  $("layout-board").innerHTML =
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${minX} ${minY} ${maxX - minX} ${maxY - minY}"><rect x="${minX}" y="${minY}" width="${maxX - minX}" height="${maxY - minY}" fill="#0c151d"/>${cells.join("")}</svg>`;
  applyLayoutView();
  $("piece-list").innerHTML = card.placements
    .map((p, i) => {
      const shape = DATA.catalog.shapes.find((s) => s.id === p.id),
        points = shape.cells.map(([q, r]) => xy(q, r));
      const x0 = Math.min(...points.map((c) => c[0])) - 1.2,
        y0 = Math.min(...points.map((c) => c[1])) - 1.2,
        w = Math.max(...points.map((c) => c[0])) - x0 + 1.2,
        h = Math.max(...points.map((c) => c[1])) - y0 + 1.2;
      const material = state.assignment?.find((m) => m.piece === i + 1),
        skills = material ? ` · ${material.traits.map(traitName).join("、")}` : "";
      return `<button class="${state.piece === i + 1 ? "selected" : ""}" data-piece="${i + 1}" title="${shape.tier}阶 · Q=${p.q}, R=${p.r}${esc(skills)}">${i + 1}<svg viewBox="${x0} ${y0} ${w} ${h}">${points.map(([x, y]) => `<polygon points="${hexPoints(x, y)}" fill="${colors[p.color]}" stroke="#172933" stroke-width=".08"/>`).join("")}</svg><span>${p.q},${p.r}${material ? " · 带特性" : ""}</span></button>`;
    })
    .join("");
}

/** 将独立视图状态应用到拼法SVG，不改变导出的坐标和内容。 */
function applyLayoutView() {
  const svg = $("layout-board").querySelector("svg");
  if (svg) svg.style.transform = `translate(${state.layoutX}px, ${state.layoutY}px) scale(${state.layoutScale})`;
}

/** 鼠标滚轮以指针位置为中心缩放拼法画布。 */
function zoomLayout(event) {
  event.preventDefault();
  const board = $("layout-board"),
    bounds = board.getBoundingClientRect(),
    oldScale = state.layoutScale;
  const nextScale = Math.max(0.5, Math.min(4, oldScale * Math.exp(-event.deltaY * 0.001))),
    ratio = nextScale / oldScale;
  const x = event.clientX - bounds.left - bounds.width / 2,
    y = event.clientY - bounds.top - bounds.height / 2;
  state.layoutX = x - (x - state.layoutX) * ratio;
  state.layoutY = y - (y - state.layoutY) * ratio;
  state.layoutScale = nextScale;
  applyLayoutView();
}

/** 左键拖动只平移视图，拼法格坐标保持不变。 */
function startLayoutPan(event) {
  if (event.button !== 0) return;
  event.preventDefault();
  state.layoutDrag = { pointer: event.pointerId, x: event.clientX, y: event.clientY };
  event.currentTarget.setPointerCapture(event.pointerId);
  event.currentTarget.classList.add("panning");
}
function moveLayoutPan(event) {
  if (!state.layoutDrag || state.layoutDrag.pointer !== event.pointerId) return;
  state.layoutX += event.clientX - state.layoutDrag.x;
  state.layoutY += event.clientY - state.layoutDrag.y;
  state.layoutDrag.x = event.clientX;
  state.layoutDrag.y = event.clientY;
  applyLayoutView();
}
function endLayoutPan(event) {
  if (!state.layoutDrag || state.layoutDrag.pointer !== event.pointerId) return;
  state.layoutDrag = null;
  event.currentTarget.classList.remove("panning");
  if (event.currentTarget.hasPointerCapture(event.pointerId))
    event.currentTarget.releasePointerCapture(event.pointerId);
}

/** 用户主动保存当前拼法，SVG包含全部格子与编号。 */
function saveSvg() {
  const source = $("layout-board").querySelector("svg");
  if (!source) return;
  const svg = source.cloneNode(true);
  svg.style.removeProperty("transform");
  const content = new XMLSerializer().serializeToString(svg),
    url = URL.createObjectURL(new Blob([content], { type: "image/svg+xml" })),
    link = document.createElement("a");
  link.href = url;
  link.download = `${state.layout.name.replace(/[\\/:*?"<>|]/g, "_")} 拼法.svg`;
  link.click();
  setTimeout(() => URL.revokeObjectURL(url), 1500);
}

/** 显示读取成果或保存文件时的可读错误。 */
function toast(message) {
  $("toast").textContent = message;
  $("toast").hidden = false;
  clearTimeout(toast.timer);
  toast.timer = setTimeout(() => {
    $("toast").hidden = true;
  }, 7000);
}

/** 连接纯查询控件；成果页没有请求服务端计算的入口。 */
function boot() {
  $("summary").textContent =
    `${DATA.catalog.recipes.length}张基础卡 · ${libraryCards.length}张最终卡 · ${DATA.catalog.encounters.length}个回想`;
  $("created").textContent = `生成于 ${new Date(DATA.created * 1000).toLocaleString("zh-CN")}`;
  $("encounter-select").innerHTML = DATA.catalog.encounters
    .map(
      (e) =>
        `<option value="${e.id}">${esc(e.chapter)} · ${esc(e.name)} · ${e.mode === "connect" ? "连接" : "映像"}</option>`
    )
    .join("");
  document.querySelectorAll(".tab").forEach((button) => {
    button.addEventListener("click", () => {
      document.querySelectorAll(".tab").forEach((b) => {
        b.classList.toggle("active", b === button);
      });
      document.querySelectorAll(".tab-content").forEach((p) => {
        p.hidden = p.id !== `${button.dataset.tab}-tab`;
      });
      if (button.dataset.tab === "encounters") {
        if (!state.result) selectEncounter();
        else loadBattleDetails();
      }
    });
  });
  $("recipe-search").addEventListener("input", () => {
    updateRecipeChoices();
    setRecipeOptions(true);
  });
  $("recipe-search").addEventListener("focus", () => setRecipeOptions(true));
  $("recipe-search").addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      setRecipeOptions(false);
      event.currentTarget.blur();
    }
    if (event.key === "Backspace" && !event.currentTarget.value && state.recipeIds.size) {
      const ids = [...state.recipeIds];
      toggleRecipe(ids[ids.length - 1]);
    }
  });
  $("recipe-multiselect").addEventListener("click", (event) => {
    event.stopPropagation();
    const remove = event.target.closest("button[data-remove-recipe]"),
      option = event.target.closest("button[data-recipe]");
    if (remove) {
      toggleRecipe(Number(remove.dataset.removeRecipe));
      return;
    }
    if (option) {
      $("recipe-search").value = "";
      toggleRecipe(Number(option.dataset.recipe));
      return;
    }
    $("recipe-search").focus();
    setRecipeOptions(true);
  });
  for (const [key, config] of Object.entries(enumFilters)) {
    $(config.options).addEventListener("click", (event) => {
      const button = event.target.closest("button[data-filter]");
      if (button) toggleEnumValue(key, Number(button.dataset.value));
    });
  }
  $("include-chromatic").addEventListener("click", (event) => {
    state.includeChromatic = !state.includeChromatic;
    event.currentTarget.classList.toggle("selected", state.includeChromatic);
    event.currentTarget.setAttribute("aria-pressed", String(state.includeChromatic));
    updateRecipeChoices();
    filterLibrary();
  });
  for (const id of [
    "slots-op",
    "slots-value",
    "left-op",
    "left-value",
    "right-op",
    "right-value",
    "strikes-op",
    "strikes-value"
  ])
    $(id).addEventListener("change", filterLibrary);
  for (const id of [
    "sort-field-1",
    "sort-direction-1",
    "sort-field-2",
    "sort-direction-2",
    "sort-field-3",
    "sort-direction-3"
  ])
    $(id).addEventListener("change", () => {
      updateSortControls();
      filterLibrary();
    });
  $("library-page-size").addEventListener("change", () => {
    state.libraryPage = 0;
    renderLibraryPage();
  });
  $("library-page-prev").addEventListener("click", () => {
    if (state.libraryPage > 0) {
      state.libraryPage--;
      renderLibraryPage();
      $("recipe-count").scrollIntoView({ block: "start" });
    }
  });
  $("library-page-next").addEventListener("click", () => {
    const pages = Math.ceil(state.libraryResults.length / Number($("library-page-size").value));
    if (state.libraryPage + 1 < pages) {
      state.libraryPage++;
      renderLibraryPage();
      $("recipe-count").scrollIntoView({ block: "start" });
    }
  });
  $("clear-recipe-filters").addEventListener("click", clearLibraryFilters);
  $("encounter-select").addEventListener("change", selectEncounter);
  $("encounter-strikes").addEventListener("input", selectEncounter);
  $("encounter-rating").addEventListener("input", selectEncounter);
  $("phase-details").addEventListener("toggle", loadBattleDetails);
  $("event-details").addEventListener("toggle", loadBattleDetails);
  $("close-layout").addEventListener("click", () => $("layout-dialog").close());
  $("layout-dialog").addEventListener("close", () => {
    if (!$("layout-dialog").open) {
      state.layoutRequest++;
      state.layout = null;
    }
  });
  const layoutBoard = $("layout-board");
  layoutBoard.addEventListener("wheel", zoomLayout, { passive: false });
  layoutBoard.addEventListener("pointerdown", startLayoutPan);
  layoutBoard.addEventListener("pointermove", moveLayoutPan);
  layoutBoard.addEventListener("pointerup", endLayoutPan);
  layoutBoard.addEventListener("pointercancel", endLayoutPan);
  $("layout-flip").addEventListener("change", drawLayout);
  $("save-svg").addEventListener("click", saveSvg);
  $("results").addEventListener("keydown", (event) => {
    const card = event.target.closest("[data-library-layout]");
    if (card && (event.key === "Enter" || event.key === " ")) {
      event.preventDefault();
      showLayout(card.dataset.libraryLayout, null, false);
    }
  });
  document.addEventListener("click", (event) => {
    if (!event.target.closest("#recipe-multiselect")) setRecipeOptions(false);
    const libraryCard = event.target.closest("[data-library-layout]");
    if (libraryCard) {
      showLayout(libraryCard.dataset.libraryLayout, null, false);
      return;
    }
    const button = event.target.closest("button");
    if (!button) return;
    if (button.dataset.playerLayout !== undefined && state.result) {
      const card = state.result.cards[Number(button.dataset.playerLayout)];
      showLayout(card.template, card.materials);
    }
    if (button.dataset.piece) {
      state.piece = state.piece === Number(button.dataset.piece) ? 0 : Number(button.dataset.piece);
      drawLayout();
    }
  });
  try {
    updateRecipeChoices();
    for (const key of Object.keys(enumFilters)) updateEnumFilter(key);
    updateSortControls();
    filterLibrary();
  } catch (error) {
    toast(error.message);
  }
}
boot();
