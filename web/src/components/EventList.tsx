import { useState } from "react";
import { describeRange, yearOf, type MergedEvent } from "../events";
import type { EventType } from "../types";

const FILTERS: { type: EventType; label: string }[] = [
  { type: "alliance", label: "Alliances" },
  { type: "war", label: "Wars against each other" },
  { type: "coalition", label: "Wars on the same side" },
  { type: "diplomacy", label: "Diplomacy" },
];

export function EventList({ events }: { events: MergedEvent[] }) {
  const [shown, setShown] = useState<Record<EventType, boolean>>({
    alliance: true,
    war: true,
    coalition: true,
    diplomacy: true,
  });

  const visible = events.filter((e) => shown[e.t]);
  let lastDecade = -1;

  return (
    <div className="card">
      <h2>Timeline</h2>
      <div className="filters">
        {FILTERS.map((f) => (
          <button
            key={f.type}
            type="button"
            className={`chip ${f.type}`}
            aria-pressed={shown[f.type]}
            onClick={() => setShown({ ...shown, [f.type]: !shown[f.type] })}
          >
            {f.label}
          </button>
        ))}
      </div>

      {visible.length === 0 && <p className="muted">Nothing to show with the current filters.</p>}

      {visible.map((e, i) => {
        const decade = Math.floor(yearOf(e.s) / 10) * 10;
        const header = decade !== lastDecade ? <div className="decade">{decade}s</div> : null;
        lastDecade = decade;
        return (
          <div key={i}>
            {header}
            <div className={`ev ${e.t}`}>
              <div className="when">
                {describeRange(e)}
                {e.lc && <span className="muted small"> started on or before 1816</span>}
              </div>
              <div>
                <h3>{e.title}</h3>
                <div>
                  {[...new Set(e.srcs)].map((src) => (
                    <span key={src} className="badge">
                      {src}
                    </span>
                  ))}
                  {e.on && <span className="badge on">open at dataset end</span>}
                </div>
                {[...new Set(e.details)]
                  .filter((text) => text)
                  .map((text) => (
                    <p key={text} className="muted small">
                      {text}
                    </p>
                  ))}
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
