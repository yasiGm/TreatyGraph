import { describe, expect, it } from "vitest";
import { packRows } from "./layout";

describe("packRows", () => {
  it("puts spans that do not overlap on the same row", () => {
    const { rows, count } = packRows([{ start: 1900, end: 1910 }, { start: 1920, end: 1930 }]);
    expect(rows).toEqual([0, 0]);
    expect(count).toBe(1);
  });
  it("puts overlapping spans on different rows", () => {
    const { rows, count } = packRows([{ start: 1900, end: 1930 }, { start: 1910, end: 1920 }]);
    expect(rows).toEqual([0, 1]);
    expect(count).toBe(2);
  });
});
