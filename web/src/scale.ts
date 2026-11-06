export const START = 1816;
export const END = 2026;
export const TICKS = [1820, 1840, 1860, 1880, 1900, 1920, 1940, 1960, 1980, 2000, 2020];

// Turn a year into a position between 0% and 100%. `from` is the first year on the axis.
export const pct = (year: number, from: number = START, to: number = END): number =>
  ((Math.min(Math.max(year, from), to) - from) / (to - from)) * 100;

// Axis labels inside the visible range: a round step that gives roughly 5-11 labels.
export const ticksFrom = (from: number, to: number = END): number[] => {
  const range = to - from;
  const step = range > 120 ? 20 : range > 60 ? 10 : range > 30 ? 5 : range > 12 ? 2 : 1;
  const ticks: number[] = [];
  for (let t = Math.ceil(from / step) * step; t <= to; t += step) ticks.push(t);
  return ticks;
};
