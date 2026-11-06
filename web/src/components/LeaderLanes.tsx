import { useEffect, useRef, useState } from "react";
import { prettyDate, yearFraction } from "../dates";
import { packRows } from "../layout";
import { END, pct } from "../scale";
import type { CountryProfile, Leader } from "../types";

export type LaneKind = "state" | "government" | "both";

interface LeaderLaneProps {
  label: string;
  kind: LaneKind;
  country: CountryProfile;
  leaders: Leader[] | null;
  from: number;
  to?: number;
  selected: Leader | null;
  onSelect: (leader: Leader, country: CountryProfile) => void;
}

const LEADER_ROW_PX = 100;

const MIN_LABEL_PX = 14;  // thinner bars get no text
const LINE_PX = 13;       // width of one line of sideways text
const MAX_ONE_LINE = 11;  // sideways names up to this many letters (spaces and hyphens not counted) stay on one line
const CHAR_PX = 7;        // rough width of one letter
const PAD_PX = 12;

interface BarLabel {
  text: string;
  vertical: boolean;
}

// How to write a name on its bar.
// Heads of state (monarchs, presidents) are always written normally: the whole name on one line, otherwise one word
// per line. Heads of government are always written sideways. A combined list picks whatever fits.
function labelFor(name: string, barPx: number, kind: LaneKind): BarLabel {
  if (barPx < MIN_LABEL_PX) return { text: "", vertical: false };

  const lines: string[] = [];
  for (const word of name.split(/\s+|(?<=-)/).filter(Boolean)) {
    if (lines.length > 0 && word.length <= 2) lines[lines.length - 1] += " " + word;
    else lines.push(word);
  }
  const longest = Math.max(...lines.map((l) => l.length));
  const sideways = (): BarLabel => {
    if (name.replace(/[\s-]/g, "").length <= MAX_ONE_LINE) return { text: name, vertical: true };
    const fit = Math.max(1, Math.floor((barPx - 4) / LINE_PX));
    return { text: lines.slice(-fit).join("\n"), vertical: true };
  };

  if (kind === "government") return sideways();

  if (barPx - PAD_PX >= name.length * CHAR_PX) return { text: name, vertical: false };
  if (lines.length > 1 && barPx - PAD_PX >= longest * CHAR_PX) return { text: lines.join("\n"), vertical: false };
  if (kind === "state") return { text: lines[lines.length - 1], vertical: false };
  return sideways();
}

export function LeaderLane({ label, kind, country, leaders, from, to = END, selected, onSelect }: LeaderLaneProps) {
  const trackRef = useRef<HTMLDivElement>(null);
  const [trackPx, setTrackPx] = useState(1000);

  useEffect(() => {
    const el = trackRef.current;
    if (!el) return;
    const update = () => setTrackPx(el.clientWidth);
    update();
    const observer = new ResizeObserver(update);
    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  // Terms of different offices can overlap in time; overlapping ones go in a second row.
  const visible = (leaders ?? []).filter((l) => yearFraction(l.e, true) >= from && yearFraction(l.s) <= to);
  const { rows, count } = packRows(visible.map((l) => ({ start: yearFraction(l.s), end: yearFraction(l.e, true) })));

  return (
    <div className="lane">
      <span className="lane-label" title={label}>
        {label}
      </span>
      <div className="track leaders" ref={trackRef} style={{ height: Math.max(count, 1) * LEADER_ROW_PX }}>
        {leaders === null ? (
          <span className="empty">no leader data for this country</span>
        ) : (
          visible.map((leader, i) => {
            const left = pct(yearFraction(leader.s), from, to);
            const width = Math.max(pct(yearFraction(leader.e, true), from, to) - left, 0.2);
            const text = labelFor(leader.name, (width / 100) * trackPx, kind);
            return (
              <button
                key={i}
                type="button"
                className={`leader k-${kind}${width < 1 ? " short" : ""}${text.vertical ? " vertical" : ""} ${i % 2 ? "l2" : "l1"}${selected === leader ? " active" : ""}`}
                style={{ left: `${left}%`, width: `${width}%`, top: rows[i] * LEADER_ROW_PX, height: LEADER_ROW_PX }}
                title={`${leader.name} (${prettyDate(leader.s)} – ${prettyDate(leader.e)}; ${leader.exit})`}
                onClick={() => onSelect(leader, country)}
              >
                {text.vertical ? <span className="vtext">{text.text}</span> : text.text}
              </button>
            );
          })
        )}
      </div>
    </div>
  );
}

// Keep one row per lane: when a term starts before the previous one ends, the previous one is cut short.
function oneAfterAnother(leaders: Leader[]): Leader[] {
  const sorted = [...leaders].sort((a, b) => a.s.localeCompare(b.s));
  const result: Leader[] = [];
  for (const next of sorted) {
    const last = result[result.length - 1];
    if (last && next.s < last.e) {
      if (next.s <= last.s) continue;
      result[result.length - 1] = { ...last, e: next.s };
    }
    result.push(next);
  }
  return result;
}

// One lane for a country whose leaders form a single list; two lanes (head of state, head of government)
// when the data tells them apart. An office that is both goes with the head of government.
export function leaderLanes(country: CountryProfile): { label: string; kind: LaneKind; leaders: Leader[] | null }[] {
  const all = country.leaders;
  if (all === null || all.every((l) => l.role === "both")) {
    return [{ label: `${country.abbr} leaders`, kind: "both" as const, leaders: all === null ? null : oneAfterAnother(all) }];
  }
  const lanes = [
    { label: `${country.abbr} head of state`, kind: "state" as const, leaders: oneAfterAnother(all.filter((l) => l.role === "state")) },
    { label: `${country.abbr} head of government`, kind: "government" as const, leaders: oneAfterAnother(all.filter((l) => l.role !== "state")) },
  ];
  return lanes.filter((l) => l.leaders.length > 0);
}


const exitText = (exit: string): string =>
  exit === "In office" ? "Still in office." : exit === "Left office" ? "Left office." : `Left office: ${exit}.`;

const KIND_TEXT: Record<LaneKind, string> = { state: "head of state", government: "head of government", both: "" };

// The panel under the timeline that shows one leader's term.
export function LeaderDetail({ leader, country }: { leader: Leader; country: CountryProfile }) {
  const kind: LaneKind = leader.role === "state" ? "state" : leader.role === "government" ? "government" : "both";
  return (
    <div className={`detail leader-detail k-${kind}`}>
      <strong>{leader.name}</strong>
      <span>
        {country.name}: {prettyDate(leader.s)} – {prettyDate(leader.e)}
        {KIND_TEXT[kind] ? ` · ${KIND_TEXT[kind]}` : ""}
      </span>
      <p>{exitText(leader.exit)}</p>
      <small>Source: {leader.source}</small>
    </div>
  );
}
