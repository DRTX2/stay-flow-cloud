import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { StatusBadge } from "./StatusBadge";

describe("StatusBadge", () => {
  it("renders the status text", () => {
    render(<StatusBadge status="Confirmed" />);
    expect(screen.getByText("Confirmed")).toBeInTheDocument();
  });

  it("renders an em dash when no status", () => {
    render(<StatusBadge />);
    expect(screen.getByText("—")).toBeInTheDocument();
  });
});
