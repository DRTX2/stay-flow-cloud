import { expect, test } from "@playwright/test";
import { loginAs } from "./helpers/auth";

test.beforeEach(async ({ page }) => {
  await loginAs(page);
});

test("dashboard shows KPI cards and a revenue chart", async ({ page }) => {
  await page.goto("/dashboard");
  await expect(page.getByRole("heading", { name: "Dashboard" })).toBeVisible();

  for (const kpi of ["Revenue (30d)", "Occupancy", "ADR", "RevPAR"]) {
    await expect(page.getByText(kpi, { exact: false }).first()).toBeVisible();
  }

  // Revenue chart card with its view tabs.
  await expect(page.getByRole("tab", { name: "Trend" })).toBeVisible();
});
