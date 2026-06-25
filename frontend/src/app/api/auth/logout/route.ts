import { NextResponse, type NextRequest } from "next/server";
import { clearAuthCookies } from "@/server/auth/cookies";
import { serverConfig } from "@/server/config";

/** Clears the BFF session cookies and returns the browser to the site root. */
function signOut(request: NextRequest) {
  const response = NextResponse.redirect(new URL("/", serverConfig.siteUrl));
  clearAuthCookies(response.cookies);
  return response;
}

// POST is the primary path (from the sign-out form/action); GET supports a plain link fallback.
export async function POST(request: NextRequest) {
  return signOut(request);
}

export async function GET(request: NextRequest) {
  return signOut(request);
}
