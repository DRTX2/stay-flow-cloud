import { NextResponse } from "next/server";
import { buildEndSessionUrl } from "@/server/auth/oidc";
import { clearAuthCookies } from "@/server/auth/cookies";

/** Clears the BFF cookies, then asks the IdP/API to clear its Identity cookie too. */
function signOut() {
  const response = NextResponse.redirect(buildEndSessionUrl());
  clearAuthCookies(response.cookies);
  return response;
}

// POST is the primary path (from the sign-out form/action); GET supports a plain link fallback.
export async function POST() {
  return signOut();
}

export async function GET() {
  return signOut();
}
