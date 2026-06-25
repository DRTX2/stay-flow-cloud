import "server-only";
import { serverConfig } from "@/server/config";
import { getAccessToken } from "@/server/auth/session";

/** Thrown for non-2xx API responses; `status` lets callers special-case 401/403/404. */
export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

interface ApiOptions extends RequestInit {
  /** Attach the bearer token (default true). Set false for anonymous/public endpoints. */
  auth?: boolean;
}

/**
 * Server-to-server fetch against the .NET API. Reads the access token from the httpOnly session
 * cookie and forwards it as a bearer token. Token freshness is maintained by the middleware
 * (proactive refresh), so this client does not refresh inline — a 401 surfaces as an ApiError and
 * the caller redirects to sign-in.
 */
export async function apiFetch(
  path: string,
  options: ApiOptions = {},
): Promise<Response> {
  const { auth = true, headers, cache, ...rest } = options;
  const merged = new Headers(headers);
  merged.set("Accept", "application/json");

  if (auth) {
    const token = await getAccessToken();
    if (token) merged.set("Authorization", `Bearer ${token}`);
  }

  const url = path.startsWith("http") ? path : `${serverConfig.apiInternalUrl}${path}`;
  return fetch(url, { ...rest, headers: merged, cache: cache ?? "no-store" });
}

export async function getJson<T>(path: string, options?: ApiOptions): Promise<T> {
  const res = await apiFetch(path, options);
  if (!res.ok) throw new ApiError(res.status, `GET ${path} → ${res.status}`);
  return (await res.json()) as T;
}

/** The API returns either a bare array or a paged envelope ({ items: [...] }); normalize. */
export async function getList<T>(path: string, options?: ApiOptions): Promise<T[]> {
  const res = await apiFetch(path, options);
  if (!res.ok) throw new ApiError(res.status, `GET ${path} → ${res.status}`);
  const data: unknown = await res.json();
  if (Array.isArray(data)) return data as T[];
  if (data && typeof data === "object" && "items" in data) {
    const items = (data as { items: unknown }).items;
    if (Array.isArray(items)) return items as T[];
  }
  return [];
}
