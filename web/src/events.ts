import { prettyDate } from "./dates";
import type { TreatyEvent } from "./types";

export interface MergedEvent extends TreatyEvent {
  srcs: string[];    // every source that reported this event
  details: string[]; // the detail text from each source
}

export const yearOf = (date: string): number => parseInt(date.slice(0, 4), 10);

// Several sources often report the same event. Merge them into one row.
export function mergeEvents(events: TreatyEvent[]): MergedEvent[] {
  const seen = new Map<string, MergedEvent>();
  for (const e of events) {
    const key = [e.t, e.title, e.s, e.e ?? "", e.t === "diplomacy" ? e.detail : ""].join("|");
    const existing = seen.get(key);
    if (existing) {
      existing.srcs.push(e.src);
      existing.details.push(e.detail);
      existing.on = existing.on || e.on;
    } else {
      seen.set(key, { ...e, srcs: [e.src], details: [e.detail] });
    }
  }
  return [...seen.values()];
}

export interface Counts {
  alliance: number;
  war: number;
  coalition: number;
  diplomacy: number;
}

export function countByType(events: TreatyEvent[]): Counts {
  const counts: Counts = { alliance: 0, war: 0, coalition: 0, diplomacy: 0 };
  for (const e of events) {
    counts[e.t] += 1;
  }
  return counts;
}

export function describeRange(e: TreatyEvent): string {
  if (e.t === "diplomacy") return `snapshot ${e.s}`;
  const start = (e.lc ? "≤ " : "") + prettyDate(e.s);
  if (e.e) return `${start} – ${prettyDate(e.e)}`;
  return e.on ? `${start} – ongoing*` : start;
}
