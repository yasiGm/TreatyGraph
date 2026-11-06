import { useEffect, useMemo, useRef, useState } from "react";
import { prettyDate, yearFraction } from "../dates";
import { describeRange, type MergedEvent } from "../events";
import { packRows } from "../layout";
import { END, START, pct } from "../scale";
import type { CountryProfile, EventType, Leader } from "../types";
import { Axis } from "./Axis";

const ROWS: { type: EventType; label: string }[] = [
  { type: "alliance", label: "Alliances" },
  { type: "war", label: "Wars" },
  { type: "coalition", label: "Side by side" },
  { type: "diplomacy", label: "Diplomacy" },
];

const ROW_HEIGHT = 20;

interface LeaderLaneProps {
  country: CountryProfile;
  from: number;
  selected: Leader | null;
  onSelect: (leader: Leader, country: CountryProfile) => void;
}

const MIN_LABEL_PX = 14; // thinner bars get no text
const LINE_PX = 13;      // width of one line of sideways text
const MAX_ONE_LINE = 11; // names up to this many letters (spaces and hyphens not counted) stay on one line

// Names are written sideways with each word on its own line (a hyphen also starts a new line; initials stay with the word before).
// A narrow bar shows as many words as fit, keeping the last ones (the family name) first.
function labelFor(name: string, barPx: number): string {
  if (barPx < MIN_LABEL_PX) return "";
  if (name.replace(/[\s-]/g, "").length <= MAX_ONE_LINE) return name;
  const lines: string[] = [];
  for (const word of name.split(/\s+|(?<=-)/).filter(Boolean)) {
    if (lines.length > 0 && word.length <= 2) lines[lines.length - 1] += " " + word;
    else lines.push(word);
  }
  const fit = Math.max(1, Math.floor((barPx - 4) / LINE_PX));
  return lines.slice(-fit).join("\n");
}

function LeaderLane({ country, from, selected, onSelect }: LeaderLaneProps) {
  const label = `${country.abbr} leaders`;
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

  return (
    <div className="lane">
      <span className="lane-label" title={label}>
        {label}
      </span>
      <div className="track leaders" ref={trackRef}>
        {country.leaders === null ? (
          <span className="empty">no leader data for this country</span>
        ) : (
          country.leaders.map((leader, i) => {
            if (yearFraction(leader.e, true) < from) return null;
            const left = pct(yearFraction(leader.s), from);
            const width = Math.max(pct(yearFraction(leader.e, true), from) - left, 0.2);
            const text = labelFor(leader.name, (width / 100) * trackPx);
            return (
              <button
                key={i}
                type="button"
                className={`leader vertical ${i % 2 ? "l2" : "l1"}${selected === leader ? " active" : ""}`}
                style={{ left: `${left}%`, width: `${width}%` }}
                title={`${leader.name} (${prettyDate(leader.s)} – ${prettyDate(leader.e)}; ended: ${leader.exit})`}
                onClick={() => onSelect(leader, country)}
              >
                {text}
              </button>
            );
          })
        )}
      </div>
    </div>
  );
}

interface EventLaneProps {
  label: string;
  events: MergedEvent[];
  selected: MergedEvent | null;
  onSelect: (e: MergedEvent) => void;
  from: number;
}

function EventLane({ label, events, selected, onSelect, from }: EventLaneProps) {
  const items = events.map((e) => {
    const start = yearFraction(e.s);
    const end = e.e ? yearFraction(e.e, true) : e.on ? END : start;
    return { e, start, end: Math.max(end, start + 2) };
  }).filter((item) => item.end >= from);
  const { rows, count } = packRows(items);

  return (
    <div className="lane">
      <span className="lane-label">{label}</span>
      <div className="track" style={{ height: count * ROW_HEIGHT + 8 }}>
        {items.length === 0 && <span className="empty">no records</span>}
        {items.map((item, i) => {
          const left = pct(item.start, from);
          const width = Math.max(pct(item.end, from) - left, 0.6);
          return (
            <button
              key={i}
              type="button"
              className={`item ${item.e.t}${selected === item.e ? " active" : ""}`}
              style={{ left: `${left}%`, width: `${width}%`, top: 4 + rows[i] * ROW_HEIGHT }}
              title={`${item.e.title} (${describeRange(item.e)})`}
              onClick={() => onSelect(item.e)}
            />
          );
        })}
      </div>
    </div>
  );
}

interface Props {
  a: CountryProfile;
  b: CountryProfile;
  events: MergedEvent[];
}

export function Timeline({ a, b, events }: Props) {
  const [selected, setSelectedEvent] = useState<MergedEvent | null>(null);
  const [leader, setLeader] = useState<{ leader: Leader; country: CountryProfile } | null>(null);
  const setSelected = (e: MergedEvent) => {
    setSelectedEvent(e);
    setLeader(null);
  };
  const selectLeader = (l: Leader, country: CountryProfile) => {
    setLeader({ leader: l, country });
    setSelectedEvent(null);
  };
  // The axis starts at the earliest year we have any data for either country.
  const from = useMemo(() => {
    const years = [
      ...events.map((e) => yearFraction(e.s)),
      ...[a, b].flatMap((c) => (c.leaders ?? []).map((l) => yearFraction(l.s))),
    ];
    return years.length === 0 ? START : Math.max(START, Math.floor(Math.min(...years)));
  }, [a, b, events]);

  return (
    <div className="card">
      <h2>
        {a.name} ⇄ {b.name} at a glance
      </h2>
      <p className="muted">Click a bar or a leader for details. The axis starts at the first year with data for either country.</p>

      <div className="timeline">
        <LeaderLane country={a} from={from} selected={leader?.leader ?? null} onSelect={selectLeader} />
        <LeaderLane country={b} from={from} selected={leader?.leader ?? null} onSelect={selectLeader} />
        {ROWS.map((row) => (
          <EventLane
            key={row.type}
            label={row.label}
            events={events.filter((e) => e.t === row.type)}
            selected={selected}
            onSelect={setSelected}
            from={from}
          />
        ))}
        <Axis from={from} />
      </div>

      {leader && (
        <div className="detail leader-detail">
          <strong>{leader.leader.name}</strong>
          <span>
            {leader.country.name}: {prettyDate(leader.leader.s)} – {prettyDate(leader.leader.e)}
          </span>
          <p>Left office: {leader.leader.exit}.</p>
          <small>Source: Archigos 4.1</small>
        </div>
      )}

      {selected && (
        <div className={`detail ${selected.t}`}>
          <strong>{selected.title}</strong>
          <span>{describeRange(selected)}</span>
          {[...new Set(selected.details)]
            .filter((text) => text)
            .map((text) => (
              <p key={text}>{text}</p>
            ))}
          <small>Source: {[...new Set(selected.srcs)].join(", ")}</small>
        </div>
      )}

      <div className="legend">
        <span><i className="alliance" />alliance</span>
        <span><i className="war" />war against each other</span>
        <span><i className="coalition" />war on the same side</span>
        <span><i className="diplomacy" />diplomatic snapshot</span>
        <span><i className="l1" /><i className="l2" />leaders (alternating)</span>
      </div>
      <p className="muted small">
        * “ongoing” = still open when that dataset ends (COW alliances 2012, ATOP 2018), not necessarily still in force today.
      </p>

    </div>
  );
}
