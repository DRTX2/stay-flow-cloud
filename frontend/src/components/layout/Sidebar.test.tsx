import { fireEvent, render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { Sidebar } from "./Sidebar";

const usePathname = vi.fn();

vi.mock("next/navigation", () => ({ usePathname: () => usePathname() }));

describe("Sidebar", () => {
  beforeEach(() => usePathname.mockReturnValue("/dashboard/housekeeping"));

  it("exposes accessible, collapsible workflow groups", () => {
    render(
      <Sidebar
        collapsed={false}
        locale="en"
        claims={{
          permissions: ["rooms:read", "housekeeping:manage", "reservations:read"],
          roles: ["Housekeeping"],
        }}
      />,
    );

    expect(
      screen.getByRole("navigation", { name: "Primary navigation" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Property operations" })).toHaveAttribute(
      "aria-expanded",
      "true",
    );
    const frontDesk = screen.getByRole("button", { name: "Front desk" });
    expect(frontDesk).toHaveAttribute("aria-expanded", "false");
    fireEvent.click(frontDesk);
    expect(frontDesk).toHaveAttribute("aria-expanded", "true");
    expect(screen.getByRole("link", { name: "Housekeeping" })).toHaveAttribute(
      "aria-current",
      "page",
    );
    expect(screen.queryByRole("link", { name: "Staff & roles" })).not.toBeInTheDocument();
  });
});
