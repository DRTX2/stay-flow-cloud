import { expect, test } from "@playwright/test";

// Public booking flow — no authentication required.
test("public booking enquiry returns a confirmation", async ({ page }) => {
  await page.goto("/book?hotel=aurora-grand-barcelona&roomType=rt-bcn-standard");
  await expect(page.getByRole("heading", { name: "Book your stay" })).toBeVisible();

  await page.getByLabel("Check-in").fill("2026-09-01");
  await page.getByLabel("Check-out").fill("2026-09-04");
  await page.getByLabel("Guests").fill("2");
  await page.getByLabel("Full name").fill("Ada Lovelace");
  await page.getByLabel("Email").fill("ada@example.com");

  await page.getByRole("button", { name: /request booking/i }).click();
  await expect(page.getByText(/request received/i)).toBeVisible();
});
