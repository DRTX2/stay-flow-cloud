import "server-only";
import { cookies } from "next/headers";
import { SESSION } from "./cookies";

export {
  SESSION,
  writeTokenCookies,
  clearAuthCookies,
  baseCookieOptions,
} from "./cookies";
export type { TokenSet, WritableCookies } from "./cookies";

export async function getAccessToken(): Promise<string | null> {
  return (await cookies()).get(SESSION.access)?.value ?? null;
}

export async function getRefreshToken(): Promise<string | null> {
  return (await cookies()).get(SESSION.refresh)?.value ?? null;
}

export async function getExpiry(): Promise<number | null> {
  const raw = (await cookies()).get(SESSION.expiry)?.value;
  return raw ? Number(raw) : null;
}
