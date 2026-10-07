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

/** 与原生Array.Sort一致的内省排序；等键的增减益事件保留原生分区交换顺序。 */
function sortEvents(queue, start = 0) {
  const swap = (a, b) => {
    [queue[a], queue[b]] = [queue[b], queue[a]];
  };
  const swapIfGreater = (a, b) => {
    if (queue[a].priority > queue[b].priority) swap(a, b);
  };
  /** 在low起点的区间内修复最大堆；堆索引从1计数。 */
  function downHeap(index, count, low) {
    const value = queue[low + index - 1];
    while (index <= Math.floor(count / 2)) {
      let child = 2 * index;
      if (child < count && queue[low + child - 1].priority < queue[low + child].priority) child++;
      if (value.priority >= queue[low + child - 1].priority) break;
      queue[low + index - 1] = queue[low + child - 1];
      index = child;
    }
    queue[low + index - 1] = value;
  }
  /** 排序闭区间，深度耗尽时退回堆排序，保持原生交换顺序。 */
  function introsort(low, high, depth) {
    while (high > low) {
      const count = high - low + 1;
      if (count <= 16) {
        if (count === 2) swapIfGreater(low, high);
        else if (count === 3) {
          swapIfGreater(low, high - 1);
          swapIfGreater(low, high);
          swapIfGreater(high - 1, high);
        } else {
          for (let i = low; i < high; i++) {
            const value = queue[i + 1];
            let j = i;
            while (j >= low && value.priority < queue[j].priority) {
              queue[j + 1] = queue[j];
              j--;
            }
            queue[j + 1] = value;
          }
        }
        return;
      }
      if (depth === 0) {
        for (let i = Math.floor(count / 2); i >= 1; i--) downHeap(i, count, low);
        for (let i = count; i > 1; i--) {
          swap(low, low + i - 1);
          downHeap(1, i - 1, low);
        }
        return;
      }
      depth--;
      const middle = low + Math.floor((high - low) / 2);
      swapIfGreater(low, middle);
      swapIfGreater(low, high);
      swapIfGreater(middle, high);
      const pivot = queue[middle].priority;
      swap(middle, high - 1);
      let left = low;
      let right = high - 1;
      while (true) {
        while (queue[++left].priority < pivot) {}
        while (pivot < queue[--right].priority) {}
        if (left >= right) break;
        swap(left, right);
      }
      if (left !== high - 1) swap(left, high - 1);
      introsort(left + 1, high, depth);
      high = left - 1;
    }
  }
  const count = queue.length - start;
  if (count > 1) introsort(start, queue.length - 1, 2 * (Math.floor(Math.log2(count)) + 1));
}

/** 新版_Ph事件排序常量，索引分别为触发条件、当前阶段减来源卡位加5。 */
const conditionPriority = [100000, 110000, 120000, 130000, 510000, 510000, 510000, 510000, 520000, 530000, 540000];
const positionPriority = [4500, 3500, 2500, 1500, 500, 0, 1000, 2000, 3000, 4000, 5000];

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
    return { kind: effect.kind === 8096 ? "damage" : "heal", amount };
  }

  /** 按卡槽、阵营、技能槽生成事件，再按原生规则处理进出范围和阶段技能。 */
  function processEvents(current, ending) {
    phase = ending ? Math.min(4, current + 1) : current;
    const before = { hp: [...hp], ...stats() };
    const firstEvent = events.length;
    const effective = ending && current === 4 ? -1 : current;
    const kind = ending ? 1 : 2;
    const queue = [
      { kind, side: 0, priority: kind * 1000000 },
      { kind, side: 1, priority: kind * 1000000 }
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
          const priority = 3000000 + conditionPriority[effect.condition];
          if (enabled && !exists) queue.push({ kind: 4, side: target, effect, priority });
          else if (!enabled && exists) queue.push({ kind: 5, side: target, effect, priority });
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
          if (fires) {
            const traitSlot = Math.floor(effect.identity / 2 ** 16) % 2 ** 16;
            const effectSlot = effect.identity % 2 ** 16;
            const priority =
              3000000 + conditionPriority[condition] + positionPriority[phase - effect.owner + 5] + side * 100 + traitSlot * 10 + effectSlot;
            queue.push({ kind: 3, side, effect, priority });
          }
        }
        if (queue.length > before) sortEvents(queue, index + 1);
      } else {
        const hpBefore = [...hp];
        let detail;
        if (event.kind === 3) detail = trigger(side, event.effect);
        else if (event.kind === 4) {
          const slot = freeSlots[side].pop() ?? modifiers[side].length;
          slots[side].set(event.effect.identity, slot);
          modifiers[side][slot] = event.effect;
          detail = { kind: "modifier", active: true };
        } else {
          const slot = slots[side].get(event.effect.identity);
          modifiers[side][slot] = null;
          slots[side].delete(event.effect.identity);
          freeSlots[side].push(slot);
          detail = { kind: "modifier", active: false };
        }
        events.push({
          ...detail,
          at: notesDone,
          side: event.effect.identity >= 2 ** 48 ? 1 : 0,
          owner: event.effect.owner,
          trait: event.effect.trait,
          before: hpBefore,
          after: [...hp]
        });
      }
    }
    const node = { at: notesDone, before, after: { hp: [...hp], ...stats() }, events: events.slice(firstEvent) };
    phase = current;
    return node;
  }

  /** 阶段末按已完成判定比例计算当时的累计成绩，保持原生BattleScore运算顺序。 */
  function scores() {
    const reduction = notesDone / notes;
    const modifier = encounter.mode === "reflection" ? 0.5 : 1;
    const offensive = (Math.max(1e-13, damage[1]) / (100 + healing[1]) / reduction) * (modifier * 10000);
    const defensive = (10000 / (Math.max(1e-13, damage[0]) / (100 + healing[0]))) * reduction;
    const score = Math.min(999999999, ((offensive * defensive) / 10000) * reduction);
    if (!Number.isFinite(score)) throw new Error("遭遇分计算结果无效。");
    return { score, offensive_score: offensive, defensive_score: defensive };
  }

  for (const [current, count] of phaseCounts(notes).entries()) {
    phase = current;
    const before = [...hp],
      damageBefore = [...damage];
    const start = processEvents(current, false);
    const values = stats();
    const curve = [{ at: notesDone, hp: [...hp] }];
    const charge = Math.max(1, Math.floor((count * 4) / 5));
    const outgoing = (values.attack[0] / (values.defense[1] || 100)) * (100 / notes);
    const incoming = (values.attack[1] / (values.defense[0] || 100)) * (100 / notes);
    for (let note = 0; note < count; note++) {
      hurt(1, outgoing * (note < charge ? 1 : values.critical[0]));
      hurt(0, incoming);
      notesDone++;
      if (note + 1 === charge) curve.push({ at: notesDone, hp: [...hp] });
    }
    curve.push({ at: notesDone, hp: [...hp] });
    const end = processEvents(current, true);
    phases.push({
      phase: current + 1,
      notes: count,
      charge,
      attack: values.attack,
      defense: values.defense,
      before,
      start,
      end,
      curve,
      scores: scores(),
      after: [...hp],
      damage: damage.map((value, side) => value - damageBefore[side])
    });
  }
  const progress = 100 - hp[1] - (100 - hp[0]);
  return {
    ...scores(),
    mode: encounter.mode,
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
