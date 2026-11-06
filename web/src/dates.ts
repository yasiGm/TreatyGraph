const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

interface DateParts {
  y: number;
  m: number | null;
  d: number | null;
}

// Dates from the API look like "1853", "1853-03" or "1853-03-28".
function parseDate(date: string): DateParts {
  const [y, m, d] = date.split("-").map(Number);
  return { y, m: m || null, d: d || null };
}

export function prettyDate(date: string | null): string {
  if (!date) return "";
  const p = parseDate(date);
  if (!p.m) return String(p.y);
  if (!p.d) return `${MONTHS[p.m - 1]} ${p.y}`;
  return `${p.d} ${MONTHS[p.m - 1]} ${p.y}`;
}

// A date as a number of years, for example 1853.2. Used to place things on the timeline.
export function yearFraction(date: string, isEnd = false): number {
  const p = parseDate(date);
  const m = p.m ?? (isEnd ? 12 : 1);
  const d = p.d ?? (isEnd ? 28 : 1);
  return p.y + (m - 1) / 12 + (d - 1) / 365;
}
