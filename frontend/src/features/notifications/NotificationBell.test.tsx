import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { NotificationBell } from "./NotificationBell";

describe("NotificationBell", () => {
  beforeEach(() => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({
          unreadCount: 1,
          items: [
            {
              id: "notification-1",
              title: "Reservation created",
              body: "A new reservation is ready for review.",
              type: "reservation",
              link: null,
              createdAtUtc: "2026-07-13T08:00:00Z",
              readAtUtc: null,
            },
          ],
        }),
      }),
    );
  });

  afterEach(() => vi.unstubAllGlobals());

  it("shows unread notifications and marks all as read", async () => {
    const user = userEvent.setup();
    render(<NotificationBell />);

    await user.click(
      await screen.findByRole("button", { name: "Notifications, 1 unread" }),
    );
    expect(await screen.findByText("Reservation created")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Mark all read" }));

    await waitFor(() =>
      expect(fetch).toHaveBeenCalledWith("/api/notifications/read-all", {
        method: "POST",
      }),
    );
  });
});
