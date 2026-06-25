import { NextResponse, type NextRequest } from "next/server";
import { buildAuthorizeUrl } from "@/server/auth/oidc";
import { createCodeVerifier, createState, deriveCodeChallenge } from "@/server/auth/pkce";
import { SESSION, baseCookieOptions } from "@/server/auth/cookies";

/** Only allow same-site relative redirect targets (no open redirects). */
function safeReturnTo(value: string | null): string {
  if (value && value.startsWith("/") && !value.startsWith("//")) return value;
  return "/dashboard";
}

/**
 * Starts the Authorization Code + PKCE flow. The verifier/state are generated server-side and kept
 * in short-lived httpOnly cookies; the browser is redirected to the OpenIddict authorize endpoint.
 */
export async function GET(request: NextRequest) {
  const verifier = createCodeVerifier();
  const state = createState();
  const challenge = await deriveCodeChallenge(verifier);
  const returnTo = safeReturnTo(request.nextUrl.searchParams.get("redirect"));

  const response = NextResponse.redirect(buildAuthorizeUrl(challenge, state));
  const options = baseCookieOptions(600); // 10 minutes to complete sign-in
  response.cookies.set(SESSION.verifier, verifier, options);
  response.cookies.set(SESSION.state, state, options);
  response.cookies.set(SESSION.returnTo, returnTo, options);
  return response;
}
