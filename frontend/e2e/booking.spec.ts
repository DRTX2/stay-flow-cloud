import { expect, test } from "@playwright/test";

// Public booking smoke — no authentication required, no dependency on mutable backend fixtures.
test("public booking page renders and validates required fields", async ({ page }) => {
  await page.goto("/book?hotel=aurora-grand-barcelona&roomType=rt-bcn-standard");
  await expect(page.getByRole("heading", { name: "Book your stay" })).toBeVisible();

  await page.getByRole("button", { name: /request booking/i }).click();
  await expect(page.getByText("Required").first()).toBeVisible();
  await expect(page.getByText("Enter your name")).toBeVisible();
  await expect(page.getByText("Enter a valid email")).toBeVisible();
});
