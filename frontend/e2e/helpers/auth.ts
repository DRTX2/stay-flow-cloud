import type { Page } from "@playwright/test";

const SPA_CLIENT = "stayflow-spa";
const SCOPE = "openid profile email roles stayflow.api offline_access";

/**
 * Seeds an authenticated session by obtaining tokens via the password grant (enabled for the
 * first-party SPA client) and writing them to localStorage — bypassing the interactive
 * OAuth redirect so functional tests stay fast and deterministic.
 */
export async function loginAs(
  page: Page,
  username = "admin@stayflow.local",
  password = "Admin123$",
) {
  // The dev server proxies /connect to the API.
  const res = await page.request.post("/connect/token", {
    form: {
      grant_type: "password",
      client_id: SPA_CLIENT,
      username,
      password,
      scope: SCOPE,
    },
  });
  if (!res.ok()) {
    throw new Error(`Token request failed: ${res.status()} ${await res.text()}`);
  }
  const body = (await res.json()) as {
    access_token: string;
    refresh_token?: string;
    expires_in: number;
  };

  const tokens = {
    accessToken: body.access_token,
    refreshToken: body.refresh_token,
    expiresAt: Date.now() + body.expires_in * 1000,
  };

  await page.addInitScript((value) => {
    window.localStorage.setItem("stayflow.tokens", value);
  }, JSON.stringify(tokens));
}
