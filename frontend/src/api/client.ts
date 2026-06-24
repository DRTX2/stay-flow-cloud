import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import { config } from "../config";
import { refresh } from "../auth/oidc";
import { clearTokens, loadTokens, saveTokens } from "../auth/tokens";

// Axios instance pointed at the API. Empty baseURL => same-origin (Vite proxy in dev).
export const api = axios.create({
  baseURL: config.apiUrl || undefined,
  headers: { Accept: "application/json" },
});

// Attach the bearer token to every request.
api.interceptors.request.use((req) => {
  const tokens = loadTokens();
  if (tokens?.accessToken) {
    req.headers.Authorization = `Bearer ${tokens.accessToken}`;
  }
  return req;
});

// On a 401, try a single silent refresh + replay before giving up.
let refreshing: Promise<string | null> | null = null;

api.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & {
      _retried?: boolean;
    };
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
          const next = await refresh(tokens.refreshToken!);
          saveTokens(next);
          return next.accessToken;
        } catch {
          clearTokens();
          return null;
        } finally {
          refreshing = null;
        }
      })();

      const newToken = await refreshing;
      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`;
        return api(original);
      }
    }
    return Promise.reject(error);
  },
);
