import "server-only";
import { redirect } from "next/navigation";
import { decodeJwt } from "jose";
import { getAccessToken } from "./session";

export interface CurrentUser {
  id?: string;
  name?: string;
  email?: string;
  roles: string[];
  permissions: string[];
  tenantId?: string;
}

function toStringArray(value: unknown): string[] {
  if (value == null) return [];
  return Array.isArray(value) ? value.map(String) : [String(value)];
}

/**
 * Decode the current access token into a user view model. The token is our own (issued by the
 * platform's OpenIddict server and stored httpOnly by the BFF), so it is trusted without
 * re-verifying the signature here; the API still validates it on every call.
 */
export async function getCurrentUser(): Promise<CurrentUser | null> {
  const token = await getAccessToken();
  if (!token) return null;
  try {
    const claims = decodeJwt(token) as Record<string, unknown>;
    return {
      id: claims.sub as string | undefined,
      name:
        (claims.name as string | undefined) ??
        (claims.preferred_username as string | undefined),
      email: claims.email as string | undefined,
      roles: toStringArray(claims.role ?? claims.roles),
      permissions: toStringArray(claims.permission),
      tenantId: claims.tenant_id as string | undefined,
    };
  } catch {
    return null;
  }
}

/**
 * Page-level guard (defense in depth on top of the middleware): returns the user or redirects to
 * sign-in. Use at the top of authenticated server components/layouts.
 */
export async function requireUser(): Promise<CurrentUser> {
  const user = await getCurrentUser();
  if (!user) redirect("/api/auth/login?redirect=/dashboard");
  return user;
}
