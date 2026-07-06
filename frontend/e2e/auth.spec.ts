import { expect, test } from "@playwright/test";
import { loginAs } from "./helpers/auth";

test.describe("Authentication", () => {
  test("unauthenticated users are redirected away from the dashboard", async ({
    page,
  }) => {
    await page.goto("/dashboard");
    // The proxy bounces to the BFF login route, which redirects to the OAuth server.
    await expect(page).not.toHaveURL(/\/dashboard$/);
  });

  test("authenticated users reach the dashboard", async ({ page }) => {
    await loginAs(page);
    await page.goto("/dashboard");
    await expect(page.getByRole("heading", { name: "Dashboard" })).toBeVisible();
  });

  test("sign out clears both the BFF session and the IdP session", async ({ page }) => {
    await loginAs(page);
    await page.getByRole("button", { name: "Account menu" }).click();
    await page.getByRole("menuitem", { name: /sign out/i }).click();

    await expect(page).toHaveURL(/\/$/);
    await page.goto("/dashboard");
    await expect(page.getByLabel("Email address")).toBeVisible();
  });
});
