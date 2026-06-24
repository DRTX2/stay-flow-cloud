// Authorization Code + PKCE flow against the OpenIddict endpoints (/connect/authorize, /connect/token).
import { config } from "../config";
import {
  createCodeVerifier,
  createState,
  deriveCodeChallenge,
} from "./pkce";
import { toTokenSet, type TokenSet } from "./tokens";

const VERIFIER_KEY = "stayflow.pkce.verifier";
const STATE_KEY = "stayflow.pkce.state";

function tokenEndpoint(): string {
  return `${config.oidc.authority}/connect/token`;
}

// Builds the /connect/authorize URL and stashes the verifier+state for the callback.
export async function beginLogin(): Promise<string> {
  const verifier = createCodeVerifier();
  const state = createState();
  const challenge = await deriveCodeChallenge(verifier);
  sessionStorage.setItem(VERIFIER_KEY, verifier);
  sessionStorage.setItem(STATE_KEY, state);

  const params = new URLSearchParams({
    client_id: config.oidc.clientId,
    response_type: "code",
    redirect_uri: config.oidc.redirectUri,
    scope: config.oidc.scope,
    code_challenge: challenge,
    code_challenge_method: "S256",
    state,
  });
  return `${config.oidc.authority}/connect/authorize?${params.toString()}`;
}

// Exchanges the authorization code (validated against the saved state) for tokens.
export async function completeLogin(
  code: string,
  returnedState: string,
): Promise<TokenSet> {
  const verifier = sessionStorage.getItem(VERIFIER_KEY);
  const savedState = sessionStorage.getItem(STATE_KEY);
  sessionStorage.removeItem(VERIFIER_KEY);
  sessionStorage.removeItem(STATE_KEY);

  if (!verifier) throw new Error("Missing PKCE verifier; restart sign-in.");
  if (!savedState || savedState !== returnedState)
    throw new Error("State mismatch; possible CSRF. Restart sign-in.");

  const body = new URLSearchParams({
    grant_type: "authorization_code",
    client_id: config.oidc.clientId,
    code,
    redirect_uri: config.oidc.redirectUri,
    code_verifier: verifier,
  });

  const res = await fetch(tokenEndpoint(), {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
  });
  if (!res.ok) throw new Error(`Token exchange failed (${res.status}).`);
  return toTokenSet(await res.json());
}

// Rotates an access token using the refresh token.
export async function refresh(refreshToken: string): Promise<TokenSet> {
  const body = new URLSearchParams({
    grant_type: "refresh_token",
    client_id: config.oidc.clientId,
    refresh_token: refreshToken,
  });
  const res = await fetch(tokenEndpoint(), {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
  });
  if (!res.ok) throw new Error(`Refresh failed (${res.status}).`);
  return toTokenSet(await res.json());
}

export function logoutUrl(): string {
  const params = new URLSearchParams({
    post_logout_redirect_uri: `${window.location.origin}/`,
  });
  return `${config.oidc.authority}/connect/logout?${params.toString()}`;
}
