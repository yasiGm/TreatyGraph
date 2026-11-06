import { END, START, pct, ticksFrom } from "../scale";

export function Axis({ from = START, to = END }: { from?: number; to?: number }) {
  return (
    <div className="axis">
      {ticksFrom(from, to).map((year) => (
        <span key={year} style={{ left: `${pct(year, from, to)}%` }}>
          {year}
        </span>
      ))}
    </div>
  );
}
