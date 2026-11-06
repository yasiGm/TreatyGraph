import { prettyDate } from "../dates";
import type { CountryProfile } from "../types";

const ROLE_TEXT = { state: "head of state", government: "head of government", both: "" } as const;

export function LeaderList({ country, title }: { country: CountryProfile; title?: string }) {
  return (
    <div className="card">
      <h2>{title ?? `${country.name} — leaders`}</h2>
      {country.leaders === null ? (
        <p className="muted">No leader data for this country.</p>
      ) : (
        <>
          <ul className="clean">
            {[...country.leaders].sort((x, y) => x.s.localeCompare(y.s)).map((leader, i) => (
              <li key={i} className={`k-${leader.role}`}>
                <b>{leader.name}</b>{" "}
                <span className="muted small">
                  {prettyDate(leader.s)} – {prettyDate(leader.e)}
                  {ROLE_TEXT[leader.role] ? ` · ${ROLE_TEXT[leader.role]}` : ""}
                  {leader.exit && !["Regular", "In office", "Left office"].includes(leader.exit) ? ` · ended: ${leader.exit}` : ""}
                </span>
              </li>
            ))}
          </ul>
          <p className="muted small">
            Sources: {[...new Set(country.leaders.map((l) => l.source))].join(", ")}. Wikidata covers current states only and its history is uneven;
            Archigos 4.1 (Goemans, Gleditsch and Chiozza) fills the earlier years.
          </p>
        </>
      )}
    </div>
  );
}
