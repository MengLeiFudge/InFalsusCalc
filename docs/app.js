/* 成果页读取静态配队，在浏览器按当前谱面条件结算，不发起搜索。 */
import { calculateBattle } from "./battle.js";
import { renderBattleProcess } from "./battle-view.js";
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
        ? "请访问 GitHub Pages，或在项目中双击“预览网页.cmd”打开网页。"
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
  power: "攻击优先，持平时选防御更高",
  fortitude: "防御优先，持平时选攻击更高",
  total: "攻防总和最高"
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
  layoutSize: null,
  piece: 0,
  layoutScale: 1,
  layoutX: 0,
  layoutY: 0,
  layoutPointers: new Map(),
  layoutPinch: null,
  layoutRequest: 0,
  encounterRequest: 0,
  board: null,
  battle: null
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
const traitName = (id) => trait(id)?.name || `技能${id}`;

/** 按原生比例合成技能图标；浮窗可复用图像而不创建第二个交互入口。 */
function traitPicture(item, showTier = true) {
  if (!item?.icon) return "";
  const tierIcon = showTier && DATA.catalog.trait_tiers[String(item.tier)];
  return `<span class="trait-icon"><img class="trait-frame" src="${item.frame}" alt=""><img class="trait-effect" src="${item.icon}" alt="">${tierIcon ? `<img class="trait-tier" src="${tierIcon}" alt="">` : ""}</span>`;
}

/** 技能入口通过统一顶层浮窗显示名称与精确效果说明。 */
function traitTag(id, iconOnly = false, { showTier = true, showSources = true } = {}) {
  return `<span class="trait-tag${iconOnly ? " icon-only" : ""}" data-trait="${id}" data-trait-sources="${showSources}" tabindex="0">${traitPicture(trait(id), showTier)}<span class="trait-label">${esc(traitName(id))}</span></span>`;
}

/** 当前顶层技能提示对应的入口，关闭或入口移除后释放。 */
let traitAnchor = null;

/** 关闭共享技能提示并移除入口上的无障碍关联。 */
function hideTraitTooltip() {
  traitAnchor?.removeAttribute("aria-describedby");
  traitAnchor = null;
  const tooltip = $("trait-tooltip");
  if (tooltip.matches(":popover-open")) tooltip.hidePopover();
}

/** 优先放在入口右侧，空间不足时改左侧，并保持浮窗在视口内。 */
function positionTraitTooltip() {
  if (!traitAnchor) return;
  if (!traitAnchor.isConnected) return hideTraitTooltip();
  const rect = traitAnchor.getBoundingClientRect();
  if (
    !rect.width ||
    !rect.height ||
    rect.bottom <= 0 ||
    rect.top >= innerHeight ||
    rect.right <= 0 ||
    rect.left >= innerWidth
  )
    return hideTraitTooltip();
  const tooltip = $("trait-tooltip"),
    box = tooltip.getBoundingClientRect();
  let x = rect.right + 10;
  if (x + box.width > innerWidth - 8) x = rect.left - box.width - 10;
  tooltip.style.left = `${Math.max(8, Math.min(x, innerWidth - box.width - 8))}px`;
  tooltip.style.top = `${Math.max(8, Math.min(rect.top + (rect.height - box.height) / 2, innerHeight - box.height - 8))}px`;
}

/** 原生popover进入顶层绘制，避免被技能所在的dialog及滚动容器裁切。 */
function showTraitTooltip(anchor) {
  if (traitAnchor === anchor) return;
  hideTraitTooltip();
  const item = trait(Number(anchor.dataset.trait));
  if (!item) return;
  const tooltip = $("trait-tooltip");
  const tiers = [...new Set(item.drop_sources.flatMap((source) => source.tiers))].sort((a, b) => a - b);
  const sources = item.drop_sources
    .map(
      (source) =>
        `<span>${esc(DATA.catalog.encounters.find((encounter) => encounter.id === source.encounter).name)}</span>`
    )
    .join("");
  // 阶数优先，每阶按红、黄、蓝、绿、紫排列，与颜色枚举中的绿蓝顺序不同。
  const particles = tiers
    .flatMap((tier) =>
      [1, 2, 4, 3, 5].map(
        (color) =>
          `<img src="${DATA.catalog.particle_icons[color][tier]}" alt="${colorNames[color]}色${tier}阶粒子" width="50" height="45">`
      )
    )
    .join("");
  const origin = tiers.length
    ? `<div class="trait-particle-icons">${particles}</div>${sources}`
    : "<span>最后四个回想无掉落</span>";
  const showSources = anchor.dataset.traitSources !== "false";
  tooltip.innerHTML = `${traitPicture(item, false)}<div class="trait-tooltip-copy"><strong>${esc(item.name)}</strong><span>${esc(plain(item.description) || "没有可用的效果说明。")}</span>${showSources ? `<div class="trait-tooltip-sources">${origin}</div>` : ""}</div>`;
  traitAnchor = anchor;
  anchor.setAttribute("aria-describedby", "trait-tooltip");
  tooltip.showPopover();
  positionTraitTooltip();
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

/** 按原生实际绑定的技能池归组；每颗的抽取次数仍显示在各自粒子配置旁。 */
function encounterDrops(encounter) {
  const loot = encounter.drops,
    groups = new Map();
  for (const drop of loot.items.filter((item) => item.weight > 0)) {
    if (!groups.has(drop.trait_pool)) groups.set(drop.trait_pool, []);
    groups.get(drop.trait_pool).push(drop);
  }
  const content = [...groups]
    .map(([id, drops]) => {
      const pool = loot.trait_pools[id];
      const particles = drops
        .map((drop) => {
          const chance = (drop.weight / loot.total_weight) * 100;
          return `<div class="drop-shape" style="--chance:${chance}%"><span class="shape-picture">${shapeIcon(drop.shape)}</span><div class="drop-shape-info"><strong>${colorNames[drop.color]} · ${drop.size}格 · ${drop.tier}阶</strong><span class="drop-chance">${num(chance, 2)}%</span><small>效能 ${drop.min_potency}–${drop.max_potency}</small><small>${drop.trait_rolls ? `每颗抽技能 ${drop.trait_rolls} 次` : "不带技能"}</small></div></div>`;
        })
        .join("");
      const skills = pool.traits
        .filter((item) => item.weight > 0)
        .map((item) => {
          const chance = (item.weight / pool.total_weight) * 100;
          return `<div class="loot-trait" style="--chance:${chance}%">${traitTag(item.id)}<strong>${num(chance, 2)}%</strong></div>`;
        })
        .join("");
      const emptyChance = (pool.no_trait_weight / pool.total_weight) * 100;
      const skillPool = drops.some((drop) => drop.trait_rolls > 0)
        ? `<div class="loot-pool"><h4>这些粒子每次抽技能的概率</h4><div class="loot-traits">${skills}<div class="loot-trait loot-empty" style="--chance:${emptyChance}%"><span>空 · 不增加技能</span><strong>${num(emptyChance, 2)}%</strong></div></div></div>`
        : "";
      return `<section class="drop-group"><h4>会掉这些粒子</h4><div class="drop-shapes">${particles}</div>${skillPool}</section>`;
    })
    .join("");
  const quantity =
    loot.base_rolls > 0
      ? `<strong>基础掉 <span>${loot.base_rolls}</span> 颗</strong><p>每颗都按下面的概率单独随机；成绩好、有相关全局技能还会多掉。</p>`
      : "<strong>基础不掉落</strong>";
  $("encounter-drops").innerHTML = `<div class="loot-summary">${quantity}</div>${content}`;
}

/** 展示当前材料能提供的技能，并用游戏描述解释触发条件。 */
function traitList(card) {
  if (!card.slots) return '<p class="help">这张卡没有技能槽。</p>';
  return (
    card.available_traits.map((id) => traitTag(id, false, { showTier: false })).join("") ||
    '<p class="help">这套粒子没有可选技能。</p>'
  );
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

/** 未选颜色时忽略变色开关；选颜色后匹配本色，或允许变色时匹配可用颜色。 */
function matchesPrimary(card, recipe) {
  if (state.tiers.size && !state.tiers.has(recipe.tier)) return false;
  if (!state.colors.size) return true;
  return state.includeChromatic ? card.colors.some((color) => state.colors.has(color)) : state.colors.has(recipe.color);
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
  hideTraitTooltip();
  const size = Number($("library-page-size").value),
    total = state.libraryResults.length,
    pages = Math.max(1, Math.ceil(total / size));
  state.libraryPage = Math.min(state.libraryPage, pages - 1);
  const start = state.libraryPage * size,
    cards = state.libraryResults.slice(start, start + size);
  renderCards(
    $("results"),
    cards.length
      ? cards.map(libraryCard).join("")
      : '<p class="empty-results">没有符合条件的卡牌，试试减少筛选条件。</p>'
  );
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

/** 基础攻防沿用制卡区域求和值；特性种数表示材料可提供的选择，不是槽数或已装备数量。 */
function cardFacts(card) {
  return [`攻 ${card.base_power}`, `防 ${card.base_fortitude}`, `技能 ${card.available_trait_count}`];
}

/** 玩家与敌方复用游戏卡面；无配方的敌方使用封面且不显示等级图案。 */
function cardVisual(card, color, traits = [], { interactive = false, showChanges = false, playerSlot = null } = {}) {
  const recipe = DATA.catalog.recipes.find((item) => item.id === card.recipe),
    sr = recipe?.is_sr ? 1 : 0,
    slots = Math.max(1, Math.min(3, card.slots));
  const assets = DATA.catalog.card_assets,
    rank = recipe ? assets.ranks[String(recipe.tier)] : null,
    assetKey = `${color || 1}-${sr}`,
    frame = assets.frames[`${assetKey}-${slots}`];
  const behavior = interactive
    ? ` role="button" tabindex="0" ${playerSlot === null ? `data-library-layout="${card.id}"` : `data-player-layout="${playerSlot}"`} aria-label="查看${esc(card.name)}详情与拼法"`
    : "";
  const otherColors = showChanges ? card.colors.filter((value) => value !== recipe.color).sort((a, b) => a - b) : [];
  const changeIcons = otherColors
    .map(
      (value, index) =>
        `<img class="card-change-icon" data-slot="${index}" src="${assets.changes[value]}" alt="可变为${colorNames[value]}色" title="可变为${colorNames[value]}色">`
    )
    .join("");
  const slotsHtml = Array.from({ length: card.slots }, (_, index) =>
    traits[index]
      ? traitTag(traits[index], true, { showSources: !!recipe })
      : `<span class="trait-empty"><img src="${DATA.catalog.trait_border}" alt="空技能槽"></span>`
  ).join("");
  const pip = (side, index, x, value) =>
    `<img class="range-pip ${side}" style="left:${x}%" src="${assets.common[index < value ? "medium-range-active" : "medium-range-inactive"]}" alt="">`;
  // 游戏leftRangePips从靠近中心的一格开始，向左依次展开。
  const leftPips = [34.48, 30.5, 26.53, 22.55].map((x, index) => pip("left", index, x, card.left)).join("");
  const rightPips = [54.36, 58.34, 62.31, 66.3].map((x, index) => pip("right", index, x, card.right)).join("");
  const levelIcon = rank
    ? `<img class="card-level-icon" style="height:${(rank.height / 820) * 100}%" src="${rank.image}" alt="等级${recipe.tier}">`
    : "";
  const facts = recipe
    ? `<div class="card-facts">${cardFacts(card)
        .map((text) => `<span>${esc(text)}</span>`)
        .join(" · ")}</div>`
    : "";
  const face = `<img class="card-art" decoding="async" src="${recipe ? assets.art[card.recipe] : assets.enemy_art}" alt="${esc(card.name)}立绘"><img class="card-frame" src="${frame}" alt=""><img class="card-inner-frame" src="${assets.common["art-frame"]}" alt=""><img class="card-top-connector" src="${assets.common["connector-top"]}" alt="">${color ? `<img class="card-color-icon" src="${assets.icons[assetKey]}" alt="${colorNames[color]}色">` : ""}${levelIcon}<img class="card-tier-backing" src="${assets.tiers[assetKey]}" alt="">${changeIcons}<img class="card-bottom-drawer" src="${assets.common["medium-bottom-drawer"]}" alt=""><img class="card-bottom-connector" src="${assets.common["connector-top"]}" alt=""><h4>${esc(card.name)}</h4>${facts}<div class="card-stats"><span class="power"><img src="${assets.stat_icons[`${assetKey}-power`]}" alt="攻击"><strong>${num(card.power)}</strong></span><span class="fortitude"><img src="${assets.stat_icons[`${assetKey}-fortitude`]}" alt="防御"><strong>${num(card.fortitude)}</strong></span></div><div class="card-range">${leftPips}<img class="range-center" src="${assets.common["medium-range-center"]}" alt="">${rightPips}</div><div class="card-traits">${slotsHtml}</div>`;
  // 所有图片统一延迟到卡牌接近视口时加载，避免先出现卡框再补立绘和技能。
  return `<article class="game-card-visual${interactive ? " interactive" : ""}${color === 0 ? " neutral" : ""}" style="--card-color:${colors[color]}" aria-busy="true"${behavior}>${face.replaceAll(" src=", " data-src=")}<div class="card-placeholder"><span>卡面加载中…</span><button type="button" class="secondary" data-retry-card hidden>重新加载</button></div></article>`;
}

/** 卡面只在接近视口时解码。替换列表时撤销旧节点观察，异步结果只更新仍在页面内的卡。 */
const cardObserver = new IntersectionObserver(
  (entries) => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      cardObserver.unobserve(entry.target);
      loadCard(entry.target);
    }
  },
  { rootMargin: "240px" }
);

/** 替换当前卡面容器，并为骨架卡登记视口观察。 */
function renderCards(root, html) {
  root.querySelectorAll(".game-card-visual").forEach((card) => cardObserver.unobserve(card));
  root.innerHTML = html;
  root.querySelectorAll(".game-card-visual").forEach((card) => cardObserver.observe(card));
}

/** 全部图层解码成功才显示整张卡；慢网继续等待，加载或解码失败保留骨架与重试。 */
async function loadCard(card) {
  if (!card.isConnected || card.classList.contains("is-decoding") || card.classList.contains("is-ready")) return;
  card.classList.add("is-decoding");
  card.classList.remove("is-error");
  card.setAttribute("aria-busy", "true");
  const placeholder = card.querySelector(".card-placeholder"),
    retry = placeholder.querySelector("button");
  placeholder.firstElementChild.textContent = "卡面加载中…";
  retry.hidden = true;
  let timer;
  try {
    const images = [...card.querySelectorAll("img[data-src]")];
    const ready = images.map((image) => {
      image.decoding = "async";
      image.src = image.dataset.src;
      return image.decode();
    });
    timer = setTimeout(() => {
      if (card.isConnected) placeholder.firstElementChild.textContent = "加载较慢，请稍候…";
    }, 20000);
    await Promise.all(ready);
    if (!card.isConnected) return;
    card.classList.add("is-ready");
    placeholder.hidden = true;
  } catch (error) {
    if (!card.isConnected) return;
    card.classList.add("is-error");
    placeholder.firstElementChild.textContent = "卡面没加载出来";
    retry.hidden = false;
    console.warn("卡面图片加载失败", error);
  } finally {
    clearTimeout(timer);
    card.classList.remove("is-decoding");
    card.setAttribute("aria-busy", "false");
  }
}

/** 一览始终显示本色，其他可用颜色由卡面侧边的原生标记表达。 */
function libraryCard(card) {
  const color = DATA.catalog.recipes.find((recipe) => recipe.id === card.recipe).color;
  return `<div class="player-card library-card">${cardVisual(card, color, [], { interactive: true, showChanges: true })}</div>`;
}

/** 两方均使用同一卡面；敌方按位置命名，只有推荐配队提供拼法入口。 */
function gameCard(card, slot, player = false) {
  const skills = card.traits.filter((id) => id > 1);
  if (!player)
    return `<div class="player-card">${cardVisual({ ...card, name: `卡牌${slot + 1}`, slots: skills.length }, card.color, skills)}</div>`;
  return `<div class="player-card">${cardVisual(card, card.color, skills, { interactive: true, playerSlot: slot })}</div>`;
}

/** 克制环与Battle.Advantage一致：红→绿→紫→黄→蓝→红；无色不参与克制。 */
const advantage = [0, 3, 4, 5, 1, 2];

/** 复用游戏选卡界面的连线、方向箭头和优势色底纹，标注优势方的攻防倍率。 */
function matchupVisual(playerColor, enemyColor) {
  const playerWins = playerColor !== 0 && advantage[playerColor] === enemyColor;
  const enemyWins = enemyColor !== 0 && advantage[enemyColor] === playerColor;
  const assets = DATA.catalog.matchup_assets;
  const label = playerWins ? "我方颜色克制，攻防×1.25" : enemyWins ? "对方颜色克制，攻防×1.25" : "无颜色克制";
  const detail =
    playerWins || enemyWins
      ? `<img class="matchup-backing" src="${assets.colors[playerWins ? playerColor : enemyColor]}" alt=""><img class="matchup-arrow" src="${enemyWins ? assets.arrow_negative : assets.arrow}" alt=""><span class="matchup-label">×1.25</span>`
      : "";
  return `<div class="card-matchup${enemyWins ? " enemy-advantage" : ""}${playerWins || enemyWins ? "" : " no-advantage"}" role="img" aria-label="${label}" title="${label}"><img class="matchup-line" src="${enemyWins ? assets.line_negative : assets.line}" alt="">${detail}</div>`;
}

/** 汇总开战前攻防，按对应位置应用克制；我方另传入联觉和谱面等级倍率，不计阶段技能。 */
function deckTotals(cards, opponent, bonus = 1) {
  let power = 0,
    fortitude = 0;
  cards.forEach((card, slot) => {
    const multiplier = (card.color !== 0 && advantage[card.color] === opponent[slot].color ? 1.25 : 1) * bonus;
    power += card.power * multiplier;
    fortitude += card.fortitude * multiplier;
  });
  return `<span>总攻击 <strong>${num(power)}</strong></span><span>总防御 <strong>${num(fortitude)}</strong></span>`;
}

/** 进入回想页或切换条件时读取当前回想，旧请求完成后不得覆盖新选择。 */
async function selectEncounter() {
  hideTraitTooltip();
  const request = ++state.encounterRequest;
  const eid = Number($("encounter-select").value),
    maxStrikes = Number($("encounter-strikes").value),
    rating = Number($("encounter-rating").value) || DATA.defaults.search_rating,
    notes = $("encounter-notes").valueAsNumber;
  state.result = null;
  state.battle = null;
  const oldTooltip = $("battle-process").querySelector(".battle-node-tooltip");
  if (oldTooltip?.matches(":popover-open")) oldTooltip.hidePopover();
  $("battle-process").replaceChildren();
  $("encounter-content").hidden = true;
  $("encounter-loading").hidden = false;
  $("encounter-loading").textContent = "正在加载当前回想…";
  $("encounter-rating-value").textContent = rating;
  $("encounter-strikes-value").textContent = maxStrikes;
  if (!Number.isInteger(notes) || notes < 25 || notes > 32767) {
    $("encounter-loading").textContent = "总判定数请填25–32767的整数，长条也算在内。";
    return;
  }
  try {
    const entry = DATA.catalog.encounters.find((e) => e.id === eid);
    const [payload] = await Promise.all([loadJson(`./data/${entry.file}`), loadCommon()]);
    if (request !== state.encounterRequest) return;
    const encounter = { ...entry, ...payload };
    const result = payload.decks[maxStrikes];
    if (!result) throw new Error("这个回想缺少所选条件的推荐配队。");
    Object.assign(DATA.templates, payload.templates);
    const players = result.cards.map((card) => ({
      ...DATA.templates[card.template],
      color: card.color,
      traits: card.traits
    }));
    const b = calculateBattle(players, encounter, DATA.catalog.traits, rating, notes);
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
      `单卡惩罚≤${maxStrikes} · 谱面等级${rating} · ${notes}个全EXACT判定 · 联觉开启 · 初始HP100`;
    $("battle-status").textContent = b.passed ? "可通关" : "当前条件下未通关";
    $("battle-status").className = `badge ${b.passed ? "good" : "bad"}`;
    renderCards(
      $("deck-matchups"),
      players
        .map(
          (card, slot) =>
            `<div class="matchup-column">${gameCard(encounter.cards[slot], slot)}${matchupVisual(card.color, encounter.cards[slot].color)}${gameCard(card, slot, true)}</div>`
        )
        .join("")
    );
    const chromatic = 1 + 0.05 * new Set(players.map((card) => card.color).filter((color) => color !== 0)).size;
    $("enemy-totals").innerHTML = deckTotals(encounter.cards, players);
    $("player-totals").innerHTML = deckTotals(players, encounter.cards, chromatic * (1 + rating / 100));

    renderBattleProcess($("battle-process"), b, (id) => {
      const item = trait(id);
      return `<div class="node-trait">${traitPicture(item, false)}<div><strong>${esc(item.name)}</strong><p>${esc(plain(item.description))}</p></div></div>`;
    });
    $("encounter-content").hidden = false;
    $("encounter-loading").hidden = true;
  } catch (error) {
    if (request !== state.encounterRequest) return;
    state.result = null;
    state.battle = null;
    $("encounter-loading").textContent = `加载失败，请重新选择回想或切换页面重试。${error.message}`;
  }
}

/** 读取卡牌拼法并忽略失效请求；配队入口携带成品颜色与技能，一览入口使用本色。 */
async function showLayout(identity, assignment = null, showGoals = true, craftedCard = null) {
  hideTraitTooltip();
  const request = ++state.layoutRequest;
  const summary = DATA.templates[identity];
  state.layout = null;
  state.layoutSize = null;
  state.board = null;
  $("layout-title").textContent = summary.name;
  $("layout-goals").textContent = "";
  renderCards($("layout-card"), "");
  for (const id of ["layout-card-details", "piece-list", "layout-traits"]) $(id).textContent = "";
  $("layout-board").textContent = "正在加载卡牌详情…";
  $("save-svg").disabled = true;
  $("show-traits").disabled = true;
  $("show-traits").hidden = !summary.slots;
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
    state.layoutPointers.clear();
    state.layoutPinch = null;
    const goalText = showGoals ? (card.goals || []).map((g) => goalNames[g]).join(" · ") : "";
    $("layout-goals").textContent = goalText;
    $("layout-goals").hidden = !goalText;
    $("layout-title").textContent = card.name;
    const recipe = DATA.catalog.recipes.find((item) => item.id === card.recipe);
    renderCards(
      $("layout-card"),
      cardVisual(card, craftedCard?.color ?? recipe.color, craftedCard?.traits.filter((id) => id > 1) ?? [], {
        showChanges: !craftedCard
      })
    );
    const amounts = card.raw_penalties,
      allowances = card.strike_tolerances;
    const strikeRows = DATA.catalog.strike_names
      .map((name, index) => {
        const amount = amounts[index],
          allowance = allowances[index];
        const used = Math.min(amount, allowance),
          excess = Math.max(0, amount - allowance);
        const description = `${name}：${amount}次，可抵消${allowance}次，实际罚${excess}分`;
        const marks = [
          ["excess", excess],
          ["unused", allowance - used],
          ["used", used]
        ]
          .map(([kind, count]) =>
            Array.from(
              { length: count },
              () =>
                `<span class="strike-slot"><img class="strike-${kind}" src="${DATA.catalog.strike_icons[kind]}" alt=""></span>`
            ).join("")
          )
          .join("");
        return `<div class="strike-row"><span>${name}</span><span class="strike-marks" role="img" aria-label="${description}" title="${description}">${marks || "—"}</span></div>`;
      })
      .join("");
    $("layout-card-details").innerHTML =
      `<div><span>所需粒子</span><strong>Ⅰ ${card.tier_counts[0]} · Ⅱ ${card.tier_counts[1]} · Ⅲ ${card.tier_counts[2]}</strong></div><div><span>粒子总数</span><strong>${card.count}/${card.particle_limit}</strong></div>${strikeRows}<div><span>总惩罚</span><strong>${card.strikes}分 · 面板保留${num(card.retained_percent, card.strikes ? 2 : 0)}%</strong></div>`;
    $("layout-traits").innerHTML = traitList(card);
    drawLayout();
    $("save-svg").disabled = false;
    $("show-traits").hidden = !card.slots;
    $("show-traits").disabled = !card.slots;
  } catch (error) {
    if (request !== state.layoutRequest || !$("layout-dialog").open) return;
    $("layout-board").textContent = `加载失败，关闭后再次点击卡牌可重试。${error.message}`;
  }
}

/** 六边格外接圆半径，单位为CSS像素；所有卡牌的初始画布和导出共享此尺寸。 */
const layoutCellRadius = 24;

/** 按游戏Q、R坐标绘制平顶六边格。 */
function hexPoints(x, y) {
  return Array.from(
    { length: 6 },
    (_, i) => `${x + Math.cos((i * Math.PI) / 3)},${y + Math.sin((i * Math.PI) / 3)}`
  ).join(" ");
}

/** 先绘底板，再绘连通粒子的外轮廓；重叠、目标色及颜色不符单独标注，SVG可独立保存。 */
function drawLayout() {
  if (!state.layout) return;
  const card = state.layout,
    recipe = state.board;
  const xy = (q, r) => [1.5 * q, -Math.sqrt(3) * (r + q / 2)];
  const safe = new Set(recipe.safe.map(([q, r]) => `${q},${r}`));
  const targets = new Map(recipe.targets.map(([q, r, color]) => [`${q},${r}`, color]));
  const occupied = new Map();
  card.placements.forEach((piece, i) => {
    for (const [q, r] of piece.cells) {
      const key = `${q},${r}`;
      if (!occupied.has(key)) occupied.set(key, []);
      occupied.get(key).push(i + 1);
    }
  });
  const relevant = [...recipe.safe, ...card.placements.flatMap((piece) => piece.cells)].map(([q, r]) => xy(q, r));
  const minX = Math.min(...relevant.map(([x]) => x)) - 2,
    maxX = Math.max(...relevant.map(([x]) => x)) + 2,
    minY = Math.min(...relevant.map(([, y]) => y)) - 2,
    boardMaxY = Math.max(...relevant.map(([, y]) => y)) + 2;
  const board = [],
    overlaps = [],
    marks = [];
  const allCells = new Map(
    [...recipe.cells, ...card.placements.flatMap((piece) => piece.cells)].map(([q, r]) => [`${q},${r}`, [q, r]])
  );
  for (const [key, [q, r]] of allCells) {
    const [x, y] = xy(q, r);
    if (x < minX - 1 || x > maxX + 1 || y < minY - 1 || y > boardMaxY + 1) continue;
    const pieces = occupied.get(key) || [],
      target = targets.get(key),
      isSafe = safe.has(key),
      wrong = !!target && pieces.some((id) => card.placements[id - 1].color !== target),
      dim = state.piece && !pieces.includes(state.piece);
    const label = target ? `${colorNames[target]}色目标` : isSafe ? "白区" : "框外·会扣分（容忍可抵消）";
    board.push(
      `<g><title>Q=${q}, R=${r} · ${label}</title><polygon points="${hexPoints(x, y)}" fill="${target ? colors[target] : isSafe ? "#e4edf2" : "#3c2228"}" fill-opacity="${target ? 0.36 : 1}" stroke="${isSafe ? "#758c99" : "#9c6571"}" stroke-width=".045" ${isSafe ? "" : 'stroke-dasharray=".12 .08"'}/></g>`
    );
    if (pieces.length > 1)
      overlaps.push(
        `<g class="layout-cell${dim ? " dim" : ""}"><polygon points="${hexPoints(x, y)}" fill="url(#layout-overlap)"/></g>`
      );
    const overlay = [
      pieces.length > 1
        ? `<circle cx="${x + 0.48}" cy="${y - 0.43}" r=".25" fill="#132430" stroke="${wrong ? "#ffd17a" : "#f0f6fa"}" stroke-width=".04"/><text x="${x + 0.48}" y="${y - 0.34}" font-size=".26" fill="#fff" text-anchor="middle">×${pieces.length}</text>`
        : "",
      pieces.length
        ? `<text x="${x}" y="${y + 0.12}" class="piece-number" text-anchor="middle">${pieces.join("/")}</text>`
        : "",
      pieces.length
        ? `<rect x="${x - 0.19}" y="${y + 0.52}" width=".38" height=".13" rx=".05" fill="${target ? colors[target] : isSafe ? "#e4edf2" : "#3c2228"}" stroke="${target ? "#edf4f7" : isSafe ? "#7a93a3" : "#bd8590"}" stroke-width=".04"/>`
        : "",
      wrong
        ? `<path d="M${x - 0.68},${y - 0.46} l.2,-.35 .2,.35 Z" fill="#ffd17a" stroke="#3a2b18" stroke-width=".035"/><text x="${x - 0.48}" y="${y - 0.52}" font-size=".25" fill="#392714" text-anchor="middle">!</text>`
        : ""
    ].join("");
    if (overlay)
      marks.push(
        `<g class="layout-cell${dim ? " dim" : ""}"><title>${label}${pieces.length ? ` · 粒子${pieces.join("、")}` : ""}${pieces.length > 1 ? ` · 重叠${pieces.length}层` : ""}${wrong ? " · 颜色不符" : ""}</title>${overlay}</g>`
      );
  }
  // 已放粒子的格保留白区/目标色/框外扣分区的小色标；粒子内部不再绘格线。
  // R轴在SVG中向上；六条边按屏幕顺时针排列，每条边仅在本粒子无相邻格时画轮廓。
  const neighbors = [
    [1, -1],
    [0, -1],
    [-1, 0],
    [-1, 1],
    [0, 1],
    [1, 0]
  ];
  const pieceLayers = card.placements.map((piece, i) => {
    const cells = new Set(piece.cells.map(([q, r]) => `${q},${r}`));
    // 全部六边格合成一个填充path，内部公共边相互抵消，避免逐格抗锯齿产生接缝。
    const fill = [],
      edges = [];
    for (const [q, r] of piece.cells) {
      const [x, y] = xy(q, r);
      fill.push(`M${hexPoints(x, y).replaceAll(" ", " L")} Z`);
      neighbors.forEach(([dq, dr], edge) => {
        if (cells.has(`${q + dq},${r + dr}`)) return;
        const angle = (edge * Math.PI) / 3,
          next = ((edge + 1) * Math.PI) / 3;
        edges.push(`M${x + Math.cos(angle)},${y + Math.sin(angle)} L${x + Math.cos(next)},${y + Math.sin(next)}`);
      });
    }
    const selected = state.piece === i + 1,
      className = `layout-piece${state.piece && !selected ? " dim" : ""}${selected ? " highlight" : ""}`;
    // 填充和外轮廓分层，后画粒子的底色不会遮住重叠粒子的边界。
    return {
      fill: `<g class="${className}"><path d="${fill.join(" ")}" fill="${colors[piece.color]}"/></g>`,
      outline: `<g class="${className}"><path class="piece-outline" d="${edges.join(" ")}" fill="none" stroke="#172a38" stroke-width=".085" stroke-linecap="round" stroke-linejoin="round"/></g>`
    };
  });
  const legendLabels = ["白区", "目标色", "框外·会扣分", "重叠", "颜色不符"],
    legendColumns = Math.min(5, Math.max(1, Math.floor((maxX - minX - 1) / 3.4))),
    legendRows = Math.ceil(legendLabels.length / legendColumns),
    maxY = boardMaxY + legendRows * 0.7;
  const legend = legendLabels
    .map((label, i) => {
      const x = minX + 0.65 + (i % legendColumns) * 3.4,
        y = boardMaxY + Math.floor(i / legendColumns) * 0.7;
      const swatch =
        i === 4
          ? `<path d="M${x - 0.2},${y + 0.18} l.2,-.36 .2,.36 Z" fill="#ffd17a"/>`
          : `<rect x="${x - 0.2}" y="${y - 0.17}" width=".4" height=".34" rx=".04" fill="${["#e4edf2", colors[1], "#3c2228", "url(#layout-overlap)"][i]}" stroke="${i === 2 ? "#9c6571" : "#91a7b5"}" stroke-width=".035" ${i === 2 ? 'stroke-dasharray=".12 .08"' : ""}/>`;
      return `${swatch}<text x="${x + 0.36}" y="${y + 0.14}" font-size=".4" fill="#c4d4df">${label}</text>`;
    })
    .join("");
  state.layoutSize = { width: (maxX - minX) * layoutCellRadius, height: (maxY - minY) * layoutCellRadius };
  $("layout-board").innerHTML =
    `<svg xmlns="http://www.w3.org/2000/svg" class="layout-plot" width="${state.layoutSize.width}" height="${state.layoutSize.height}" viewBox="${minX} ${minY} ${maxX - minX} ${maxY - minY}" role="img" aria-label="${esc(card.name)}的粒子拼法"><style>.layout-plot text{font-family:sans-serif;pointer-events:none}.layout-plot .layout-piece.dim,.layout-plot .layout-cell.dim{opacity:.2}.layout-plot .layout-piece.highlight .piece-outline{stroke:#fff;stroke-width:.13}.layout-plot .piece-number{font-size:.34px;fill:#142b38;fill-opacity:.8;font-weight:600;paint-order:stroke;stroke:#f3f7fa;stroke-width:.025px}</style><defs><pattern id="layout-overlap" width=".24" height=".24" patternUnits="userSpaceOnUse" patternTransform="rotate(30)"><path d="M0,0 V.24" stroke="#102433" stroke-opacity=".65" stroke-width=".075"/></pattern></defs><rect x="${minX}" y="${minY}" width="${maxX - minX}" height="${maxY - minY}" fill="#0c151d"/>${board.join("")}${pieceLayers.map((piece) => piece.fill).join("")}${overlaps.join("")}${pieceLayers.map((piece) => piece.outline).join("")}${marks.join("")}${legend}</svg>`;
  applyLayoutView();
  $("piece-list").innerHTML = card.placements
    .map((piece, i) => {
      const shape = DATA.catalog.shapes.find((item) => item.id === piece.id),
        points = shape.cells.map(([q, r]) => xy(q, r));
      const x0 = Math.min(...points.map(([x]) => x)) - 1.2,
        y0 = Math.min(...points.map(([, y]) => y)) - 1.2,
        w = Math.max(...points.map(([x]) => x)) - x0 + 1.2,
        h = Math.max(...points.map(([, y]) => y)) - y0 + 1.2;
      const material = state.assignment?.find((item) => item.piece === i + 1),
        skills = material ? ` · ${material.traits.map(traitName).join("、")}` : "";
      return `<button class="${state.piece === i + 1 ? "selected" : ""}" data-piece="${i + 1}" title="${shape.tier}阶 · Q=${piece.q}, R=${piece.r}${esc(skills)}">${i + 1}<svg viewBox="${x0} ${y0} ${w} ${h}">${points.map(([x, y]) => `<polygon points="${hexPoints(x, y)}" fill="${colors[piece.color]}" stroke="#172933" stroke-width=".08"/>`).join("")}</svg><span>${piece.q},${piece.r}</span></button>`;
    })
    .join("");
}

/** 改变SVG实际视口尺寸以触发矢量重绘，不用CSS scale放大缓存图层。 */
function applyLayoutView() {
  const svg = $("layout-board").querySelector("svg");
  if (!svg) return;
  svg.setAttribute("width", state.layoutSize.width * state.layoutScale);
  svg.setAttribute("height", state.layoutSize.height * state.layoutScale);
  svg.style.left = `calc(50% + ${state.layoutX}px)`;
  svg.style.top = `calc(50% + ${state.layoutY}px)`;
}

/** 把页面坐标换算为以拼法画布中心为原点的CSS像素坐标。 */
function layoutPoint(clientX, clientY) {
  const bounds = $("layout-board").getBoundingClientRect();
  return { x: clientX - bounds.left - bounds.width / 2, y: clientY - bounds.top - bounds.height / 2 };
}

/** 在两个画布坐标之间缩放并平移，使手势下的拼法内容持续跟随手指。 */
function scaleLayout(nextScale, from, to = from) {
  const oldScale = state.layoutScale,
    scale = Math.max(0.1, Math.min(8, nextScale)),
    ratio = scale / oldScale;
  state.layoutX = to.x - (from.x - state.layoutX) * ratio;
  state.layoutY = to.y - (from.y - state.layoutY) * ratio;
  state.layoutScale = scale;
  applyLayoutView();
}

/** 返回当前前两根手指的中点和距离；额外触点不参与缩放。 */
function layoutPinch() {
  const points = [...state.layoutPointers.values()].slice(0, 2);
  if (points.length < 2) return null;
  return {
    center: layoutPoint((points[0].x + points[1].x) / 2, (points[0].y + points[1].y) / 2),
    distance: Math.hypot(points[0].x - points[1].x, points[0].y - points[1].y)
  };
}

/** 鼠标滚轮以指针位置为中心缩放拼法画布。 */
function zoomLayout(event) {
  event.preventDefault();
  const point = layoutPoint(event.clientX, event.clientY);
  scaleLayout(state.layoutScale * Math.exp(-event.deltaY * 0.001), point);
}

/** 单指或鼠标平移；双指同时按距离缩放、按中点移动平移。 */
function startLayoutPan(event) {
  if (event.pointerType === "mouse" && event.button !== 0) return;
  event.preventDefault();
  state.layoutPointers.set(event.pointerId, { x: event.clientX, y: event.clientY });
  state.layoutPinch = layoutPinch();
  event.currentTarget.setPointerCapture(event.pointerId);
  event.currentTarget.classList.add("panning");
}
function moveLayoutPan(event) {
  const previous = state.layoutPointers.get(event.pointerId);
  if (!previous) return;
  event.preventDefault();
  state.layoutPointers.set(event.pointerId, { x: event.clientX, y: event.clientY });
  if (state.layoutPointers.size >= 2) {
    const current = layoutPinch();
    if (state.layoutPinch && current) {
      const ratio = state.layoutPinch.distance > 0 ? current.distance / state.layoutPinch.distance : 1;
      scaleLayout(state.layoutScale * ratio, state.layoutPinch.center, current.center);
    }
    state.layoutPinch = current;
    return;
  }
  state.layoutPinch = null;
  state.layoutX += event.clientX - previous.x;
  state.layoutY += event.clientY - previous.y;
  applyLayoutView();
}
function endLayoutPan(event) {
  if (!state.layoutPointers.has(event.pointerId)) return;
  state.layoutPointers.delete(event.pointerId);
  state.layoutPinch = layoutPinch();
  if (event.currentTarget.hasPointerCapture(event.pointerId))
    event.currentTarget.releasePointerCapture(event.pointerId);
  if (!state.layoutPointers.size) event.currentTarget.classList.remove("panning");
}

/** 用户主动保存当前拼法，SVG包含全部格子与编号。 */
function saveSvg() {
  const source = $("layout-board").querySelector("svg");
  if (!source) return;
  const svg = source.cloneNode(true);
  svg.removeAttribute("style");
  svg.querySelectorAll(".dim, .highlight").forEach((node) => node.classList.remove("dim", "highlight"));
  svg.setAttribute("width", state.layoutSize.width);
  svg.setAttribute("height", state.layoutSize.height);
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
  for (const path of new Set([...Object.values(DATA.catalog.card_assets.common), DATA.catalog.trait_border])) {
    const link = document.createElement("link");
    link.rel = "preload";
    link.as = "image";
    link.href = path;
    document.head.append(link);
  }
  $("summary").textContent =
    `配方${DATA.catalog.recipes.length} · 卡牌${libraryCards.length} · 回想${DATA.catalog.encounters.length}`;
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
      const nodeTooltip = $("battle-process").querySelector(".battle-node-tooltip");
      if (nodeTooltip?.matches(":popover-open")) nodeTooltip.hidePopover();
      if (button.dataset.tab === "encounters" && !state.result) selectEncounter();
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
  $("encounter-notes").addEventListener("input", selectEncounter);

  $("show-conditions").addEventListener("click", () => $("conditions-dialog").showModal());
  $("close-conditions").addEventListener("click", () => $("conditions-dialog").close());
  $("show-traits").addEventListener("click", () => $("traits-dialog").showModal());
  $("close-traits").addEventListener("click", () => $("traits-dialog").close());
  $("close-layout").addEventListener("click", () => $("layout-dialog").close());
  $("layout-dialog").addEventListener("close", () => {
    if (!$("layout-dialog").open) {
      renderCards($("layout-card"), "");
      if ($("traits-dialog").open) $("traits-dialog").close();
      state.layoutRequest++;
      state.layout = null;
    }
  });
  document.addEventListener("pointerover", (event) => {
    const anchor = event.target.closest("[data-trait]");
    if (anchor) showTraitTooltip(anchor);
  });
  document.addEventListener("pointerout", (event) => {
    const anchor = event.target.closest("[data-trait]");
    if (
      anchor &&
      anchor === traitAnchor &&
      !anchor.contains(event.relatedTarget) &&
      !anchor.contains(document.activeElement)
    )
      hideTraitTooltip();
  });
  document.addEventListener("focusin", (event) => {
    const anchor = event.target.closest("[data-trait]");
    if (anchor) showTraitTooltip(anchor);
    else hideTraitTooltip();
  });
  document.addEventListener("focusout", (event) => {
    if (traitAnchor && !traitAnchor.contains(event.relatedTarget)) hideTraitTooltip();
  });
  document.addEventListener("close", hideTraitTooltip, true);
  document.addEventListener("scroll", positionTraitTooltip, true);
  window.addEventListener("resize", positionTraitTooltip);
  const layoutBoard = $("layout-board");
  layoutBoard.addEventListener("wheel", zoomLayout, { passive: false });
  layoutBoard.addEventListener("pointerdown", startLayoutPan);
  layoutBoard.addEventListener("pointermove", moveLayoutPan);
  layoutBoard.addEventListener("pointerup", endLayoutPan);
  layoutBoard.addEventListener("pointercancel", endLayoutPan);
  $("save-svg").addEventListener("click", saveSvg);
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") hideTraitTooltip();
    const card = event.target.closest("[data-library-layout], [data-player-layout]");
    if (
      card &&
      !event.target.closest("[data-trait], [data-retry-card]") &&
      (event.key === "Enter" || event.key === " ")
    ) {
      event.preventDefault();
      card.click();
    }
  });
  document.addEventListener("click", (event) => {
    const retry = event.target.closest("[data-retry-card]");
    if (retry) {
      loadCard(retry.closest(".game-card-visual"));
      return;
    }
    if (!event.target.closest("#recipe-multiselect")) setRecipeOptions(false);
    const libraryCard = event.target.closest("[data-library-layout]");
    if (libraryCard) {
      showLayout(libraryCard.dataset.libraryLayout, null, false);
      return;
    }
    const playerCard = event.target.closest("[data-player-layout]");
    if (playerCard && state.result) {
      const card = state.result.cards[Number(playerCard.dataset.playerLayout)];
      showLayout(card.template, card.materials, false, card);
      return;
    }
    const button = event.target.closest("button");
    if (!button) return;
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
