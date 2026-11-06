import { useEffect, useState } from "react";
import { getCountry, getMeta, getPair } from "./api";
import { CountryPicker } from "./components/CountryPicker";
import { CountrySelect } from "./components/CountrySelect";
import { CountryView } from "./components/CountryView";
import { PairView, type PairData } from "./components/PairView";
import { mergeEvents } from "./events";
import { DEFAULT_A, DEFAULT_B, countryHash, pairHash, parseHash, type Route } from "./route";
import type { CountryProfile, Meta } from "./types";

export default function App() {
  const [route, setRoute] = useState<Route>(() => parseHash(window.location.hash));
  const [meta, setMeta] = useState<Meta | null>(null);
  const [pair, setPair] = useState<PairData | null>(null);
  const [profile, setProfile] = useState<CountryProfile | null>(null);
  const [error, setError] = useState<string | null>(null);

  // The address decides the page: follow it when it changes.
  useEffect(() => {
    const onChange = () => setRoute(parseHash(window.location.hash));
    window.addEventListener("hashchange", onChange);
    return () => window.removeEventListener("hashchange", onChange);
  }, []);

  useEffect(() => {
    getMeta()
      .then(setMeta)
      .catch(() => setError("Could not reach the API. If the database was asleep, wait a few seconds and reload."));
  }, []);

  const pairA = route.kind === "pair" ? route.a : null;
  const pairB = route.kind === "pair" ? route.b : null;
  const countryCode = route.kind === "country" ? route.c : null;

  // Pair page data.
  useEffect(() => {
    if (pairA === null || pairB === null || pairA === pairB) {
      setPair(null);
      return;
    }
    let cancelled = false;
    setError(null);
    setPair(null);
    Promise.all([getCountry(pairA), getCountry(pairB), getPair(pairA, pairB)])
      .then(([a, b, events]) => {
        if (!cancelled) setPair({ a, b, events: mergeEvents(events) });
      })
      .catch(() => {
        if (!cancelled) setError("Could not load this pair. Please try again.");
      });
    return () => {
      cancelled = true;
    };
  }, [pairA, pairB]);

  // Country page data.
  useEffect(() => {
    if (countryCode === null) {
      setProfile(null);
      return;
    }
    let cancelled = false;
    setError(null);
    setProfile(null);
    getCountry(countryCode)
      .then((p) => {
        if (!cancelled) setProfile(p);
      })
      .catch(() => {
        if (!cancelled) setError("Could not load this country. Please try again.");
      });
    return () => {
      cancelled = true;
    };
  }, [countryCode]);

  const go = (hash: string) => {
    window.location.hash = hash;
  };

  const currentA = route.kind === "pair" ? route.a : route.c;
  const pairTabHash = route.kind === "pair" ? pairHash(route.a, route.b) : pairHash(route.c, route.c === DEFAULT_B ? DEFAULT_A : DEFAULT_B);

  return (
    <>
      <header className="top">
        <div className="wrap">
          <h1>
            <img className="logo" src="/logo-white.png" alt="TreatyGraph" />
          </h1>
          <nav className="tabs">
            <a href={pairTabHash} className={route.kind === "pair" ? "active" : ""}>
              Pair timeline
            </a>
            <a href={countryHash(currentA)} className={route.kind === "country" ? "active" : ""}>
              Country profile
            </a>
          </nav>
        </div>
      </header>

      <main className="wrap">
        <h2 className="title">Country Relations Explorer</h2>
        <p className="muted">
          Pick two countries and see how their relationship unfolded since 1816: alliances, wars, diplomatic ties and who was in
          charge.
        </p>

        {error && <p className="error">{error}</p>}

        {route.kind === "pair" && meta && (
          <>
            <CountryPicker countries={meta.countries} a={route.a} b={route.b} onChange={(x, y) => go(pairHash(x, y))} />
            {route.a === route.b && <p className="warn">Choose two different countries.</p>}
            {pair && <PairView data={pair} coverage={meta.coverage} />}
            {!pair && !error && route.a !== route.b && <p className="muted">Loading…</p>}
          </>
        )}

        {route.kind === "country" && meta && (
          <>
            <CountrySelect countries={meta.countries} value={route.c} onChange={(c) => go(countryHash(c))} />
            {profile && <CountryView country={profile} />}
            {!profile && !error && <p className="muted">Loading…</p>}
          </>
        )}
      </main>

      <footer className="wrap foot">
        <p>
          <strong>Read this before trusting a blank timeline.</strong> “No records” means the datasets below contain
          nothing for that pair or period. It does not mean nothing happened. Coverage differs by source and several
          sources stop years ago (see the coverage bars on each pair page).
        </p>
        <p>
          Sources: Correlates of War (Formal Alliances v4.1; Inter-State War Data v4.0; Intra-State War Data v4.1;
          Diplomatic Exchange v2006.1; State System Membership v2024), ATOP 5.1 (Leeds et al.), Archigos 4.1 (Goemans,
          Gleditsch &amp; Chiozza), the UCDP/PRIO Armed Conflict Dataset v26.1 (Uppsala Conflict Data Program, CC BY 4.0) and
          Wikidata (CC0) for current heads of state and government and for diplomatic relations, and the Gleditsch &amp; Ward list
          of independent states (1999). Country identity and borders follow COW state-system
          membership. Built for portfolio / non-commercial use. Data are served from SQL Server through an ASP.NET Core API; see
          the README for citations and licence notes.
        </p>
        <p className="credit">
          © {new Date().getFullYear()} Yasaman Gandomi · Code released under the{" "}
          <a href="https://github.com/yasiGm/TreatyGraph/blob/main/LICENSE">AGPL-3.0 licence</a> ·{" "}
          <a href="https://github.com/yasiGm/TreatyGraph">GitHub</a>
        </p>
      </footer>
    </>
  );
}
