import {
  authorizeEndpoint,
  endSessionEndpoint,
  redirectUri,
  serverConfig,
  tokenEndpoint,
} from "@/server/config";
import type { TokenSet } from "./session";

interface TokenResponse {
  access_token: string;
  refresh_token?: string;
  expires_in?: number;
  token_type?: string;
}

function toTokenSet(json: TokenResponse): TokenSet {
  const expiresIn = json.expires_in ?? 3600;
  return {
    accessToken: json.access_token,
    refreshToken: json.refresh_token,
    expiresAt: Math.floor(Date.now() / 1000) + expiresIn,
  };
}

/** Build the browser-facing /connect/authorize URL for the Authorization Code + PKCE flow. */
export function buildAuthorizeUrl(challenge: string, state: string): string {
  const params = new URLSearchParams({
    client_id: serverConfig.oidc.clientId,
    response_type: "code",
    redirect_uri: redirectUri(),
    scope: serverConfig.oidc.scope,
    code_challenge: challenge,
    code_challenge_method: "S256",
    state,
  });
  return `${authorizeEndpoint()}?${params.toString()}`;
}

/** Exchange an authorization code (server-to-server) for tokens. Public client → no secret. */
export async function exchangeCode(code: string, verifier: string): Promise<TokenSet> {
  const res = await fetch(tokenEndpoint(), {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: new URLSearchParams({
      grant_type: "authorization_code",
      client_id: serverConfig.oidc.clientId,
      code,
      redirect_uri: redirectUri(),
      code_verifier: verifier,
    }),
    cache: "no-store",
  });
  if (!res.ok) {
    const errorText = await res.text().catch(() => "");
    console.error(`Token exchange failed (${res.status}): ${errorText}`);
    throw new Error(`Token exchange failed (${res.status}).`);
  }
  return toTokenSet(await res.json());
}

/** Rotate the access token using the refresh token. */
export async function refreshAccessToken(refreshToken: string): Promise<TokenSet> {
  const res = await fetch(tokenEndpoint(), {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: new URLSearchParams({
      grant_type: "refresh_token",
      client_id: serverConfig.oidc.clientId,
      refresh_token: refreshToken,
    }),
    cache: "no-store",
  });
  if (!res.ok) throw new Error(`Refresh failed (${res.status}).`);
  return toTokenSet(await res.json());
}

/** End-session URL that returns the browser to the site root after sign-out. */
export function buildEndSessionUrl(): string {
  const params = new URLSearchParams({
    post_logout_redirect_uri: `${serverConfig.siteUrl}/`,
  });
  return `${endSessionEndpoint()}?${params.toString()}`;
}
