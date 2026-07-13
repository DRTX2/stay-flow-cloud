import { NextResponse, type NextRequest } from "next/server";
import { SESSION, clearAuthCookies, writeTokenCookies } from "@/server/auth/cookies";
import { refreshAccessToken } from "@/server/auth/oidc";
import { decodeJwt } from "jose";
import { authenticatedDestination } from "@/server/auth/routing";

const CLOCK_SKEW_SECONDS = 30;

/**
 * Gatekeeps the authenticated area. On each dashboard request it ensures a usable access token:
 *  - valid + unexpired  → continue
 *  - expired but refreshable → rotate tokens server-side and set fresh cookies
 *  - otherwise → bounce to sign-in, remembering the requested path
 *
 * Doing the refresh here means server components downstream can simply read the cookie.
 */
export async function proxy(request: NextRequest) {
  const host = request.headers.get("host");

  // Prevent state_mismatch OAuth error by forcing localhost
  if (host && (host.startsWith("0.0.0.0") || host.startsWith("127.0.0.1"))) {
    const newUrl = new URL(request.url);
    newUrl.hostname = "localhost";
    return NextResponse.redirect(newUrl);
  }

  const protectedArea =
    request.nextUrl.pathname.startsWith("/dashboard") ||
    request.nextUrl.pathname.startsWith("/portal");
  if (!protectedArea) {
    return NextResponse.next();
  }

  const access = request.cookies.get(SESSION.access)?.value;
  const refresh = request.cookies.get(SESSION.refresh)?.value;
  const expiryRaw = request.cookies.get(SESSION.expiry)?.value;
  const expiry = expiryRaw ? Number(expiryRaw) : 0;
  const now = Math.floor(Date.now() / 1000);

  if (access && expiry - CLOCK_SKEW_SECONDS > now) {
    return responseForToken(request, access);
  }

  if (refresh) {
    try {
      const tokens = await refreshAccessToken(refresh);
      const response = responseForToken(request, tokens.accessToken);
      writeTokenCookies(response.cookies, tokens);
      return response;
    } catch {
      // Refresh token expired/revoked — fall through to sign-in.
    }
  }

  const loginUrl = new URL("/api/auth/login", request.url);
  loginUrl.searchParams.set(
    "redirect",
    request.nextUrl.pathname + request.nextUrl.search,
  );
  const response = NextResponse.redirect(loginUrl);
  clearAuthCookies(response.cookies);
  return response;
}

function responseForToken(request: NextRequest, token: string): NextResponse {
  try {
    const claims = decodeJwt(token) as Record<string, unknown>;
    const value = claims.role ?? claims.roles;
    const roles =
      value == null ? [] : Array.isArray(value) ? value.map(String) : [String(value)];
    const requested = request.nextUrl.pathname + request.nextUrl.search;
    const destination = authenticatedDestination(requested, roles);
    return destination === requested
      ? NextResponse.next()
      : NextResponse.redirect(new URL(destination, request.url));
  } catch {
    return NextResponse.next();
  }
}

export const config = {
  matcher: ["/dashboard", "/dashboard/:path*", "/portal", "/portal/:path*"],
};
