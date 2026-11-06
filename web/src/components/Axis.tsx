import { START, pct, ticksFrom } from "../scale";

export function Axis({ from = START }: { from?: number }) {
  return (
    <div className="axis">
      {ticksFrom(from).map((year) => (
        <span key={year} style={{ left: `${pct(year, from)}%` }}>
          {year}
        </span>
      ))}
    </div>
  );
}
