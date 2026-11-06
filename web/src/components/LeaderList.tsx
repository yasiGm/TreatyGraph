import { prettyDate } from "../dates";
import type { CountryProfile } from "../types";

export function LeaderList({ country, title }: { country: CountryProfile; title?: string }) {
  return (
    <div className="card">
      <h2>{title ?? `${country.name} — leaders`}</h2>
      {country.leaders === null ? (
        <p className="muted">No leader data for this country in Archigos.</p>
      ) : (
        <>
          <ul className="clean">
            {country.leaders.map((leader, i) => (
              <li key={i}>
                <b>{leader.name}</b>{" "}
                <span className="muted small">
                  {prettyDate(leader.s)} – {prettyDate(leader.e)}
                  {leader.exit && leader.exit !== "Regular" ? ` · ended: ${leader.exit}` : ""}
                </span>
              </li>
            ))}
          </ul>
          <p className="muted small">Source: Archigos 4.1 (Goemans, Gleditsch and Chiozza).</p>
        </>
      )}
    </div>
  );
}
