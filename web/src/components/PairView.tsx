import { countByType, type MergedEvent } from "../events";
import type { CountryProfile, CoverageItem } from "../types";
import { CoverageChart } from "./CoverageChart";
import { EventList } from "./EventList";
import { LeaderList } from "./LeaderList";
import { StatCards } from "./StatCards";
import { Timeline } from "./Timeline";

export interface PairData {
  a: CountryProfile;
  b: CountryProfile;
  events: MergedEvent[];
}

export function PairView({ data, coverage }: { data: PairData; coverage: CoverageItem[] }) {
  const { a, b, events } = data;
  const counts = countByType(events);
  const overlap = a.spans.some(([s1, e1]) => b.spans.some(([s2, e2]) => s1 <= e2 && s2 <= e1));

  const warCoverage = coverage.find((c) => c.src.includes("Inter-State"));
  const warsNote = warCoverage ? `wars against each other (data covers ${warCoverage.from}–${warCoverage.to})` : undefined;

  return (
    <>
      {!overlap && (
        <p className="warn">
          {a.name} and {b.name} were never members of the COW state system at the same time, so no pair records exist.
        </p>
      )}

      <StatCards counts={counts} warsNote={warsNote} />

      {events.length === 0 && overlap && (
        <p className="warn">
          <strong>No records for this pair.</strong> The datasets loaded here contain no alliance, war or diplomatic
          record for them. That does not mean nothing happened.
        </p>
      )}

      <Timeline key={`${a.c}-${b.c}`} a={a} b={b} events={events} />
      <CoverageChart a={a} b={b} coverage={coverage} />
      <EventList events={events} />

      {(a.leaders || b.leaders) && (
        <div className="two">
          <LeaderList country={a} />
          <LeaderList country={b} />
        </div>
      )}
    </>
  );
}
