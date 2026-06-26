import { describe, expect, it } from "vitest";
import { formatDate, humanizeEnum, initials, money, number, percent } from "./format";

describe("format", () => {
  it("formats money and falls back to em dash", () => {
    expect(money(1000)).toContain("1,000");
    expect(money(undefined)).toBe("—");
  });

  it("normalizes percent for both 0..1 and 0..100", () => {
    expect(percent(0.5)).toBe("50.0%");
    expect(percent(50)).toBe("50.0%");
    expect(percent(null)).toBe("—");
  });

  it("formats numbers", () => {
    expect(number(1234)).toBe("1,234");
    expect(number(undefined)).toBe("—");
  });

  it("formats invalid dates passthrough", () => {
    expect(formatDate(undefined)).toBe("—");
    expect(formatDate("not-a-date")).toBe("not-a-date");
  });

  it("derives initials", () => {
    expect(initials("Ada Lovelace")).toBe("AL");
    expect(initials(undefined)).toBe("U");
  });

  it("humanizes PascalCase enum names", () => {
    expect(humanizeEnum("FoodAndBeverage")).toBe("Food And Beverage");
    expect(humanizeEnum("Spa")).toBe("Spa");
    expect(humanizeEnum(undefined)).toBe("—");
  });
});
