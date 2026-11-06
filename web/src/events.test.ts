import { describe, expect, it } from "vitest";
import { countByType, describeRange, mergeEvents, yearOf } from "./events";
import type { TreatyEvent } from "./types";

const make = (over: Partial<TreatyEvent>): TreatyEvent => ({
  t: "alliance",
  src: "COW Alliances v4.1",
  s: "1900",
  e: "1910",
  on: false,
  lc: false,
  k: 0,
  title: "Test Pact",
  detail: "",
  ...over,
});

describe("yearOf", () => {
  it("reads the year from a full date", () => {
    expect(yearOf("1853-03-28")).toBe(1853);
  });
  it("reads the year when only the year is known", () => {
    expect(yearOf("1900")).toBe(1900);
  });
});

describe("mergeEvents", () => {
  it("merges the same event reported by two sources", () => {
    const merged = mergeEvents([make({}), make({ src: "ATOP 5.1" })]);
    expect(merged).toHaveLength(1);
    expect(merged[0].srcs).toEqual(["COW Alliances v4.1", "ATOP 5.1"]);
  });
  it("keeps different events apart", () => {
    const merged = mergeEvents([make({}), make({ title: "Other Pact" })]);
    expect(merged).toHaveLength(2);
  });
  it("marks the merged event as open if any source says so", () => {
    const merged = mergeEvents([make({ e: null }), make({ e: null, src: "ATOP 5.1", on: true })]);
    expect(merged[0].on).toBe(true);
  });
});

describe("countByType", () => {
  it("counts events per type", () => {
    const counts = countByType([make({}), make({ t: "war" }), make({ t: "war" }), make({ t: "diplomacy" })]);
    expect(counts).toEqual({ alliance: 1, war: 2, coalition: 0, diplomacy: 1 });
  });
});

describe("describeRange", () => {
  it("shows a closed range", () => {
    expect(describeRange(make({ s: "1853-03-28", e: "1856" }))).toBe("28 Mar 1853 – 1856");
  });
  it("shows an open range", () => {
    expect(describeRange(make({ e: null, on: true }))).toBe("1900 – ongoing*");
  });
  it("shows a diplomacy snapshot", () => {
    expect(describeRange(make({ t: "diplomacy", s: "1950", e: null }))).toBe("snapshot 1950");
  });
});
