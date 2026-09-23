import { createHash } from "node:crypto";
import { existsSync, mkdirSync, mkdtempSync, readFileSync, renameSync, statSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

// 只构建静态文件，不运行求解器，不连接 GitHub。Node 22+ 的 rawJSON 保留完整报告中的数值字面量。
const root = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const input = process.argv[2] ? resolve(process.argv[2]) : join(root, "reports/report.json");
if (process.argv.length > 3) throw new Error("用法：node scripts/build-site.mjs [完整结果JSON]");
if (!JSON.rawJSON) throw new Error("网页数据构建需要 Node.js 22 或更新版本。");
const source = readFileSync(input, "utf8").replace(/^\uFEFF/, "");
const report = JSON.parse(source);
const catalog = JSON.parse(readFileSync(join(root, "Calculator/Data/catalog.json"), "utf8"));
const assets = JSON.parse(readFileSync(join(root, "reports/assets.json"), "utf8"));
const craftingUi = JSON.parse(readFileSync(join(root, "reports/crafting-ui.json"), "utf8"));

/** 按网页消费字段显式投影；缺失字段阻止发布，避免生成不完整页面。 */
function pick(value, fields) {
  return Object.fromEntries(
    fields.map((key) => {
      if (!value || !Object.hasOwn(value, key)) throw new Error(`网页数据缺少字段：${key}`);
      return [key, value[key]];
    })
  );
}

/** 比较编号集合，避免只比较数量而漏掉缺失或重复的卡牌、回想。 */
function sameIds(actual, expected) {
  return JSON.stringify([...actual].sort((a, b) => a - b)) === JSON.stringify([...expected].sort((a, b) => a - b));
}

if (
  report.schema !== 14 ||
  craftingUi.schema !== 1 ||
  craftingUi.catalog_id !== catalog.id ||
  report.catalog.id !== catalog.id ||
  !sameIds(
    report.catalog.recipes.map((r) => r.id),
    catalog.recipes.map((r) => r.Id.Value)
  ) ||
  !sameIds(
    report.catalog.encounters.map((e) => e.id),
    catalog.encounters.map((e) => e.id)
  )
) {
  throw new Error("结果版本或资源范围不兼容，网页数据未更新。");
}
for (const recipe of report.catalog.recipes) {
  const order = report.recipes[recipe.id]?.ChronologicalIndex;
  if (!Number.isInteger(order) || order < 0) throw new Error(`卡牌 ${recipe.id} 缺少有效的游戏默认顺序。`);
}
for (const encounter of report.catalog.encounters) {
  for (let limit = 0; limit <= 2; limit++) {
    const deck = report.encounters[encounter.id]?.[limit];
    if (
      !deck ||
      deck.library !== report.library ||
      deck.max_strikes !== limit ||
      !deck.battle.passed ||
      deck.cards.length !== 5
    )
      throw new Error(`回想 ${encounter.id} 的罚分 ${limit} 配队不完整。`);
    for (let rating = 1; rating <= 20; rating++) {
      if (!deck.rating_battles[rating]) throw new Error(`回想 ${encounter.id} 缺少等级 ${rating} 的结果。`);
    }
    for (const card of deck.cards) {
      if (!report.templates[card.template]) throw new Error(`缺少配队模板 ${card.template}。`);
    }
  }
}

const published = Object.values(report.templates);
// 旧完整报告没有单独的最终卡库时，保持原网页同卡同结构面板支配筛选的结果。
const library = report.library_templates
  ? Object.values(report.library_templates)
  : published.filter(
      (card) =>
        !published.some(
          (other) =>
            other.id !== card.id &&
            other.recipe === card.recipe &&
            other.slots === card.slots &&
            other.left === card.left &&
            other.right === card.right &&
            other.power >= card.power &&
            other.fortitude >= card.fortitude &&
            (other.power > card.power || other.fortitude > card.fortitude)
        )
    );
const needed = new Set(library.map((card) => card.id));
for (const tiers of Object.values(report.encounters)) {
  for (const deck of Object.values(tiers)) for (const card of deck.cards) needed.add(card.template);
}
const summaryFields = [
  "id",
  "recipe",
  "name",
  "power",
  "fortitude",
  "total",
  "colors",
  "slots",
  "left",
  "right",
  "strikes"
];
const detailFields = [
  "base_power",
  "base_fortitude",
  "retained_percent",
  "penalties",
  "raw_penalties",
  "tier_counts",
  "count",
  "available_traits",
  "goals"
];
mkdirSync(join(root, ".codex"), { recursive: true });
const stage = mkdtempSync(join(root, ".codex/site-stage-"));
mkdirSync(join(stage, "chunks"));
const chunks = new Map();

/** 内容相同的数据共享文件名，使不同罚分档的重复战斗详情可复用缓存。 */
function chunk(value) {
  const text = JSON.stringify(value);
  const hash = createHash("sha256").update(text).digest("hex");
  const path = `chunks/${hash}.json`;
  if (!chunks.has(path)) {
    writeFileSync(join(stage, path), text);
    chunks.set(path, Buffer.byteLength(text));
  }
  return path;
}

const common = chunk({
  traits: report.catalog.traits.map((trait) => ({
    ...pick(trait, ["id", "name", "description", "tier", "icon"]),
    condition: trait.effects[0].TraitActivationCondition
  })),
  shapes: report.catalog.shapes.map((shape) => ({
    id: shape.Id.Value,
    color: shape.Color,
    tier: shape.Tier,
    cells: shape.Segments.map((cell) => [cell.Q, cell.R])
  })),
  strike_names: craftingUi.names.slice(0, 4),
  ...pick(assets, ["trait_icons", "trait_border", "trait_frames", "trait_tiers", "strike_icons"])
});
/** 投影当前拼法的容忍及可用粒子上限；与报告中的净罚分核对，避免重复计算或展示过期规则。 */
function craftingDetails(card, board) {
  const base = craftingUi.character_tolerances[board.Character];
  const capacity = craftingUi.character_capacity[board.Character];
  if (!base || base.length !== 4 || !Number.isInteger(capacity) || capacity < 0)
    throw new Error(`配方 ${card.recipe} 缺少角色制卡上限。`);
  const limits = [...base];
  let particleLimit = Math.min(capacity, board.MaxIota);
  for (const index of card.active) {
    for (const effect of board.BonusAreas[index].BonusEffects) {
      if (effect.EffectType >= 9 && effect.EffectType <= 11)
        limits[effect.EffectType - 8] += effect.Parameters[0].IntValue;
      if (effect.EffectType === 15) particleLimit -= effect.Parameters[0].IntValue;
    }
  }
  if (
    !Number.isInteger(particleLimit) ||
    particleLimit < 0 ||
    Math.max(0, card.count - particleLimit) !== card.raw_penalties[0] ||
    limits.some(
      (limit, index) =>
        !Number.isInteger(limit) ||
        limit < 0 ||
        Math.max(0, card.raw_penalties[index] - limit) !== card.penalties[index]
    )
  )
    throw new Error(`模板 ${card.id} 的罚分容忍与计算结果不一致。`);
  return { strike_tolerances: limits, particle_limit: particleLimit };
}
const recipes = {};
for (const recipe of report.catalog.recipes) {
  const cards = published.filter((card) => card.recipe === recipe.id && needed.has(card.id));
  if (!cards.length) continue;
  const board = report.recipes[recipe.id];
  recipes[recipe.id] = chunk({
    board: {
      cells: board.AllSegments.map((cell) => [cell.Hex.Q, cell.Hex.R]),
      safe: board.SafeSegments.map((cell) => [cell.Hex.Q, cell.Hex.R]),
      targets: board.BonusAreas.flatMap((area) => area.Cells.map((cell) => [cell.Hex.Q, cell.Hex.R, cell.Color]))
    },
    cards: Object.fromEntries(
      cards.map((card) => [
        card.id,
        {
          ...pick(card, detailFields),
          ...craftingDetails(card, board),
          placements: card.placements.map((piece) => pick(piece, ["id", "q", "r", "color", "cells"]))
        }
      ])
    )
  });
}
const encounters = [];
let battleCount = 0;
for (const encounter of report.catalog.encounters) {
  const decks = {};
  const templates = {};
  for (let limit = 0; limit <= 2; limit++) {
    const deck = report.encounters[encounter.id][limit];
    const ratings = {};
    for (let rating = 1; rating <= 20; rating++) {
      const battle = deck.rating_battles[rating];
      ratings[rating] = {
        ...pick(battle, ["score", "offensive_score", "defensive_score", "passed", "enemy_progress"]),
        ...pick(
          battle,
          encounter.mode === "connect"
            ? ["player_remaining", "minimum_hp", "broken", "enemy_remaining"]
            : ["progress", "hp"]
        ),
        details: chunk({
          phases: battle.phases.map((phase) => pick(phase, ["phase", "attack", "defense", "damage", "after"])),
          events: battle.events.map((event) => pick(event, ["at", "side", "trait", "kind", "amount"]))
        })
      };
      battleCount++;
    }
    decks[limit] = {
      cards: deck.cards.map((card) => ({
        ...pick(card, ["template", "color", "traits"]),
        materials: card.materials.map((material) => pick(material, ["piece", "traits", "sources"]))
      })),
      ratings
    };
    for (const card of deck.cards) {
      templates[card.template] = pick(report.templates[card.template], [
        ...summaryFields,
        "inner_structure",
        "outer_structure"
      ]);
    }
  }
  encounters.push({
    ...pick(encounter, ["id", "name", "chapter", "mode"]),
    file: chunk({
      cards: encounter.cards.map((card) =>
        pick(card, ["name", "color", "power", "fortitude", "left", "right", "traits"])
      ),
      drops: encounter.drops,
      decks,
      templates
    })
  });
}
const cardAssets = {
  ...pick(assets.card_assets, ["art", "frames", "stat_icons", "tiers", "levels", "icons", "ranks", "changes"]),
  common: pick(assets.card_assets.common, [
    "art-frame",
    "connector-top",
    "medium-bottom-drawer",
    "medium-range-active",
    "medium-range-inactive",
    "medium-range-center"
  ])
};
const index = {
  schema: 1,
  created: report.created,
  common,
  recipes,
  catalog: {
    // 前端以此数组的顺序作为“卡牌默认顺序”，使用游戏序号而非配方编号。
    recipes: report.catalog.recipes
      .toSorted((a, b) => report.recipes[a.id].ChronologicalIndex - report.recipes[b.id].ChronologicalIndex)
      .map((recipe) => pick(recipe, ["id", "name", "tier", "color", "is_sr"])),
    encounters,
    card_assets: cardAssets,
    trait_border: assets.trait_border
  },
  templates: Object.fromEntries(library.map((card) => [card.id, pick(card, summaryFields)])),
  defaults: pick(report.defaults, ["search_rating"])
};
const indexText = JSON.stringify(index);
writeFileSync(join(stage, "index.json"), indexText);

// 仓库保留完整结果，数值字面量通过 rawJSON 原样写回；运行时页面只拿上面生成的投影。
const repositoryReport = join(root, "reports/report.json");
if (input !== repositoryReport) {
  const readable =
    JSON.stringify(
      JSON.parse(source, (_, value, context) => (typeof value === "number" ? JSON.rawJSON(context.source) : value)),
      null,
      2
    ) + "\n";
  writeFileSync(`${repositoryReport}.${process.pid}.tmp`, readable);
  renameSync(`${repositoryReport}.${process.pid}.tmp`, repositoryReport);
}
const destination = join(root, "docs/data");
const backup = join(root, `.codex/trash/pages-data-${Date.now()}`);
mkdirSync(dirname(backup), { recursive: true });
if (existsSync(destination)) renameSync(destination, backup);
try {
  renameSync(stage, destination);
} catch (error) {
  if (existsSync(backup)) renameSync(backup, destination);
  throw error;
}
const total = Buffer.byteLength(indexText) + [...chunks.values()].reduce((sum, size) => sum + size, 0);
console.log(
  `网页数据已构建：首批 ${(Buffer.byteLength(indexText) / 1024).toFixed(1)} KiB，全部 ${(total / 1024 / 1024).toFixed(2)} MiB。`
);
console.log(
  `${library.length} 张列表卡牌，${Object.keys(recipes).length} 份拼法，${encounters.length} 个回想，${battleCount} 份战斗详情，共 ${chunks.size} 个去重数据块。`
);
console.log(
  `完整报告仅保存在 reports/report.json（${(statSync(repositoryReport).size / 1024 / 1024).toFixed(2)} MiB），未提交或推送。`
);
