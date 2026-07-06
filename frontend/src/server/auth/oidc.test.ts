import { describe, expect, it, vi } from "vitest";

describe("oidc helpers", () => {
  it("builds an end-session URL that signs out through the API/IdP and returns to the web app", async () => {
    vi.resetModules();
    vi.stubEnv("NEXT_PUBLIC_OIDC_AUTHORITY", "https://api.example.test");
    vi.stubEnv("NEXT_PUBLIC_SITE_URL", "https://web.example.test");
    vi.stubEnv("OIDC_CLIENT_ID", "stayflow-spa");

    const { buildEndSessionUrl } = await import("./oidc");
    const url = new URL(buildEndSessionUrl());

    expect(url.origin).toBe("https://api.example.test");
    expect(url.pathname).toBe("/connect/logout");
    expect(url.searchParams.get("client_id")).toBe("stayflow-spa");
    expect(url.searchParams.get("post_logout_redirect_uri")).toBe(
      "https://web.example.test/",
    );

    vi.unstubAllEnvs();
  });
});
