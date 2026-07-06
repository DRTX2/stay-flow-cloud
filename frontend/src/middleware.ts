import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export function middleware(request: NextRequest) {
  const host = request.headers.get("host");

  // If the user accesses the site via 0.0.0.0 or 127.0.0.1, the OAuth flow will fail
  // with state_mismatch because cookies are set on the IP but the redirect URL uses localhost.
  // We automatically redirect them to localhost to prevent this.
  if (host && (host.startsWith("0.0.0.0") || host.startsWith("127.0.0.1"))) {
    const newUrl = new URL(request.url);
    newUrl.hostname = "localhost";
    return NextResponse.redirect(newUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|favicon.ico|sitemap.xml|robots.txt).*)"],
};
