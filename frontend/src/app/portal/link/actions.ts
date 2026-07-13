"use server";

import { cookies } from "next/headers";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";
import { getRefreshToken, writeTokenCookies } from "@/server/auth/session";
import { refreshAccessToken } from "@/server/auth/oidc";

export async function linkGuestAction(token: string): Promise<ActionResult> {
  const response = await apiFetch("/api/v1/portal/link", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ token }),
  });
  if (!response.ok) return fail(response, "The invitation is invalid or has expired");

  const refreshToken = await getRefreshToken();
  if (!refreshToken)
    return { ok: false, error: "Sign in again to finish connecting your stay." };
  try {
    const tokens = await refreshAccessToken(refreshToken);
    writeTokenCookies(await cookies(), tokens);
    return ok;
  } catch {
    return { ok: false, error: "Your stay was connected. Sign in again to continue." };
  }
}
