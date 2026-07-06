import type { Page } from "@playwright/test";

/** Drives the same Authorization Code + PKCE flow a real browser user follows. */
export async function loginAs(
  page: Page,
  username = process.env.E2E_ADMIN_EMAIL ?? "admin@stayflow.local",
  password = process.env.E2E_ADMIN_PASSWORD ?? "Admin123$",
) {
  await page.goto(`/api/auth/login?redirect=${encodeURIComponent("/dashboard")}`);

  const email = page.getByLabel("Email address");
  if (await email.isVisible({ timeout: 15_000 }).catch(() => false)) {
    await email.fill(username);
    await page.getByLabel("Password").fill(password);
    await page.getByRole("button", { name: /sign in/i }).click();
  }

  await page.waitForURL(/\/dashboard(?:\/)?(?:\?.*)?$/, { timeout: 30_000 });
}
