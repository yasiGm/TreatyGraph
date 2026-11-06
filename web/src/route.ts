// The page address (the part after #) decides what the page shows, so links can be shared.

export type Route = { kind: "pair"; a: number; b: number } | { kind: "country"; c: number };

export const DEFAULT_A = 365; // Russia
export const DEFAULT_B = 630; // Iran

export const pairHash = (a: number, b: number) => `#/pair/${a}/${b}`;
export const countryHash = (c: number) => `#/country/${c}`;

export function parseHash(hash: string): Route {
  const m = hash.match(/^#\/(pair|country)\/(\d+)(?:\/(\d+))?/);
  if (!m) return { kind: "pair", a: DEFAULT_A, b: DEFAULT_B };
  if (m[1] === "country") return { kind: "country", c: Number(m[2]) };
  return { kind: "pair", a: Number(m[2]), b: m[3] ? Number(m[3]) : DEFAULT_B };
}
