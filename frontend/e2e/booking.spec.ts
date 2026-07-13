import { expect, test } from "@playwright/test";

test("public booking page renders and validates required fields", async ({ page }) => {
  await page.goto("/book");
  await expect(page.getByRole("heading", { name: "Book your stay" })).toBeVisible();

  await page.getByRole("button", { name: /request booking/i }).click();
  await expect(page.getByText("Required").first()).toBeVisible();
  await expect(page.getByText("Enter your name")).toBeVisible();
  await expect(page.getByText("Enter a valid email")).toBeVisible();
});

test("public catalog room opens a real availability quote", async ({ page }) => {
  await page.goto("/hotels");
  const hotelLink = page.locator('main a[href^="/hotels/"]').first();
  await expect(hotelLink).toBeVisible();
  await hotelLink.click();

  const bookingLink = page.locator('main a[href^="/book?"]').first();
  await expect(bookingLink).toBeVisible();
  await bookingLink.click();

  const checkIn = new Date();
  checkIn.setUTCDate(checkIn.getUTCDate() + 120);
  const checkOut = new Date(checkIn);
  checkOut.setUTCDate(checkOut.getUTCDate() + 3);
  await page.getByLabel("Check-in").fill(checkIn.toISOString().slice(0, 10));
  await page.getByLabel("Check-out").fill(checkOut.toISOString().slice(0, 10));
  await page.getByRole("button", { name: "Check availability" }).click();

  await expect(page.getByText(/rooms available/i)).toBeVisible();
  await expect(page.getByText(/3 nights/i)).toBeVisible();
});
