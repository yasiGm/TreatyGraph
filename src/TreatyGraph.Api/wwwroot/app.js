/* TreatyGraph — static front-end (no build step, no dependencies). */
(() => {
  "use strict";
  const $ = (s) => document.querySelector(s);
  const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
  const Y0 = 1816, Y1 = 2025;
  const state = { meta: null, byCode: {}, dyadCache: {}, countryCache: {}, filters: { alliance: true, war: true, coalition: true, diplomacy: true } };

  const esc = (s) => String(s).replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

  // ---------- date helpers
  function parts(s) { if (!s) return null; const [y, m, d] = s.split("-").map(Number); return { y, m: m || null, d: d || null }; }
  function pretty(s) {
    const p = parts(s); if (!p) return "";
    if (!p.m) return String(p.y);
    if (!p.d) return MONTHS[p.m - 1] + " " + p.y;
    return p.d + " " + MONTHS[p.m - 1] + " " + p.y;
  }
  function frac(s, end) {
    const p = parts(s); const m = p.m || (end ? 12 : 1); const d = p.d || (end ? 28 : 1);
    return p.y + (m - 1) / 12 + (d - 1) / 365;
  }
  function range(ev) {
    if (ev.t === "diplomacy") return "snapshot " + ev.s;
    const start = (ev.lc ? "≤ " : "") + pretty(ev.s);
    if (ev.e) return start + " – " + pretty(ev.e);
    if (ev.on) return start + " – ongoing*";
    return start;
  }

  // ---------- data loading
  async function getJSON(url) {
    const r = await fetch(url);
    if (!r.ok) throw new Error(url + " → " + r.status);
    return r.json();
  }
  // All data comes from the ASP.NET Core API (which reads SQL Server).
  function dyad(a, b) {
    const key = Math.min(a, b) + "/" + Math.max(a, b);
    if (!state.dyadCache[key]) state.dyadCache[key] = getJSON("api/pairs/" + key);
    return state.dyadCache[key];
  }
  function country(c) {
    if (!state.countryCache[c]) state.countryCache[c] = getJSON("api/countries/" + c);
    return state.countryCache[c];
  }

  // merge identical records reported by several sources into one row
  function mergeEvents(evs) {
    const map = new Map(); const out = [];
    for (const e of evs) {
      const k = [e.t, e.title, e.s, e.e || "", e.t === "diplomacy" ? e.detail : ""].join("|");
      if (map.has(k)) { const m = map.get(k); m.srcs.push(e.src); m.details.push(e.detail); if (e.on) m.on = true; }
      else { const m = Object.assign({}, e, { srcs: [e.src], details: [e.detail] }); map.set(k, m); out.push(m); }
    }
    return out;
  }

  // ---------- coverage + swimlane chart
  const W = 1000, PADL = 110, PADR = 12;
  const xs = (y) => PADL + ((Math.min(Math.max(y, Y0), Y1) - Y0) / (Y1 - Y0)) * (W - PADL - PADR);

  function pack(items) { // greedy row assignment for overlapping bars
    const rows = []; const res = [];
    items.sort((a, b) => a.x1 - b.x1);
    for (const it of items) {
      let r = rows.findIndex((end) => end <= it.x1 + 0.5);
      if (r < 0) { r = rows.length; rows.push(0); }
      rows[r] = it.x2; res.push(Object.assign(it, { row: r }));
    }
    return { res, n: Math.max(1, rows.length) };
  }

  function axis(y, h) {
    let g = "";
    for (let yr = 1820; yr <= 2020; yr += 20) {
      g += `<line x1="${xs(yr)}" x2="${xs(yr)}" y1="${y}" y2="${y + h}" stroke="var(--line)"/><text x="${xs(yr)}" y="${y + h + 12}" text-anchor="middle">${yr}</text>`;
    }
    return g;
  }

  function coverageChart(a, b, cov) {
    const rowH = 16; const h = cov.length * rowH + 2 * rowH; let y = 8; let g = "";
    const lanes = [];
    for (const c of [a, b]) lanes.push({ label: c.name, spans: c.spans.map(([s, e]) => [s, e]), color: "var(--ink)", op: 0.35 });
    const SHORT = { "COW Diplomatic Exchange v2006.1": "COW Diplomacy", "COW Inter-State Wars v4.0": "COW Wars", "COW Alliances v4.1": "COW Alliances", "Archigos 4.1 (leaders)": "Archigos (leaders)" };
    for (const c of cov) lanes.push({ label: SHORT[c.src] || c.src, spans: [[c.from, c.to]], color: "var(--accent)", op: 0.55, note: c.note });
    g += axis(0, lanes.length * rowH + 4);
    lanes.forEach((l, i) => {
      const yy = 4 + i * rowH;
      g += `<text x="${PADL - 8}" y="${yy + 10}" text-anchor="end">${esc(l.label.length > 26 ? l.label.slice(0, 25) + "…" : l.label)}</text>`;
      for (const [s, e] of l.spans) g += `<rect x="${xs(s)}" y="${yy + 2}" width="${Math.max(2, xs(e + 1) - xs(s))}" height="9" rx="2" fill="${l.color}" opacity="${l.op}"><title>${esc(l.label)}: ${s}–${e}${l.note ? " — " + esc(l.note) : ""}</title></rect>`;
    });
    const H = lanes.length * rowH + 22;
    return `<div class="scroll"><svg class="chart" viewBox="0 0 ${W} ${H}" role="img" aria-label="Data coverage by source">${g}</svg></div>`;
  }

  function swimlanes(a, b, la, lb, evs) {
    const lanes = []; let g = ""; let y = 4;
    const leaderLane = (c, list, idx) => {
      if (!list) return `<text class="lane" x="${PADL - 8}" y="${y + 13}" text-anchor="end">${esc(c.abbr)} leaders</text><text x="${PADL + 4}" y="${y + 13}">not loaded yet for this country (pilot countries only)</text>`;
      let s = `<text class="lane" x="${PADL - 8}" y="${y + 13}" text-anchor="end">${esc(c.abbr)} leaders</text>`;
      list.forEach((L, i) => {
        const x1 = xs(frac(L.s)), x2 = xs(frac(L.e, true)); const w = Math.max(1.5, x2 - x1);
        s += `<rect x="${x1}" y="${y}" width="${w}" height="20" fill="var(${i % 2 ? "--lead2" : "--lead1"})" stroke="var(--line)"><title>${esc(L.name)} (${esc(pretty(L.s))} – ${esc(pretty(L.e))}; ended: ${esc(L.exit)})</title></rect>`;
        if (w > L.name.length * 5.2 + 4) s += `<text class="lead" x="${x1 + 3}" y="${y + 13}">${esc(L.name)}</text>`;
      });
      return s;
    };
    g += leaderLane(a, la); y += 26;
    g += leaderLane(b, lb); y += 32;

    const barLane = (label, list, color) => {
      const items = list.map((e) => ({ e, x1: xs(frac(e.s)), x2: e.e ? xs(frac(e.e, true)) : (e.on ? xs(Y1) : xs(frac(e.s)) + 2) }));
      const { res, n } = pack(items);
      let s = `<text class="lane" x="${PADL - 8}" y="${y + 11}" text-anchor="end">${label}</text>`;
      if (!list.length) s += `<text x="${PADL + 4}" y="${y + 11}">no records</text>`;
      for (const it of res) {
        const w = Math.max(3, it.x2 - it.x1);
        s += `<rect x="${it.x1}" y="${y + it.row * 12}" width="${w}" height="9" rx="2" fill="${color}"><title>${esc(it.e.title)} — ${esc(range(it.e))}</title></rect>`;
      }
      y += Math.max(1, n) * 12 + 10;
      return s;
    };
    g += barLane("Alliances", evs.filter((e) => e.t === "alliance"), "var(--alliance)");
    g += barLane("Wars", evs.filter((e) => e.t === "war" || e.t === "coalition"), "var(--war)");
    // diplomacy dots
    const dips = evs.filter((e) => e.t === "diplomacy");
    g += `<text class="lane" x="${PADL - 8}" y="${y + 9}" text-anchor="end">Diplomacy</text>`;
    if (!dips.length) g += `<text x="${PADL + 4}" y="${y + 9}">no records</text>`;
    for (const e of dips) g += `<circle cx="${xs(+e.s)}" cy="${y + 5}" r="4" fill="var(--diplomacy)"><title>${esc(e.title)} — snapshot ${e.s}</title></circle>`;
    y += 22;
    const axisG = axis(0, y - 6);
    return `<div class="scroll"><svg class="chart" viewBox="0 0 ${W} ${y + 6}" role="img" aria-label="Timeline of leaders, alliances, wars and diplomacy">${axisG}${g}</svg></div>`;
  }

  // ---------- pair view
  async function renderPair(ca, cb) {
    const out = $("#pair-out");
    if (ca === cb) { out.innerHTML = `<p class="note">Choose two different countries.</p>`; return; }
    out.innerHTML = `<p class="note">Loading…</p>`;
    let a, b, la, lb, raw;
    try {
      [a, b, raw] = await Promise.all([country(ca), country(cb), dyad(ca, cb)]);
    } catch (err) { out.innerHTML = `<p class="err">Could not load data: ${esc(err.message)}</p>`; return; }
    la = a.leaders; lb = b.leaders;
    const evs = mergeEvents(raw);
    const counts = {
      alliance: evs.filter((e) => e.t === "alliance").length,
      war: evs.filter((e) => e.t === "war").length,
      coalition: evs.filter((e) => e.t === "coalition").length,
      diplomacy: evs.filter((e) => e.t === "diplomacy").length,
    };
    const overlap = a.spans.some(([s1, e1]) => b.spans.some(([s2, e2]) => s1 <= e2 && s2 <= e1));
    let html = "";
    if (!overlap) html += `<div class="warn">${esc(a.name)} and ${esc(b.name)} were never members of the COW state system at the same time, so no dyadic records exist.</div>`;
    html += `<div class="stats">
      <div class="stat"><b>${counts.alliance}</b><span>alliance records</span></div>
      <div class="stat"><b>${counts.war}</b><span>wars against each other</span></div>
      <div class="stat"><b>${counts.coalition}</b><span>wars on the same side</span></div>
      <div class="stat"><b>${counts.diplomacy}</b><span>diplomatic changes seen</span></div></div>`;
    if (!evs.length && overlap) html += `<div class="warn"><strong>No records for this pair.</strong> The datasets loaded here contain no alliance, war or diplomatic-level change involving both countries. That is not the same as “no relationship”: trade, treaties short of alliance, border disputes, and events after each dataset’s end date are not covered.</div>`;

    html += `<div class="card"><h2>${esc(a.name)} ⇄ ${esc(b.name)} at a glance</h2>${swimlanes(a, b, la, lb, evs)}
      <div class="legend"><span><i style="background:var(--alliance)"></i>alliance</span><span><i style="background:var(--war)"></i>war</span><span><i style="background:var(--diplomacy)"></i>diplomatic snapshot</span><span><i style="background:var(--lead1)"></i><i style="background:var(--lead2)"></i>leaders (alternating)</span></div>
      <p class="note">* “ongoing” = still open when that dataset ends (COW alliances 2012, ATOP 2018), not necessarily still in force today.</p></div>`;

    html += `<div class="card"><h2>What each source covers</h2>${coverageChart(a, b, state.meta.coverage)}
      <p class="note">Grey bars = years each country exists in the COW state system; blue bars = years each dataset covers. A gap in the timeline outside the blue bars means “not measured”, not “quiet”.</p></div>`;

    html += `<div class="card"><h2>Timeline</h2><div class="filters" id="filters">
      ${[["alliance", "Alliances"], ["war", "Wars against each other"], ["coalition", "Wars on the same side"], ["diplomacy", "Diplomacy"]]
        .map(([k, l]) => `<button data-k="${k}" aria-pressed="${state.filters[k]}">${l}</button>`).join("")}</div><div id="tl"></div></div>`;

    if (la || lb) {
      html += `<div class="two">${[[a, la], [b, lb]].map(([c, l]) => `<div class="card"><h2>${esc(c.name)} — leaders</h2>${l ? "<ul class='clean'>" + l.map((x) => `<li><b>${esc(x.name)}</b> <span class="note">${esc(pretty(x.s))} – ${esc(pretty(x.e))}${x.exit && x.exit !== "Regular" ? " · " + esc(x.exit) : ""}</span></li>`).join("") + "</ul>" : "<p class='note'>Leader data is only loaded for the pilot countries (Iran, Russia, United Kingdom, Germany, France, USA, Japan, China, Turkey).</p>"}</div>`).join("")}</div>`;
    }
    out.innerHTML = html;

    const drawTimeline = () => {
      const shown = evs.filter((e) => state.filters[e.t]);
      if (!shown.length) { $("#tl").innerHTML = `<p class="note">Nothing to show with the current filters.</p>`; return; }
      let dec = null, h = "";
      for (const e of shown) {
        const d = Math.floor(+e.s.slice(0, 4) / 10) * 10;
        if (d !== dec) { dec = d; h += `<div class="decade">${d}s</div>`; }
        const detail = [...new Set(e.details)].map(esc).join("<br>");
        h += `<div class="ev ${e.t}"><div class="when">${esc(range(e))}${e.lc ? "<br><span class='note'>started on or before 1816</span>" : ""}</div>
          <div><h3>${esc(e.title)}</h3><div>${e.srcs.map((s) => `<span class="badge">${esc(s)}</span>`).join("")}${e.on ? "<span class='badge on'>open at dataset end</span>" : ""}</div><p>${detail}</p></div></div>`;
      }
      $("#tl").innerHTML = h;
    };
    drawTimeline();
    $("#filters").addEventListener("click", (ev) => {
      const k = ev.target.dataset && ev.target.dataset.k; if (!k) return;
      state.filters[k] = !state.filters[k]; ev.target.setAttribute("aria-pressed", state.filters[k]); drawTimeline();
    });
  }

  // ---------- country view
  async function renderCountry(c) {
    const out = $("#country-out"); out.innerHTML = `<p class="note">Loading…</p>`;
    let p; try { p = await country(c); } catch (err) { out.innerHTML = `<p class="err">${esc(err.message)}</p>`; return; }
    const span = p.spans.map(([s, e]) => s + "–" + (e >= 2024 ? "present" : e)).join(", ");
    let h = `<div class="card"><h2>${esc(p.name)}</h2><p class="note">In the COW state system: ${esc(span)} · code ${p.c} (${esc(p.abbr)})</p></div>`;
    h += `<div class="card"><h2>Leaders</h2>${p.leaders ? "<ul class='clean'>" + p.leaders.map((x) => `<li><b>${esc(x.name)}</b> <span class="note">${esc(pretty(x.s))} – ${esc(pretty(x.e))}${x.exit && x.exit !== "Regular" ? " · ended: " + esc(x.exit) : ""}</span></li>`).join("") + "</ul><p class='note'>Source: Archigos 4.1 — the effective leader (e.g. prime minister or de-facto ruler), not always the head of state.</p>" : "<p class='note'>Not loaded yet. Leader data is included only for the pilot countries in this build.</p>"}</div>`;
    h += `<div class="card"><h2>Inter-state wars (${p.wars.length})</h2>${p.wars.length ? "<ul class='clean'>" + p.wars.map((w) => `<li><b>${esc(w.name)}</b> <span class="note">${esc(pretty(w.s))} – ${esc(pretty(w.e))}</span><br><span class="note">${w.result ? esc(w.result) + " · " : ""}${w.initiator ? "initiator · " : ""}vs ${esc(w.vs.join(", ") || "—")}${w.with.length ? " · with " + esc(w.with.join(", ")) : ""}${w.deaths ? " · " + w.deaths.toLocaleString() + " battle deaths" : ""}</span></li>`).join("") + "</ul>" : "<p class='note'>No inter-state wars in COW v4.0.</p>"}</div>`;
    h += `<div class="card"><h2>Civil and internationalised wars (${p.civil.length})</h2>${p.civil.length ? "<ul class='clean'>" + p.civil.map((w) => `<li><b>${esc(w.name)}</b> <span class="note">${esc(pretty(w.s))} – ${esc(pretty(w.e))}</span><br><span class="note">${esc(w.sides)}${w.internationalized ? " · internationalised" : ""}</span></li>`).join("") + "</ul>" : "<p class='note'>None recorded in COW Intra-State War data.</p>"}</div>`;
    h += `<div class="card"><h2>Alliance partners (${p.alliances.length})</h2>${p.alliances.map((a) => `<details><summary>${esc(a.partnerName)} <span class="note">(${a.items.length})</span></summary><ul class="clean">${a.items.map((i) => `<li>${esc(i.title)} <span class="note">${esc(pretty(i.s))} – ${i.e ? esc(pretty(i.e)) : "ongoing*"} · ${esc(i.src)}</span></li>`).join("")}</ul><p><a href="#/pair/${p.c}/${a.partner}">Open the full ${esc(p.abbr)}–${esc(state.byCode[a.partner] ? state.byCode[a.partner].abbr : a.partner)} timeline →</a></p></details>`).join("") || "<p class='note'>No alliance records.</p>"}</div>`;
    out.innerHTML = h;
  }

  // ---------- routing / init
  function fillSelect(sel, value) {
    sel.innerHTML = state.meta.countries.map((c) => `<option value="${c.c}">${esc(c.name)}</option>`).join("");
    sel.value = String(value);
  }
  function go(hash) { if (location.hash !== hash) location.hash = hash; else route(); }
  function route() {
    const m = location.hash.match(/^#\/(pair|country)\/(\d+)(?:\/(\d+))?/);
    const kind = m ? m[1] : "pair";
    const A = m ? +m[2] : 365, B = m && m[3] ? +m[3] : 630;
    $("#tab-pair").setAttribute("aria-selected", kind === "pair"); $("#tab-country").setAttribute("aria-selected", kind === "country");
    $("#view-pair").hidden = kind !== "pair"; $("#view-country").hidden = kind !== "country";
    if (kind === "pair") { $("#selA").value = String(A); $("#selB").value = String(B); renderPair(A, B); }
    else { $("#selC").value = String(A); renderCountry(A); }
  }
  async function init() {
    try { state.meta = await getJSON("api/meta"); } catch (e) { $("#pair-out").innerHTML = `<p class="err">Could not reach the API. Check that SQL Server is running, that the importer has loaded the database, and that the API is started (dotnet run --project src/TreatyGraph.Api).</p>`; return; }
    state.meta.countries.forEach((c) => (state.byCode[c.c] = c));
    fillSelect($("#selA"), 365); fillSelect($("#selB"), 630); fillSelect($("#selC"), 630);
    const pair = () => go(`#/pair/${$("#selA").value}/${$("#selB").value}`);
    $("#selA").onchange = pair; $("#selB").onchange = pair;
    $("#swap").onclick = () => go(`#/pair/${$("#selB").value}/${$("#selA").value}`);
    $("#selC").onchange = () => go(`#/country/${$("#selC").value}`);
    $("#tab-pair").onclick = () => go(`#/pair/${$("#selA").value}/${$("#selB").value}`);
    $("#tab-country").onclick = () => go(`#/country/${$("#selA").value}`);
    window.addEventListener("hashchange", route);
    route();
  }
  init();
})();
