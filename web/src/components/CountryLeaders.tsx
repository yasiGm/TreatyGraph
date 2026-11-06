import { useMemo, useState } from "react";
import { yearFraction } from "../dates";
import { START } from "../scale";
import type { CountryProfile, Leader } from "../types";
import { Axis } from "./Axis";
import { LeaderDetail, LeaderLane, leaderLanes } from "./LeaderLanes";

// A country's heads of state (monarchs, presidents) and heads of government on a timeline of their own.
export function CountryLeaders({ country }: { country: CountryProfile }) {
  const [selected, setSelected] = useState<Leader | null>(null);
  const lanes = useMemo(() => leaderLanes(country), [country]);

  const from = useMemo(() => {
    const years = (country.leaders ?? []).map((l) => yearFraction(l.s));
    return years.length === 0 ? START : Math.max(START, Math.floor(Math.min(...years)));
  }, [country]);

  if (country.leaders === null) return null;

  return (
    <div className="card">
      <h2>Kings, presidents and leaders over time</h2>
      <p className="muted">Click a bar for details.</p>
      <div className="timeline">
        {lanes.map((lane) => (
          <LeaderLane
            key={lane.label}
            label={lane.label}
            kind={lane.kind}
            country={country}
            leaders={lane.leaders}
            from={from}
            selected={selected}
            onSelect={(leader) => setSelected(leader)}
          />
        ))}
        <Axis from={from} />
      </div>
      <div className="legend">
        <span><i className="k-state" />head of state / monarch</span>
        <span><i className="k-government" />head of government</span>
        <span><i className="k-both" />leader (one combined list)</span>
      </div>
      {selected && <LeaderDetail leader={selected} country={country} />}
    </div>
  );
}
