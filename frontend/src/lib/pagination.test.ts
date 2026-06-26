import { describe, expect, it } from "vitest";
import { parsePageParams } from "./pagination";

describe("parsePageParams", () => {
  it("applies defaults for empty params", () => {
    expect(parsePageParams({})).toEqual({ page: 1, pageSize: 20, search: undefined });
  });

  it("parses and trims values", () => {
    expect(parsePageParams({ page: "3", pageSize: "50", search: "  ada  " })).toEqual({
      page: 3,
      pageSize: 50,
      search: "ada",
    });
  });

  it("clamps out-of-range and invalid numbers", () => {
    expect(parsePageParams({ page: "0" }).page).toBe(1);
    expect(parsePageParams({ page: "-5" }).page).toBe(1);
    expect(parsePageParams({ pageSize: "9999" }).pageSize).toBe(100);
    expect(parsePageParams({ pageSize: "abc" }).pageSize).toBe(20);
  });

  it("takes the first value when params repeat", () => {
    expect(parsePageParams({ page: ["2", "7"] }).page).toBe(2);
  });

  it("treats blank search as undefined", () => {
    expect(parsePageParams({ search: "   " }).search).toBeUndefined();
  });
});
