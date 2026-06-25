import { NextResponse, type NextRequest } from "next/server";
import { SESSION, writeTokenCookies } from "@/server/auth/cookies";
import { refreshAccessToken } from "@/server/auth/oidc";

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
  const access = request.cookies.get(SESSION.access)?.value;
  const refresh = request.cookies.get(SESSION.refresh)?.value;
  const expiryRaw = request.cookies.get(SESSION.expiry)?.value;
  const expiry = expiryRaw ? Number(expiryRaw) : 0;
  const now = Math.floor(Date.now() / 1000);

  if (access && expiry - CLOCK_SKEW_SECONDS > now) {
    return NextResponse.next();
  }

  if (refresh) {
    try {
      const tokens = await refreshAccessToken(refresh);
      const response = NextResponse.next();
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
  return NextResponse.redirect(loginUrl);
}

export const config = {
  matcher: ["/dashboard", "/dashboard/:path*"],
};
