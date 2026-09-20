/* 成果页只查询内嵌数据，不发起求解、网络请求或后台任务。 */
"use strict";
const $=id=>document.getElementById(id);
const DATA=window.PLANNER_REPORT;
const colors={0:"#94a3b0",1:"#ed8585",2:"#e5c65d",3:"#83ce9c",4:"#7caee9",5:"#b499e8",6:"#415362"};
const colorNames={0:"无色",1:"红",2:"黄",3:"绿",4:"蓝",5:"紫"};
const goalNames={power:"攻击最高",fortitude:"防御最高",total:"攻防之和最高",full:"全覆盖代表"};
function goalLabel(goal,card){const parts=String(goal).split(":");const direction=parts.length>1?parts[0]:"";const name=parts.length>1?parts[1]:parts[0];if(direction==="inner")return `内向 · ${goalNames[name]} · 结构分${num(card.inner_structure)}`;if(direction==="outer")return `外向 · ${goalNames[name]} · 结构分${num(card.outer_structure)}`;return goalNames[name]||name;};
const state={recipe:null,group:null,result:null,layout:null,piece:0};
const esc=value=>String(value??"").replace(/[&<>"']/g,c=>({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"}[c]));
const num=(value,digits=0)=>Number(value).toLocaleString("zh-CN",{minimumFractionDigits:digits,maximumFractionDigits:digits});
const plain=text=>String(text||"").replace(/<[^>]*>/g,"");
const palette=mask=>Object.keys(colorNames).map(Number).filter(c=>mask&(1<<c));
const trait=id=>DATA.catalog.traits.find(t=>t.id===id);
const traitName=id=>trait(id)?.name||`特性${id}`;

/** 同一模板的多个最高项合并到一张卡，材料数量采用真实用量。 */
function resultCard(row){
  const card=DATA.templates[row.template],counts=card.tier_counts;
  return `<article class="result-card"><div class="body"><div class="goals">${row.goals.map(g=>`<span class="goal">${goalLabel(g,card)}</span>`).join("")}</div><div class="stat-grid"><div class="raw"><div class="label">原始攻击</div><div class="value">${num(card.base_power)}</div></div><div class="raw"><div class="label">原始防御</div><div class="value">${num(card.base_fortitude)}</div></div><div><div class="label">卡牌攻击</div><div class="value">${num(card.power)}</div></div><div><div class="label">卡牌防御</div><div class="value">${num(card.fortitude)}</div></div></div><div class="result-lines"><div class="result-line"><span>内向结构分</span><span>${num(card.inner_structure)}</span></div><div class="result-line"><span>外向结构分</span><span>${num(card.outer_structure)}</span></div><div class="result-line"><span>卡牌攻防之和</span><span>${num(card.total)}</span></div><div class="result-line"><span>惩罚</span><span class="${card.strikes?"penalty":"zero"}">${card.strikes}次 · 保留${num(card.retained_percent,card.strikes?2:0)}%</span></div><div class="result-line"><span>粒子实际用量</span><span>Ⅰ ${counts[0]} · Ⅱ ${counts[1]} · Ⅲ ${counts[2]}</span></div><div class="result-line"><span>粒子总数</span><span>${card.count}块</span></div></div><button class="full-width" data-layout="${card.id}">查看拼法</button><details><summary>可选择特性 · ${card.available_traits.length}种</summary>${traitList(card)}</details></div></article>`;
}

/** 展示当前材料能提供的技能，并用游戏描述解释触发条件。 */
function traitList(card){
  if(!card.slots)return '<p class="help">没有特性槽。</p>';
  return `<div class="trait-list">${card.available_traits.map(id=>`<span class="trait-item" title="${esc(plain(trait(id)?.description))}">${esc(traitName(id))}</span>`).join("")}</div><p class="help" style="margin-top:10px">最多装备${card.slots}项；具体组合还须由实际粒子同时提供，配队结果已处理该约束。</p>`;
}

/** 只列出已经证明可制作的确定组合，不把理论奖励组合当作可行结果。 */
function selectRecipe(){
  state.recipe=DATA.recipes[$("recipe-select").value];
  const groups=DATA.groups[$("recipe-select").value];
  $("group-select").innerHTML=groups.map((group,index)=>{
    const [slots,left,right,mask]=group.key;
    const label=group.full_coverage?`全覆盖代表 · ${slots}槽 · ${palette(mask).map(c=>colorNames[c]).join("、")}`:`${slots}槽 · 左${left}右${right} ＋ ${palette(mask).map(c=>colorNames[c]).join("、")}`;
    return `<option value="${index}">${label}</option>`;
  }).join("");
  $("recipe-title").textContent=state.recipe.name;
  $("recipe-count").textContent=groups.some(group=>group.full_coverage)?"1个全覆盖结果":`${groups.length}种实际可行组合`;
  selectGroup();
}

/** 选择槽数与方向固定的类别，只切换已经计算好的三个代表。 */
function selectGroup(){
  const groups=DATA.groups[$("recipe-select").value];
  state.group=groups[Number($("group-select").value)];
  if(!state.group){$("results").innerHTML='<p class="help">没有可制作的组合。</p>';return;}
  const [slots,left,right,mask]=state.group.key;
  $("group-summary").innerHTML=state.group.full_coverage?`全部 BonusAreas 已激活 · ${slots}个特性槽 · 可选颜色 <span class="color-dots">${palette(mask).map(c=>`<span><i class="color-dot" style="background:${colors[c]}"></i>${colorNames[c]}</span>`).join("")}</span>`:`${slots}个特性槽 · ${slots?`左范围${left}，右范围${right}`:"范围不影响战斗"} · 可选颜色 <span class="color-dots">${palette(mask).map(c=>`<span><i class="color-dot" style="background:${colors[c]}"></i>${colorNames[c]}</span>`).join("")}</span>`;
  $("results").innerHTML=state.group.results.map(resultCard).join("");
}

/** 固定五个位置显示卡牌面板、颜色、范围和实际选中的特性。 */
function gameCard(card,slot,player=false){
  const skills=card.traits.filter(id=>id>1);
  return `<article class="game-card" style="--card-color:${colors[card.color]}"><div class="slot">第${slot+1}槽 · ${colorNames[card.color]}</div><h4>${esc(card.name)}</h4><div class="stat"><small>卡牌攻击</small><span>${num(card.power)}</span></div><div class="stat"><small>卡牌防御</small><span>${num(card.fortitude)}</span></div><p class="range">${player&&!card.slots?"0槽 · 范围无效":`左${card.left} · 右${card.right}${player?` · ${card.slots}槽`:""}`} ${player?`· 内向结构${card.inner_structure} · 外向结构${card.outer_structure}`:""}</p><ul>${skills.length?skills.map(id=>`<li title="${esc(plain(trait(id)?.description))}">${esc(traitName(id))}</li>`).join(""):'<li>无特性</li>'}</ul>${player?`<button class="secondary" data-player-layout="${slot}">查看拼法</button>`:""}</article>`;
}

/** 结算条显示真实数值，超过100%的击破进度仍保留数值，只将条形长度封顶。 */
function gauge(caption,value,left,right,enemy=false){
  return `<div class="gauge ${enemy?"enemy":""}"><div class="caption">${esc(caption)}</div><div class="percent">${num(value,2)}%</div><div class="bar"><div class="fill" style="width:${Math.max(0,Math.min(100,value))}%"></div></div><div class="details"><span>${esc(left)}</span><span>${esc(right)}</span></div></div>`;
}

/** 按所选等级读取同一推荐配队的预计算战斗，不发起计算。 */
function selectEncounter(){
  const eid=Number($("encounter-select").value),rating=Number($("encounter-rating").value)||DATA.defaults.search_rating;
  const encounter=DATA.catalog.encounters.find(e=>e.id===eid);
  const result=DATA.encounters[eid];
  if(!result)throw new Error("这个回想没有预计算成果，成果文件不完整。");
  state.result=result;
  const b=result.rating_battles?.[rating]||result.battle;
  $("encounter-rating-value").value=rating;
  $("encounter-rating-value").textContent=rating;
  $("encounter-title").textContent=`${encounter.name} · ${encounter.mode==="connect"?"连接":"映像"}`;
  $("battle-condition").textContent=`观察等级${rating} · 配队搜索等级10 · 联觉开启 · 25个代表性全EXACT键 · 初始HP100`;
  $("battle-status").textContent=b.passed?"可通关":"此推荐尚未通关";
  $("battle-status").className=`badge ${b.passed?"good":"bad"}`;
  $("encounter-score").innerHTML=`<div><span>遭遇总分</span><strong>${num(b.score)}</strong></div><div><span>进攻分</span><strong>${num(b.offensive_score)}</strong></div><div><span>防守分</span><strong>${num(b.defensive_score)}</strong></div>`;
  if(b.mode==="connect"){
    $("settlement").innerHTML=gauge("己方剩余HP",b.player_remaining,`全程最低 ${num(Math.max(0,b.minimum_hp),2)}%`,b.broken[0]?"曾破损":"全程未破损")+gauge("对方击破进度",b.enemy_progress,`剩余HP ${num(b.enemy_remaining,2)}%`,b.broken[1]?`已击破 · 超额 ${num(Math.max(0,b.enemy_progress-100),2)}%`:"尚未击破",true);
  }else{
    $("settlement").innerHTML=gauge("映像净推进",b.progress,"目标 100%",`超过目标 ${num(b.progress-100,2)}%`)+gauge("对方净受伤",b.enemy_progress,`己方净受伤 ${num(100-b.hp[0],2)}%`,"净推进＝对方净受伤－己方净受伤",true);
  }
  $("player-deck").innerHTML=result.cards.map((c,i)=>gameCard({...DATA.templates[c.template],color:c.color,traits:c.traits},i,true)).join("");
  $("enemy-deck").innerHTML=encounter.cards.map((c,i)=>gameCard(c,i)).join("");
  $("phase-table").querySelector("tbody").innerHTML=b.phases.map(p=>`<tr><td>${p.phase}</td><td>${num(p.attack[0])}／${num(p.defense[0])}</td><td>${num(p.attack[1])}／${num(p.defense[1])}</td><td>${num(p.damage[1],2)}%</td><td>${num(p.damage[0],2)}%</td><td>${num(Math.max(0,p.after[0]),2)}%</td><td>${num(100-p.after[1],2)}%</td></tr>`).join("");
  $("materials").innerHTML=result.cards.map((c,i)=>`<div class="materials-group"><strong>第${i+1}槽 · ${esc(DATA.templates[c.template].name)}</strong>${c.materials.length?c.materials.map(m=>`<p>拼图第${m.piece}块：${m.traits.map(t=>esc(traitName(t))).join("、")}<br>可刷来源：${m.sources.map(eid=>esc(DATA.catalog.encounters.find(e=>e.id===eid)?.name||`回想${eid}`)).join("／")}</p>`).join(""):'<p>无需携带特性的粒子。</p>'}</div>`).join("");
  $("events").innerHTML=b.events.length?b.events.map(e=>`<div class="event-row">代表键进度 ${e.at}/25 · ${e.side===0?"己方":"对方"} · ${esc(traitName(e.trait))} · ${e.kind==="heal"?"有效回复":"造成伤害"} ${num(e.amount,4)}%</div>`).join(""):'<p class="help">没有瞬时攻击或回复事件。</p>';
}

/** 展示实际布局，素材来源编号沿用同一份已保存的粒子顺序。 */
function showLayout(identity,assignment=null){
  const card=DATA.templates[identity];
  state.layout=card;state.piece=0;state.assignment=assignment;
  $("layout-goals").textContent=(card.goals||[]).map(g=>goalNames[g]).join(" · ");
  $("layout-title").textContent=card.name;
  $("layout-stats").textContent=`原始攻击 ${num(card.base_power)} · 原始防御 ${num(card.base_fortitude)} ｜ 卡牌攻击 ${num(card.power)} · 卡牌防御 ${num(card.fortitude)} ｜ 惩罚${card.strikes}次，保留${num(card.retained_percent,card.strikes?2:0)}%`;
  $("layout-zoom").value="100";$("layout-flip").checked=false;
  $("layout-traits").innerHTML=traitList(card);
  drawLayout();
  if(!$("layout-dialog").open)$("layout-dialog").showModal();
}

/** 按游戏Q、R坐标绘制平顶六边格，翻转只改变阅读方向。 */
function hexPoints(x,y){
  return Array.from({length:6},(_,i)=>`${x+Math.cos(i*Math.PI/3)},${y+Math.sin(i*Math.PI/3)}`).join(" ");
}

/** 编号、形状小图和原始坐标共同描述拼法，重叠格保留全部粒子编号。 */
function drawLayout(){
  const card=state.layout,recipe=DATA.recipes[card.recipe],flip=$("layout-flip").checked?1:-1;
  const xy=(q,r)=>[1.5*q,flip*Math.sqrt(3)*(r+q/2)];
  const safe=new Set(recipe.SafeSegments.map(c=>`${c.Hex.Q},${c.Hex.R}`));
  const occupied=new Map();
  card.placements.forEach((p,i)=>p.cells.forEach(([q,r])=>{const key=`${q},${r}`;if(!occupied.has(key))occupied.set(key,[]);occupied.get(key).push(i+1);}));
  const relevant=[...recipe.SafeSegments.map(c=>[c.Hex.Q,c.Hex.R]),...card.placements.flatMap(p=>p.cells)].map(([q,r])=>xy(q,r));
  const minX=Math.min(...relevant.map(p=>p[0]))-2,maxX=Math.max(...relevant.map(p=>p[0]))+2,minY=Math.min(...relevant.map(p=>p[1]))-2,maxY=Math.max(...relevant.map(p=>p[1]))+2;
  const targets=new Map();recipe.BonusAreas.forEach(z=>z.Cells.forEach(c=>targets.set(`${c.Hex.Q},${c.Hex.R}`,c.Color)));
  const cells=[];
  for(const raw of recipe.AllSegments){
    const q=raw.Hex.Q,r=raw.Hex.R,key=`${q},${r}`,[x,y]=xy(q,r);
    if(x<minX-1||x>maxX+1||y<minY-1||y>maxY+1)continue;
    const pieces=occupied.get(key)||[],target=targets.get(key),fill=pieces.length?colors[card.placements[pieces[0]-1].color]:target?colors[target]:"#14202b";
    const dim=state.piece&&!pieces.includes(state.piece),high=state.piece&&pieces.includes(state.piece);
    cells.push(`<g class="layout-cell ${dim?"dim":""} ${high?"highlight":""}"><title>Q=${q}, R=${r}${pieces.length?` · 粒子${pieces.join("、")}`:""}</title><polygon points="${hexPoints(x,y)}" fill="${fill}" fill-opacity="${pieces.length ? .86 : target ? .23 : .45}" stroke="${safe.has(key)?"#657b8b":"#2a3b49"}" stroke-width=".045" ${safe.has(key)?"":'stroke-dasharray=".13 .1"'}/>${pieces.length?`<text x="${x}" y="${y+.16}" text-anchor="middle" font-size="${pieces.length>2 ? .34 : .48}" font-family="sans-serif" font-weight="600" fill="#10202a">${pieces.join("/")}</text>`:""}</g>`);
  }
  $("layout-board").innerHTML=`<svg xmlns="http://www.w3.org/2000/svg" viewBox="${minX} ${minY} ${maxX-minX} ${maxY-minY}" style="width:${$("layout-zoom").value}%"><rect x="${minX}" y="${minY}" width="${maxX-minX}" height="${maxY-minY}" fill="#0c151d"/>${cells.join("")}</svg>`;
  $("piece-list").innerHTML=card.placements.map((p,i)=>{
    const shape=DATA.catalog.shapes.find(s=>s.Id.Value===p.id),points=shape.Segments.map(c=>xy(c.Q,c.R));
    const x0=Math.min(...points.map(c=>c[0]))-1.2,y0=Math.min(...points.map(c=>c[1]))-1.2,w=Math.max(...points.map(c=>c[0]))-x0+1.2,h=Math.max(...points.map(c=>c[1]))-y0+1.2;
    const material=state.assignment?.find(m=>m.piece===i+1),skills=material?` · ${material.traits.map(traitName).join("、")}`:"";
    return `<button class="${state.piece===i+1?"selected":""}" data-piece="${i+1}" title="${shape.Tier}阶 · Q=${p.q}, R=${p.r}${esc(skills)}">${i+1}<svg viewBox="${x0} ${y0} ${w} ${h}">${points.map(([x,y])=>`<polygon points="${hexPoints(x,y)}" fill="${colors[p.color]}" stroke="#172933" stroke-width=".08"/>`).join("")}</svg><span>${p.q},${p.r}${material?" · 带特性":""}</span></button>`;
  }).join("");
}

/** 用户主动保存当前拼法，SVG包含全部格子与编号。 */
function saveSvg(){
  const svg=$("layout-board").querySelector("svg");if(!svg)return;
  const content=new XMLSerializer().serializeToString(svg),url=URL.createObjectURL(new Blob([content],{type:"image/svg+xml"})),link=document.createElement("a");
  link.href=url;link.download=`${state.layout.name.replace(/[\\/:*?"<>|]/g,"_")} 拼法.svg`;link.click();setTimeout(()=>URL.revokeObjectURL(url),1500);
}

/** 显示读取成果或保存文件时的可读错误。 */
function toast(message){
  $("toast").textContent=message;$("toast").hidden=false;clearTimeout(toast.timer);toast.timer=setTimeout(()=>{$("toast").hidden=true;},7000);
}

/** 连接纯查询控件；成果页没有请求服务端计算的入口。 */
function boot(){
  if(!DATA){$("notice").hidden=false;$("notice").textContent="这个文件是页面模板，请打开程序已生成的成果网页。";return;}
  $("summary").textContent=`${DATA.catalog.recipes.length}个配方 · ${DATA.meta.feasible_groups}种组合 · ${DATA.catalog.encounters.length}个回想`;
  $("created").textContent=`生成于 ${new Date(DATA.created*1000).toLocaleString("zh-CN")}`;
  $("recipe-select").innerHTML=DATA.catalog.recipes.map(r=>`<option value="${r.id}">${esc(r.name)}</option>`).join("");
  $("encounter-select").innerHTML=DATA.catalog.encounters.map(e=>`<option value="${e.id}">${esc(e.name)} · ${e.mode==="connect"?"连接":"映像"}</option>`).join("");
  document.querySelectorAll(".tab").forEach(button=>button.addEventListener("click",()=>{
    document.querySelectorAll(".tab").forEach(b=>b.classList.toggle("active",b===button));
    document.querySelectorAll(".tab-content").forEach(p=>{p.hidden=p.id!==`${button.dataset.tab}-tab`;});
  }));
  $("recipe-select").addEventListener("change",selectRecipe);$("group-select").addEventListener("change",selectGroup);
  $("encounter-select").addEventListener("change",()=>{try{selectEncounter();}catch(e){toast(e.message);}});
  $("encounter-rating").addEventListener("input",()=>{try{selectEncounter();}catch(e){toast(e.message);}});
  $("close-layout").addEventListener("click",()=>$("layout-dialog").close());
  $("layout-zoom").addEventListener("input",()=>{const svg=$("layout-board").querySelector("svg");if(svg)svg.style.width=`${$("layout-zoom").value}%`;});
  $("layout-flip").addEventListener("change",drawLayout);$("save-svg").addEventListener("click",saveSvg);
  document.addEventListener("click",event=>{
    const button=event.target.closest("button");if(!button)return;
    if(button.dataset.layout)showLayout(button.dataset.layout);
    if(button.dataset.playerLayout!==undefined){const card=state.result.cards[Number(button.dataset.playerLayout)];showLayout(card.template,card.materials);}
    if(button.dataset.piece){state.piece=state.piece===Number(button.dataset.piece)?0:Number(button.dataset.piece);drawLayout();}
  });
  try{selectRecipe();selectEncounter();}catch(error){toast(error.message);}
}
document.addEventListener("DOMContentLoaded",boot);
