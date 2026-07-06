import { expect, test } from "@playwright/test";
import { loginAs } from "./helpers/auth";

test.beforeEach(async ({ page }) => {
  await loginAs(page);
});

test("reservations list renders with a create action", async ({ page }) => {
  await page.goto("/dashboard/reservations");
  await expect(
    page.getByRole("heading", { name: "Reservations", exact: true }),
  ).toBeVisible();
  await expect(page.getByRole("button", { name: /new reservation/i })).toBeVisible();
});

test("create reservation: validation blocks an empty submit", async ({ page }) => {
  await page.goto("/dashboard/reservations");
  await page.getByRole("button", { name: /new reservation/i }).click();
  await expect(page.getByRole("dialog")).toBeVisible();
  // Submit without choosing anything -> zod validation messages appear.
  await page.getByRole("button", { name: "Create", exact: true }).click();
  await expect(page.getByText(/select a guest/i)).toBeVisible();
});
