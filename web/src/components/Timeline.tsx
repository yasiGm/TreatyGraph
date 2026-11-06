import { useMemo, useState } from "react";
import { yearFraction } from "../dates";
import { describeRange, type MergedEvent } from "../events";
import { packRows } from "../layout";
import { END, START, pct } from "../scale";
import type { CountryProfile, EventType, Leader } from "../types";
import { Axis } from "./Axis";
import { LeaderDetail, LeaderLane, leaderLanes } from "./LeaderLanes";

const ROWS: { type: EventType; label: string }[] = [
  { type: "alliance", label: "Alliances" },
  { type: "war", label: "Wars" },
  { type: "coalition", label: "Side by side" },
  { type: "diplomacy", label: "Diplomacy" },
];

const ROW_HEIGHT = 20;

interface EventLaneProps {
  label: string;
  events: MergedEvent[];
  selected: MergedEvent | null;
  onSelect: (e: MergedEvent) => void;
  from: number;
  to: number;
}

function EventLane({ label, events, selected, onSelect, from, to }: EventLaneProps) {
  const items = events.map((e) => {
    const start = yearFraction(e.s);
    const end = e.e ? yearFraction(e.e, true) : e.on ? END : start;
    return { e, start, end: Math.max(end, start + (to - from) * 0.01) };
  }).filter((item) => item.end >= from && item.start <= to);
  const { rows, count } = packRows(items);

  return (
    <div className="lane">
      <span className="lane-label">{label}</span>
      <div className="track" style={{ height: count * ROW_HEIGHT + 8 }}>
        {items.length === 0 && <span className="empty">no records</span>}
        {items.map((item, i) => {
          const left = pct(item.start, from, to);
          const width = Math.max(pct(item.end, from, to) - left, 0.6);
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
  // Built once per pair so a clicked leader keeps matching its bar.
  const lanes = useMemo(
    () => [a, b].flatMap((country) => leaderLanes(country).map((lane) => ({ ...lane, country }))),
    [a, b],
  );

  // The axis starts at the earliest year we have any data for either country.
  const from = useMemo(() => {
    const years = [
      ...events.map((e) => yearFraction(e.s)),
      ...[a, b].flatMap((c) => (c.leaders ?? []).map((l) => yearFraction(l.s))),
    ];
    return years.length === 0 ? START : Math.max(START, Math.floor(Math.min(...years)));
  }, [a, b, events]);

  // Zoom: show only a chosen range of years, so very short terms become wide enough to see.
  const [zoom, setZoom] = useState<{ from: number; to: number } | null>(null);
  const viewFrom = zoom ? Math.max(zoom.from, from) : from;
  const viewTo = zoom ? zoom.to : END;
  const years = Array.from({ length: END - Math.floor(from) + 1 }, (_, i) => Math.floor(from) + i);
  const setRange = (f: number, t: number) => {
    if (f === Math.floor(from) && t === END) setZoom(null);
    else setZoom({ from: f, to: t });
  };

  return (
    <div className="card">
      <h2>
        {a.name} ⇄ {b.name} at a glance
      </h2>
      <p className="muted">Click a bar or a leader for details. The axis starts at the first year with data for either country.</p>

      <div className="zoom">
        <span>Zoom to years:</span>
        <select
          aria-label="From year"
          value={Math.floor(viewFrom)}
          onChange={(ev) => {
            const f = Number(ev.target.value);
            setRange(f, Math.max(viewTo, f + 1));
          }}
        >
          {years.filter((y) => y < END).map((y) => (
            <option key={y} value={y}>{y}</option>
          ))}
        </select>
        <span>to</span>
        <select
          aria-label="To year"
          value={viewTo}
          onChange={(ev) => {
            const t = Number(ev.target.value);
            setRange(Math.min(Math.floor(viewFrom), t - 1), t);
          }}
        >
          {years.filter((y) => y > Math.floor(from)).map((y) => (
            <option key={y} value={y}>{y}</option>
          ))}
        </select>
        <button type="button" disabled={!zoom} onClick={() => setZoom(null)}>Show all years</button>
      </div>

      <div className="timeline">
{lanes.map((lane) => (
          <LeaderLane
            key={`${lane.country.c}-${lane.label}`}
            label={lane.label}
            kind={lane.kind}
            country={lane.country}
            leaders={lane.leaders}
            from={viewFrom}
            to={viewTo}
            selected={leader?.leader ?? null}
            onSelect={selectLeader}
          />
        ))}
        {ROWS.map((row) => (
          <EventLane
            key={row.type}
            label={row.label}
            events={events.filter((e) => e.t === row.type)}
            selected={selected}
            onSelect={setSelected}
            from={viewFrom}
            to={viewTo}
          />
        ))}
        <Axis from={viewFrom} to={viewTo} />
      </div>

      {leader && <LeaderDetail leader={leader.leader} country={leader.country} />}

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
        <span><i className="k-state" />head of state / monarch</span>
        <span><i className="k-government" />head of government</span>
        <span><i className="k-both" />leader (one combined list)</span>
      </div>
      <p className="muted small">
        * “ongoing” = still open when that dataset ends (COW alliances 2012, ATOP 2018), not necessarily still in force today.
      </p>

    </div>
  );
}
