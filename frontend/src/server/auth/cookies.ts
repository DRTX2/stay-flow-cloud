/**
 * Edge-safe cookie helpers for the BFF session. No `next/headers` / `server-only` imports here so
 * this module can be used from middleware (Edge runtime) as well as route handlers and the
 * server-only session module.
 */
export const SESSION = {
  access: "sf_at",
  refresh: "sf_rt",
  expiry: "sf_exp",
  verifier: "sf_pkce",
  state: "sf_state",
  returnTo: "sf_return",
} as const;

export interface TokenSet {
  accessToken: string;
  refreshToken?: string;
  /** Absolute expiry, epoch seconds. */
  expiresAt: number;
}

/** Minimal writable-cookie surface shared by NextResponse.cookies and the cookies() store. */
export interface WritableCookies {
  set(name: string, value: string, options?: Record<string, unknown>): void;
  delete(name: string): void;
}

const isProd = process.env.NODE_ENV === "production";
const REFRESH_MAX_AGE = 60 * 60 * 24 * 30; // 30 days

export function baseCookieOptions(maxAge: number) {
  return {
    httpOnly: true,
    secure: isProd,
    sameSite: "lax" as const,
    path: "/",
    maxAge,
  };
}

/** Persist a freshly issued/rotated token set into httpOnly cookies. */
export function writeTokenCookies(store: WritableCookies, tokens: TokenSet): void {
  const accessMaxAge = Math.max(0, tokens.expiresAt - Math.floor(Date.now() / 1000));
  store.set(SESSION.access, tokens.accessToken, baseCookieOptions(accessMaxAge));
  store.set(SESSION.expiry, String(tokens.expiresAt), baseCookieOptions(REFRESH_MAX_AGE));
  if (tokens.refreshToken) {
    store.set(SESSION.refresh, tokens.refreshToken, baseCookieOptions(REFRESH_MAX_AGE));
  }
}

/** Drop every auth/transient cookie (logout, or failed/expired session). */
export function clearAuthCookies(store: WritableCookies): void {
  for (const name of Object.values(SESSION)) store.delete(name);
}
