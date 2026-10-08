import { createHash } from "node:crypto";
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

// 只导出网页展示数据；输入为游戏资源的原始JSON树，不修改计算器或完整报告。
const root = resolve(dirname(fileURLToPath(import.meta.url)), "..");
if (process.argv.length !== 4)
  throw new Error("用法：node scripts/extract-loot-ui.mjs GameData.json EncounterDetails.json");
const texts = process.argv.slice(2).map((path) => readFileSync(resolve(path), "utf8"));
const [game, details] = texts.map((text) => JSON.parse(text.replace(/^\uFEFF/, "")));
const catalog = JSON.parse(readFileSync(join(root, "Calculator/Data/catalog.json"), "utf8"));
// 此绑定已核实于以下原生二进制；资源更新后须先确认取池路径，不能沿用旧地址。
const bindingBinary = "e603dc61c6561762d81934c28e694af1d06ec1f7206074310cfce20cecf35993";
if (catalog.binary_hash !== bindingBinary) throw new Error("此游戏版本的技能池绑定尚未核实，请先核对原生代码。");
if (game.CommitId !== catalog.commit || details.CommitId !== catalog.commit)
  throw new Error("掉落资源与当前资源版本不一致。");
const shapes = new Map(catalog.shapes.map((shape) => [shape.Id.Value, shape]));
const traits = new Set(catalog.traits.map((trait) => trait.id));
const encounters = {};

/** 原始权重必须非负且与总数相符，禁止把缺失或损坏的表当作无掉落。 */
function checkedTable(table, label) {
  if (
    !table ||
    !Array.isArray(table.Rows) ||
    !Number.isInteger(table.TotalWeight) ||
    table.TotalWeight <= 0 ||
    table.Rows.some((row) => !Number.isInteger(row.Weight) || row.Weight < 0) ||
    table.Rows.reduce((sum, row) => sum + row.Weight, 0) !== table.TotalWeight
  )
    throw new Error(`${label}的权重不完整。`);
  return table;
}

for (const encounter of catalog.encounters) {
  const raw = details.encounterDetails.filter((item) => item.Id.Value === encounter.id);
  if (raw.length !== 1 || !Number.isInteger(raw[0].BaseRolls) || raw[0].BaseRolls < 0)
    throw new Error(`回想${encounter.id}的基础数量不完整。`);
  const index = raw[0].LootTableIndex;
  if (index !== encounter.loot_table) throw new Error(`回想${encounter.id}的掉落表版本不一致。`);
  const iota = checkedTable(game.lootIotaTables[index], `粒子池${index}`);
  // 当前原生ResultsScene.Start在RVA0x737A6F/0x737A73用回想LootTableIndex取池，
  // 0x7382D5传给_c._d。_d使用传入池，不使用当前资源全为0的行内TraitTableIndex。
  const pool = checkedTable(game.lootTraitTables[index], `技能池${index}`);
  const weights = new Map();
  let empty = 0;
  for (const row of pool.Rows) {
    const id = row.TraitId.Value;
    if (id <= 1) empty += row.Weight;
    else {
      if (!traits.has(id)) throw new Error(`技能池${index}引用未知技能${id}。`);
      weights.set(id, (weights.get(id) || 0) + row.Weight);
    }
  }
  encounters[encounter.id] = {
    table: index,
    base_rolls: raw[0].BaseRolls,
    total_weight: iota.TotalWeight,
    items: iota.Rows.map((row) => {
      const shape = shapes.get(row.IotaId.Value);
      if (
        !shape ||
        !Number.isInteger(row.TraitRolls) ||
        row.TraitRolls < 0 ||
        !Number.isInteger(row.MinPotency) ||
        row.MinPotency < 1 ||
        !Number.isInteger(row.MaxPotency) ||
        row.MaxPotency < row.MinPotency
      )
        throw new Error(`粒子池${index}的配置不完整。`);
      return {
        shape: shape.Id.Value,
        color: shape.Color,
        tier: shape.Tier,
        size: shape.Size,
        weight: row.Weight,
        min_potency: row.MinPotency,
        max_potency: row.MaxPotency,
        trait_rolls: row.TraitRolls,
        trait_pool: index
      };
    }),
    trait_pools: {
      [index]: {
        total_weight: pool.TotalWeight,
        no_trait_weight: empty,
        traits: [...weights].map(([id, weight]) => ({ id, weight }))
      }
    }
  };
}
const snapshot = {
  schema: 1,
  catalog_id: catalog.id,
  source: {
    commit: game.CommitId,
    binary_hash: bindingBinary,
    assets: texts.map((text, i) => ({
      name: ["GameData", "EncounterDetails"][i],
      sha256: createHash("sha256").update(text).digest("hex")
    })),
    pool_binding:
      "ResultsScene.Start RVA0x737A6F/0x737A73、0x7382D5及_c._d RVA0x47E5AE：使用回想LootTableIndex的技能池。"
  },
  encounters
};
writeFileSync(join(root, "reports/loot-ui.json"), JSON.stringify(snapshot, null, 2) + "\n");
console.log(`网页掉落快照已导出：${Object.keys(encounters).length}个回想，包含基础数量与实际技能池。`);
