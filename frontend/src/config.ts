// Runtime configuration sourced from Vite env vars (see .env.example).
// API_URL empty => same-origin (use the dev proxy in vite.config.ts).
export const config = {
  apiUrl: import.meta.env.VITE_API_URL ?? "",
  oidc: {
    // Empty => same-origin: authorize/token/logout go through the dev proxy (vite.config.ts)
    // or the reverse proxy in front of the SPA, so the browser never makes a cross-origin
    // token request (no CORS needed on the API).
    authority: import.meta.env.VITE_OIDC_AUTHORITY ?? "",
    clientId: import.meta.env.VITE_OIDC_CLIENT_ID ?? "stayflow-spa",
    scope:
      import.meta.env.VITE_OIDC_SCOPE ??
      "openid profile email roles stayflow.api offline_access",
    redirectUri:
      import.meta.env.VITE_OIDC_REDIRECT_URI ?? "http://localhost:5173/callback",
  },
} as const;
