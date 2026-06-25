// Minimal JWT payload decoder (no signature check — the API validates that). Used only to read
// display name, tenant and permission claims for the UI.
export interface AccessTokenClaims {
  sub?: string;
  name?: string;
  email?: string;
  tenant_id?: string;
  role?: string | string[];
  permission?: string | string[];
  exp?: number;
}

export function decodeJwt(token: string): AccessTokenClaims | null {
  const part = token.split(".")[1];
  if (!part) return null;
  try {
    const json = atob(part.replace(/-/g, "+").replace(/_/g, "/"));
    return JSON.parse(decodeURIComponent(escape(json))) as AccessTokenClaims;
  } catch {
    return null;
  }
}

export function asArray(value: string | string[] | undefined): string[] {
  if (!value) return [];
  return Array.isArray(value) ? value : [value];
}
