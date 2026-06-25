import type { Page } from "@playwright/test";

const SPA_CLIENT = "stayflow-spa";
const SCOPE = "openid profile email roles stayflow.api offline_access";

/**
 * Seeds an authenticated BFF session. Obtains tokens via the password grant (enabled for the
 * first-party SPA client) through the Next proxy, then writes the same httpOnly cookies the
 * /api/auth/callback route would set — so functional tests skip the interactive OAuth redirect.
 */
export async function loginAs(
  page: Page,
  username = "admin@stayflow.local",
  password = "Admin123$",
) {
  // The Next dev server proxies /connect to the API.
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

  const expiresAt = Math.floor(Date.now() / 1000) + body.expires_in;
  await page
    .context()
    .addCookies([
      cookie("sf_at", body.access_token),
      cookie("sf_rt", body.refresh_token ?? ""),
      cookie("sf_exp", String(expiresAt)),
    ]);
}

function cookie(name: string, value: string) {
  return {
    name,
    value,
    domain: "localhost",
    path: "/",
    httpOnly: true,
    sameSite: "Lax" as const,
  };
}
