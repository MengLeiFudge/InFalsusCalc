/** 原生结算采用五成双舍入；JS的Math.round在半整数处不符合该规则。 */
export function roundEven(value) {
  const low = Math.floor(value);
  return value - low === 0.5 ? low + Math.abs(low % 2) : Math.round(value);
}

/** 按原生单精度舍入与累计余数补偿，将总判定数分配到五阶段。 */
function phaseCounts(notes) {
  let remaining = notes;
  let error = 0;
  return Array.from({ length: 5 }, (_, phase) => {
    const share = remaining / (5 - phase);
    const count = roundEven(Math.fround(share + error));
    error += share - count;
    remaining -= share;
    return count;
  });
}

/** 复现原生等键Array.Sort的分区交换，阶段事件不能改成稳定排序。 */
function sortEvents(queue, low = 0, high = queue.length - 1) {
  while (high - low + 1 > 16) {
    const middle = low + Math.floor((high - low) / 2);
    [queue[middle], queue[high - 1]] = [queue[high - 1], queue[middle]];
    let left = low;
    let right = high - 1;
    while (++left < --right) [queue[left], queue[right]] = [queue[right], queue[left]];
    [queue[left], queue[high - 1]] = [queue[high - 1], queue[left]];
    sortEvents(queue, left + 1, high);
    high = left - 1;
  }
}

/** 固定配队的全EXACT结算，与Calculator/src/Battle.cs保持同一技能、事件及得分规则。
 * 判定数限25–32767，下限为网页允许的最少判定数，上限不超过原生有符号16位阶段索引范围。
 * 每次建立独立状态，不改变推荐卡牌，也不执行配队搜索。
 */
export function calculateBattle(players, encounter, traits, rating, notes) {
  if (!Number.isInteger(notes) || notes < 25 || notes > 32767) throw new Error("总判定数须为25至32767的整数。");
  if (!Number.isInteger(rating) || rating < 1 || rating > 20 || players.length !== 5 || encounter.cards.length !== 5)
    throw new Error("战斗等级或配队数据无效。");
  const decks = [players, encounter.cards];
  const skills = new Map(traits.map((trait) => [trait.id, trait]));
  const effects = [[], []];
  // C# Dictionary删除后按空槽栈复用位置；保留遍历次序，避免浮点求和顺序漂移。
  const modifiers = [[], []];
  const slots = [new Map(), new Map()];
  const freeSlots = [[], []];
  const hp = [100, 100];
  const damage = [0, 0];
  const healing = [0, 0];
  const broken = [false, false];
  const phases = [];
  const events = [];
  const advantage = [0, 3, 4, 5, 1, 2];
  const chromatic = 1 + 0.05 * new Set(players.map((card) => card.color).filter(Boolean)).size;
  let phase = 0;
  let notesDone = 0;
  let minimum = 100;
  for (let side = 0; side < 2; side++) {
    decks[side].forEach((card, owner) => {
      card.traits.forEach((id, traitSlot) => {
        if (id <= 1) return;
        const skill = skills.get(id);
        if (!skill?.effects) throw new Error(`特性${id}缺少战斗参数，请刷新页面。`);
        skill.effects.forEach((effect, index) => {
          const kind = effect.TraitEffect;
          if (![1, 2, 3, 4, 5, 128, 129, 8096, 8097].includes(kind)) throw new Error(`未支持的特性效果${kind}。`);
          effects[side].push({
            owner,
            identity: owner * 2 ** 32 + traitSlot * 2 ** 16 + index + side * 2 ** 48,
            trait: id,
            kind,
            condition: effect.TraitActivationCondition,
            value: effect.Parameter0
          });
        });
      });
    });
  }

  /** 当前阶段的整队攻防、未受特性修正的防御和暴击倍率；色彩隔离按当前对位判断。 */
  function stats() {
    const attack = [0, 0],
      defense = [0, 0],
      plain = [0, 0],
      critical = [0, 0];
    const isolation = [false, false];
    for (let side = 0; side < 2; side++) {
      let pUp = 1,
        fUp = 1,
        pDown = 1,
        fDown = 1,
        crit = -1;
      for (const modifier of modifiers[side]) {
        if (!modifier) continue;
        const { kind, value } = modifier;
        switch (kind) {
          case 1:
            pUp += value * 0.01;
            break;
          case 2:
            fUp += value * 0.01;
            break;
          case 3:
          case 129:
            fDown += value / 100;
            break;
          case 128:
            pDown += value / 100;
            break;
          case 5:
            crit = Math.max(0, crit) + value / 100;
            break;
          case 4:
            isolation[side] ||= decks[1 - side][phase].color !== value;
            break;
        }
      }
      const colorBonus = side === 0 ? chromatic : 1;
      const difficulty = side === 0 ? 1 + rating / 100 : 1;
      decks[side].forEach((card, slot) => {
        const color = card.color !== 0 && advantage[card.color] === decks[1 - side][slot].color ? 1.25 : 1;
        attack[side] += card.power * (pUp / pDown) * colorBonus * color * difficulty;
        defense[side] += card.fortitude * (fUp / fDown) * colorBonus * color * difficulty;
        plain[side] += card.fortitude * colorBonus * color * difficulty;
      });
      critical[side] = crit > 0 ? crit : 5;
    }
    if (isolation[0]) attack[1] = 0;
    if (isolation[1]) attack[0] = 0;
    return { attack, defense, plain, critical };
  }

  /** 伤害按HP百分点累计，击破后仍累计伤害，连接的破损状态不可恢复。 */
  function hurt(target, amount) {
    if (!Number.isFinite(amount) || amount < 0) throw new Error("出现无效伤害。");
    hp[target] -= amount;
    damage[target] += amount;
    if (encounter.mode === "connect" && hp[target] <= 0) broken[target] = true;
    minimum = Math.min(minimum, hp[0]);
  }

  /** 执行瞬时攻击或有效回复，保留原生float百分比常量及技能触发时的攻防。 */
  function trigger(side, effect) {
    let amount = effect.value * Math.fround(0.01);
    if (effect.kind === 8096) {
      const values = stats();
      const other = 1 - side;
      if (values.defense[other] <= 0 || values.plain[other] <= 0) throw new Error("额外攻击遇到未核对的零防御条件。");
      amount = (amount * values.attack[side]) / (values.defense[other] / values.plain[other]) / values.plain[other];
      hurt(other, amount);
    } else if (effect.kind === 8097) {
      if (encounter.mode === "connect" && broken[side]) amount = 0;
      else {
        const next = Math.min(100, hp[side] + amount);
        amount = next - hp[side];
        hp[side] = next;
        healing[side] += amount;
      }
    } else throw new Error("阶段触发使用了未知瞬时效果。");
    events.push({ at: notesDone, side, trait: effect.trait, kind: effect.kind === 8096 ? "damage" : "heal", amount });
  }

  /** 按卡槽、阵营、技能槽生成事件，再按原生规则处理进出范围和阶段技能。 */
  function processEvents(current, ending) {
    phase = ending ? Math.min(4, current + 1) : current;
    const effective = ending && current === 4 ? -1 : current;
    const queue = [
      { kind: ending ? 1 : 2, side: 0 },
      { kind: ending ? 1 : 2, side: 1 }
    ];
    for (let slot = 0; slot < 5; slot++) {
      for (let side = 0; side < 2; side++) {
        for (const effect of effects[side]) {
          if (effect.owner !== slot || ![4, 5, 6, 7].includes(effect.condition)) continue;
          const card = decks[side][slot];
          const inside =
            effective >= 0 && Math.max(0, slot - card.left) <= effective && effective <= Math.min(4, slot + card.right);
          const own = effective === slot;
          const enabled =
            effect.condition === 4 ? !own : effect.condition === 5 ? !inside : effect.condition === 6 ? own : inside;
          const target = effect.kind >= 128 ? 1 - side : side;
          const exists = slots[target].has(effect.identity);
          if (enabled && !exists) queue.push({ kind: 4, side: target, effect });
          else if (!enabled && exists) queue.push({ kind: 5, side: target, effect });
        }
      }
    }
    sortEvents(queue);
    for (let index = 0; index < queue.length; index++) {
      const event = queue[index];
      const side = event.side;
      if (event.kind === 1 || event.kind === 2) {
        const before = queue.length;
        for (const effect of effects[side]) {
          const card = decks[side][effect.owner];
          const start = Math.max(0, effect.owner - card.left),
            end = Math.min(4, effect.owner + card.right);
          const condition = effect.condition;
          const fires = ending
            ? (condition === 1 && effect.owner === current) ||
              (condition === 2 && start <= current && current <= end) ||
              (condition === 3 && current === end)
            : (condition === 10 && effect.owner === current) ||
              (condition === 9 && start <= current && current <= end) ||
              (condition === 8 && current === start);
          if (fires) queue.push({ kind: 3, side, effect });
        }
        if (queue.length > before) sortEvents(queue, index + 1);
      } else if (event.kind === 3) trigger(side, event.effect);
      else if (event.kind === 4) {
        const slot = freeSlots[side].pop() ?? modifiers[side].length;
        slots[side].set(event.effect.identity, slot);
        modifiers[side][slot] = event.effect;
      } else if (event.kind === 5) {
        const slot = slots[side].get(event.effect.identity);
        modifiers[side][slot] = null;
        slots[side].delete(event.effect.identity);
        freeSlots[side].push(slot);
      }
    }
    phase = current;
  }

  for (const [current, count] of phaseCounts(notes).entries()) {
    phase = current;
    const before = [...hp],
      damageBefore = [...damage];
    processEvents(current, false);
    const values = stats();
    const charge = Math.max(1, Math.floor((count * 4) / 5));
    const outgoing = (values.attack[0] / (values.defense[1] || 100)) * (100 / notes);
    const incoming = (values.attack[1] / (values.defense[0] || 100)) * (100 / notes);
    for (let note = 0; note < count; note++) {
      hurt(1, outgoing * (note < charge ? 1 : values.critical[0]));
      hurt(0, incoming);
      notesDone++;
    }
    processEvents(current, true);
    phases.push({
      phase: current + 1,
      notes: count,
      charge,
      attack: values.attack,
      defense: values.defense,
      before,
      after: [...hp],
      damage: damage.map((value, side) => value - damageBefore[side])
    });
  }
  const progress = 100 - hp[1] - (100 - hp[0]);
  const reduction = notesDone / notes;
  const modifier = encounter.mode === "reflection" ? 0.5 : 1;
  const offensive = (Math.max(1e-13, damage[1]) / (100 + healing[1]) / reduction) * (modifier * 10000);
  const defensive = (10000 / (Math.max(1e-13, damage[0]) / (100 + healing[0]))) * reduction;
  const score = Math.min(999999999, ((offensive * defensive) / 10000) * reduction);
  if (!Number.isFinite(score)) throw new Error("遭遇分计算结果无效。");
  return {
    score,
    offensive_score: offensive,
    defensive_score: defensive,
    passed: encounter.mode === "reflection" ? progress >= 100 : broken[1] && !broken[0],
    hp,
    minimum_hp: minimum,
    broken,
    progress,
    enemy_progress: 100 - hp[1],
    player_remaining: Math.max(0, hp[0]),
    enemy_remaining: Math.max(0, hp[1]),
    notes,
    phases,
    events
  };
}
