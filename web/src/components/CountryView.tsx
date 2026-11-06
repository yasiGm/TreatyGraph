import { prettyDate } from "../dates";
import type { CountryProfile } from "../types";
import { CountryLeaders } from "./CountryLeaders";
import { LeaderList } from "./LeaderList";

export function CountryView({ country }: { country: CountryProfile }) {
  const years = country.spans.map(([s, e]) => `${s}–${e >= 2024 ? "present" : e}`).join(", ");

  return (
    <>
      <div className="card">
        <h2>{country.name}</h2>
        <p className="muted">
          In the COW state system: {years} · code {country.c} ({country.abbr})
        </p>
      </div>

      <CountryLeaders country={country} />
      <LeaderList country={country} title="Leaders" />

      <div className="card">
        <h2>Inter-state wars ({country.wars.length})</h2>
        {country.wars.length === 0 ? (
          <p className="muted">No records.</p>
        ) : (
          <ul className="clean">
            {country.wars.map((war, i) => {
              const facts = [
                war.result,
                war.initiator ? "initiator" : null,
                war.vs.length ? `vs ${war.vs.join(", ")}` : null,
                war.with.length ? `with ${war.with.join(", ")}` : null,
                war.deaths !== null ? `${war.deaths.toLocaleString()} battle deaths` : null,
              ].filter(Boolean);
              return (
                <li key={i}>
                  <b>{war.name}</b>{" "}
                  <span className="muted small">
                    {prettyDate(war.s)} – {prettyDate(war.e)}
                  </span>
                  <br />
                  <span className="muted small">{facts.join(" · ")}</span>
                </li>
              );
            })}
          </ul>
        )}
      </div>

      <div className="card">
        <h2>Civil and internationalised wars ({country.civil.length})</h2>
        {country.civil.length === 0 ? (
          <p className="muted">No records.</p>
        ) : (
          <ul className="clean">
            {country.civil.map((war, i) => (
              <li key={i}>
                <b>{war.name}</b>{" "}
                <span className="muted small">
                  {prettyDate(war.s)} – {war.e ? prettyDate(war.e) : "ongoing"}
                </span>
                <br />
                <span className="muted small">
                  {war.sides}
                  {war.internationalized ? " · internationalised" : ""}
                </span>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="card">
        <h2>Alliance partners ({country.alliances.length})</h2>
        {country.alliances.length === 0 && <p className="muted">No records.</p>}
        {country.alliances.map((partner) => (
          <details key={partner.partner}>
            <summary>
              {partner.partnerName} <span className="muted small">({partner.items.length})</span>
            </summary>
            <ul className="clean">
              {partner.items.map((item, i) => (
                <li key={i}>
                  {item.title}{" "}
                  <span className="muted small">
                    {prettyDate(item.s)} – {item.e ? prettyDate(item.e) : item.on ? "ongoing*" : ""} · {item.src}
                  </span>
                </li>
              ))}
            </ul>
          </details>
        ))}
      </div>
    </>
  );
}
