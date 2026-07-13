import { describe, expect, it } from "vitest";
import { authenticatedDestination } from "./routing";

describe("authenticatedDestination", () => {
  it("routes customers to the portal and preserves portal return paths", () => {
    expect(authenticatedDestination("/dashboard/reservations", ["Customer"])).toBe(
      "/portal",
    );
    expect(authenticatedDestination("/portal/reservations?tab=past", ["Customer"])).toBe(
      "/portal/reservations?tab=past",
    );
  });

  it("routes staff to the dashboard and preserves dashboard return paths", () => {
    expect(authenticatedDestination("/portal", ["Manager"])).toBe("/dashboard");
    expect(authenticatedDestination("/dashboard/rooms?page=2", ["Manager"])).toBe(
      "/dashboard/rooms?page=2",
    );
  });

  it("rejects external return targets", () => {
    expect(authenticatedDestination("//evil.example", ["Customer"])).toBe("/portal");
  });
});
