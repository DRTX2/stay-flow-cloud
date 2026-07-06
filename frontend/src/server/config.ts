/**
 * Server-side configuration for the BFF. Values come from environment variables with
 * dev-friendly defaults. Two distinct API URLs matter:
 *  - `apiInternalUrl`  server→API calls (data fetching, token exchange). In containers this is the
 *    service name (e.g. http://stayflow.api:8080).
 *  - `oidcPublicUrl`   browser-facing OAuth endpoints (authorize, end-session) the user is
 *    redirected to, so it must be reachable from the browser.
 */
export const serverConfig = {
  apiInternalUrl: process.env.API_INTERNAL_URL ?? "http://127.0.0.1:8080",
  oidcPublicUrl: process.env.NEXT_PUBLIC_OIDC_AUTHORITY ?? "http://localhost:8080",
  siteUrl: process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000",
  oidc: {
    clientId: process.env.OIDC_CLIENT_ID ?? "stayflow-spa",
    scope:
      process.env.OIDC_SCOPE ?? "openid profile email roles stayflow.api offline_access",
  },
} as const;

export const redirectUri = () => `${serverConfig.siteUrl}/api/auth/callback`;
export const tokenEndpoint = () => `${serverConfig.apiInternalUrl}/connect/token`;
export const authorizeEndpoint = () => `${serverConfig.oidcPublicUrl}/connect/authorize`;
export const endSessionEndpoint = () => `${serverConfig.oidcPublicUrl}/connect/logout`;
