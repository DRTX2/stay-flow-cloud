// Token storage + the OAuth2 token response shape. Tokens live in localStorage so a refresh
// survives reloads; a production hardening step would move refresh tokens to an HttpOnly cookie
// via a BFF (see docs/IMPROVEMENTS.md).

export interface TokenSet {
  accessToken: string;
  refreshToken?: string;
  expiresAt: number; // epoch ms
  idToken?: string;
}

const KEY = "stayflow.tokens";

export function loadTokens(): TokenSet | null {
  const raw = localStorage.getItem(KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as TokenSet;
  } catch {
    return null;
  }
}

export function saveTokens(tokens: TokenSet): void {
  localStorage.setItem(KEY, JSON.stringify(tokens));
}

export function clearTokens(): void {
  localStorage.removeItem(KEY);
}

interface RawTokenResponse {
  access_token: string;
  refresh_token?: string;
  expires_in: number;
  id_token?: string;
}

export function toTokenSet(raw: RawTokenResponse): TokenSet {
  return {
    accessToken: raw.access_token,
    refreshToken: raw.refresh_token,
    idToken: raw.id_token,
    expiresAt: Date.now() + raw.expires_in * 1000,
  };
}
