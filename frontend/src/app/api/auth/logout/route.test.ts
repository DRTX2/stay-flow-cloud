import { describe, expect, it, vi } from "vitest";
import { SESSION } from "@/server/auth/cookies";

describe("GET /api/auth/logout", () => {
  it("clears local BFF cookies and redirects through the API end-session endpoint", async () => {
    vi.resetModules();
    vi.stubEnv("NEXT_PUBLIC_OIDC_AUTHORITY", "https://api.example.test");
    vi.stubEnv("NEXT_PUBLIC_SITE_URL", "https://web.example.test");
    vi.stubEnv("OIDC_CLIENT_ID", "stayflow-spa");

    const { GET } = await import("./route");
    const response = await GET();
    const location = response.headers.get("location");

    expect(response.status).toBe(307);
    expect(location).toContain("https://api.example.test/connect/logout");
    expect(location).toContain("client_id=stayflow-spa");
    expect(location).toContain(
      "post_logout_redirect_uri=https%3A%2F%2Fweb.example.test%2F",
    );

    const setCookie = response.headers.getSetCookie().join("\n");
    for (const name of Object.values(SESSION)) {
      expect(setCookie).toContain(`${name}=`);
    }
    expect(setCookie).toContain("Expires=Thu, 01 Jan 1970 00:00:00 GMT");

    vi.unstubAllEnvs();
  });
});
