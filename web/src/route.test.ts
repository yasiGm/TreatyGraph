import { describe, expect, it } from "vitest";
import { DEFAULT_A, DEFAULT_B, countryHash, pairHash, parseHash } from "./route";

describe("parseHash", () => {
  it("falls back to the default pair", () => {
    expect(parseHash("")).toEqual({ kind: "pair", a: DEFAULT_A, b: DEFAULT_B });
  });
  it("reads a pair link", () => {
    expect(parseHash(pairHash(2, 200))).toEqual({ kind: "pair", a: 2, b: 200 });
  });
  it("reads a country link", () => {
    expect(parseHash(countryHash(220))).toEqual({ kind: "country", c: 220 });
  });
});
