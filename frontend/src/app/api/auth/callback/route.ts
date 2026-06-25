import { NextResponse, type NextRequest } from "next/server";
import { exchangeCode } from "@/server/auth/oidc";
import { SESSION, writeTokenCookies } from "@/server/auth/cookies";
import { serverConfig } from "@/server/config";

function loginWithError(request: NextRequest, reason: string) {
  const url = new URL("/login", request.url);
  url.searchParams.set("error", reason);
  return NextResponse.redirect(url);
}

function safeReturnTo(value: string | undefined): string {
  if (value && value.startsWith("/") && !value.startsWith("//")) return value;
  return "/dashboard";
}

/**
 * Completes the flow: validates the state, exchanges the code for tokens server-to-server, and
 * stores them in httpOnly cookies before redirecting back to the originally requested page.
 */
export async function GET(request: NextRequest) {
  const params = request.nextUrl.searchParams;
  const code = params.get("code");
  const state = params.get("state");
  const oauthError = params.get("error");

  const verifier = request.cookies.get(SESSION.verifier)?.value;
  const savedState = request.cookies.get(SESSION.state)?.value;
  const returnTo = safeReturnTo(request.cookies.get(SESSION.returnTo)?.value);

  if (oauthError) return loginWithError(request, oauthError);
  if (!code || !state || !verifier) return loginWithError(request, "missing_params");
  if (!savedState || savedState !== state)
    return loginWithError(request, "state_mismatch");

  try {
    const tokens = await exchangeCode(code, verifier);
    const response = NextResponse.redirect(new URL(returnTo, serverConfig.siteUrl));
    writeTokenCookies(response.cookies, tokens);
    response.cookies.delete(SESSION.verifier);
    response.cookies.delete(SESSION.state);
    response.cookies.delete(SESSION.returnTo);
    return response;
  } catch {
    return loginWithError(request, "exchange_failed");
  }
}
