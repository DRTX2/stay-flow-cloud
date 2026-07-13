import { describe, expect, it } from "vitest";
import { getVisibleNavSections } from "./nav";

describe("dashboard navigation metadata", () => {
  it("groups permitted routes into workflows and removes empty groups", () => {
    const sections = getVisibleNavSections({
      permissions: ["rooms:read", "housekeeping:manage", "reservations:read"],
      roles: ["Housekeeping"],
    });

    expect(sections.map((section) => section.title)).toEqual([
      "Front desk",
      "Property operations",
      "Inventory & billing",
    ]);
    expect(sections.flatMap((section) => section.items.map((item) => item.href))).toEqual(
      [
        "/dashboard/reservations",
        "/dashboard/booking-enquiries",
        "/dashboard/housekeeping",
        "/dashboard/rooms",
        "/dashboard/room-types",
      ],
    );
  });

  it("does not expose administration routes without their claim", () => {
    const sections = getVisibleNavSections({
      permissions: ["analytics:view"],
      roles: ["Manager"],
    });
    const routes = sections.flatMap((section) => section.items.map((item) => item.href));

    expect(routes).toContain("/dashboard");
    expect(routes).not.toContain("/dashboard/staff");
    expect(routes).not.toContain("/dashboard/integrations");
  });

  it("applies role filtering to routes without a permission policy", () => {
    const adminRoutes = getVisibleNavSections({
      permissions: [],
      roles: ["Admin"],
    }).flatMap((section) => section.items.map((item) => item.href));
    const staffRoutes = getVisibleNavSections({
      permissions: [],
      roles: ["Staff"],
    }).flatMap((section) => section.items.map((item) => item.href));

    expect(adminRoutes).toContain("/dashboard/integrations");
    expect(staffRoutes).not.toContain("/dashboard/integrations");
  });
});
