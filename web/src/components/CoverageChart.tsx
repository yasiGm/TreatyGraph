import { pct } from "../scale";
import type { CountryProfile, CoverageItem } from "../types";
import { Axis } from "./Axis";

const SHORT: Record<string, string> = {
  "COW Diplomatic Exchange v2006.1": "COW Diplomacy",
  "COW Inter-State Wars v4.0": "COW Wars",
  "COW Alliances v4.1": "COW Alliances",
  "Archigos 4.1 (leaders)": "Archigos (leaders)",
};

interface Props {
  a: CountryProfile;
  b: CountryProfile;
  coverage: CoverageItem[];
}

export function CoverageChart({ a, b, coverage }: Props) {
  const lanes = [
    { label: a.name, spans: a.spans, kind: "state", note: "" },
    { label: b.name, spans: b.spans, kind: "state", note: "" },
    ...coverage.map((c) => ({ label: SHORT[c.src] ?? c.src, spans: [[c.from, c.to]], kind: "data", note: c.note })),
  ];

  return (
    <div className="card">
      <h2>What each source covers</h2>
      <div className="timeline">
        {lanes.map((lane) => (
          <div className="lane" key={lane.label}>
            <span className="lane-label" title={lane.label}>
              {lane.label}
            </span>
            <div className="track thin">
              {lane.spans.map(([start, end], i) => (
                <span
                  key={i}
                  className={`cov ${lane.kind}`}
                  style={{ left: `${pct(start)}%`, width: `${Math.max(pct(end + 1) - pct(start), 0.5)}%` }}
                  title={`${lane.label}: ${start}–${end}${lane.note ? ` — ${lane.note}` : ""}`}
                />
              ))}
            </div>
          </div>
        ))}
        <Axis />
      </div>
      <p className="muted small">
        Grey bars = years each country exists in the COW state system; burgundy bars = years each dataset covers. A gap in the
        timeline outside the burgundy bars means “not measured”, not “quiet”.
      </p>
    </div>
  );
}
