import { expect, test } from "@playwright/test";
import { loginAs } from "./helpers/auth";

test.beforeEach(async ({ page }) => {
  await loginAs(page);
});

test("reservations list renders with a create action", async ({ page }) => {
  await page.goto("/dashboard/reservations");
  await expect(page.getByRole("heading", { name: "Reservations" })).toBeVisible();
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

test("create, then cancel a reservation", async ({ page }) => {
  await page.goto("/dashboard/reservations");

  // Create
  await page.getByRole("button", { name: /new reservation/i }).click();
  const dialog = page.getByRole("dialog");
  await dialog.getByLabel("Guest").click();
  await page.getByRole("option").first().click();
  await dialog.getByLabel("Room").click();
  await page.getByRole("option").first().click();
  await dialog.getByLabel("Check-in").fill("2026-08-01");
  await dialog.getByLabel("Check-out").fill("2026-08-03");
  await dialog.getByLabel("Guests").fill("2");
  await dialog.getByRole("button", { name: "Create", exact: true }).click();
  await expect(page.getByText(/reservation created/i)).toBeVisible();

  // Cancel the first row
  await page
    .getByRole("button", { name: /open menu/i })
    .first()
    .click();
  await page.getByRole("menuitem", { name: /cancel/i }).click();
  await page.getByRole("button", { name: /cancel reservation/i }).click();
  await expect(page.getByText(/reservation cancelled/i)).toBeVisible();
});

test("generate an invoice from a reservation", async ({ page }) => {
  await page.goto("/dashboard/reservations");
  await page
    .getByRole("button", { name: /open menu/i })
    .first()
    .click();
  await page.getByRole("menuitem", { name: /generate invoice/i }).click();
  await expect(page.getByText(/invoice generated/i)).toBeVisible();
});
