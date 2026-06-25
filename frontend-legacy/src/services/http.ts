import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { env } from "@/config/env";
import { refreshTokens } from "@/services/auth/oidc";
import { clearTokens, loadTokens, saveTokens } from "@/services/auth/tokens";

// Axios instance pointed at the API. Empty baseURL => same-origin (dev proxy).
export const http = axios.create({
  baseURL: env.apiUrl || undefined,
  headers: { Accept: "application/json" },
});

// Attach the bearer token to every request.
http.interceptors.request.use((req) => {
  const tokens = loadTokens();
  if (tokens?.accessToken) {
    req.headers.Authorization = `Bearer ${tokens.accessToken}`;
  }
  return req;
});

// On a 401, try a single silent refresh + replay before propagating the error.
// A 401 after refresh dispatches a global event the app uses to redirect to /login.
export const AUTH_EXPIRED_EVENT = "stayflow:auth-expired";
let refreshing: Promise<string | null> | null = null;

http.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as
      | (InternalAxiosRequestConfig & { _retried?: boolean })
      | undefined;
    const tokens = loadTokens();

    if (
      error.response?.status === 401 &&
      original &&
      !original._retried &&
      tokens?.refreshToken
    ) {
      original._retried = true;
      refreshing ??= (async () => {
        try {
          const next = await refreshTokens(tokens.refreshToken!);
          saveTokens(next);
          return next.accessToken;
        } catch {
          clearTokens();
          window.dispatchEvent(new Event(AUTH_EXPIRED_EVENT));
          return null;
        } finally {
          refreshing = null;
        }
      })();

      const newToken = await refreshing;
      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`;
        return http(original);
      }
    }
    return Promise.reject(error);
  },
);
