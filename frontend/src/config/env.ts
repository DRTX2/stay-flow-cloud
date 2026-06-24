// Runtime configuration from Vite env vars (see .env.example).
// Empty apiUrl / authority => same-origin: requests go through the dev proxy (vite.config.ts)
// or the reverse proxy in front of the SPA, so the OAuth2 token exchange needs no CORS.
export const env = {
  apiUrl: import.meta.env.VITE_API_URL ?? "",
  oidc: {
    authority: import.meta.env.VITE_OIDC_AUTHORITY ?? "",
    clientId: import.meta.env.VITE_OIDC_CLIENT_ID ?? "stayflow-spa",
    scope:
      import.meta.env.VITE_OIDC_SCOPE ??
      "openid profile email roles stayflow.api offline_access",
    redirectUri:
      import.meta.env.VITE_OIDC_REDIRECT_URI ?? `${window.location.origin}/callback`,
  },
} as const;
