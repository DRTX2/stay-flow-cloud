import {
  createContext,
  use,
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { asArray, decodeJwt } from "@/services/auth/jwt";
import { beginLogin, logoutUrl, refreshTokens } from "@/services/auth/oidc";
import {
  clearTokens,
  loadTokens,
  saveTokens,
  type TokenSet,
} from "@/services/auth/tokens";
import { AUTH_EXPIRED_EVENT } from "@/services/http";

export interface AuthUser {
  name: string;
  email?: string;
  tenantId?: string;
  roles: string[];
  permissions: string[];
}

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: () => Promise<void>;
  logout: () => void;
  setSession: (tokens: TokenSet) => void;
  hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function userFromTokens(tokens: TokenSet): AuthUser | null {
  const claims = decodeJwt(tokens.accessToken);
  if (!claims) return null;
  return {
    name: claims.name ?? claims.email ?? claims.sub ?? "User",
    email: claims.email,
    tenantId: claims.tenant_id,
    roles: asArray(claims.role),
    permissions: asArray(claims.permission),
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [tokens, setTokens] = useState<TokenSet | null>(() => loadTokens());
  const [isLoading, setLoading] = useState(true);

  const setSession = useCallback((next: TokenSet) => {
    saveTokens(next);
    setTokens(next);
  }, []);

  // Silently refresh on mount if the access token is expired but a refresh token exists.
  useEffect(() => {
    let cancelled = false;
    void (async () => {
      const current = loadTokens();
      if (current && current.expiresAt <= Date.now() && current.refreshToken) {
        try {
          const next = await refreshTokens(current.refreshToken);
          if (!cancelled) setSession(next);
        } catch {
          clearTokens();
          if (!cancelled) setTokens(null);
        }
      }
      if (!cancelled) setLoading(false);
    })();
    return () => {
      cancelled = true;
    };
  }, [setSession]);

  // The HTTP layer fires this when a refresh ultimately fails.
  useEffect(() => {
    const onExpired = () => setTokens(null);
    window.addEventListener(AUTH_EXPIRED_EVENT, onExpired);
    return () => window.removeEventListener(AUTH_EXPIRED_EVENT, onExpired);
  }, []);

  const login = useCallback(async () => {
    window.location.href = await beginLogin();
  }, []);

  const logout = useCallback(() => {
    clearTokens();
    setTokens(null);
    window.location.href = logoutUrl();
  }, []);

  const user = useMemo(() => (tokens ? userFromTokens(tokens) : null), [tokens]);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: Boolean(tokens) && tokens!.expiresAt > Date.now(),
      isLoading,
      login,
      logout,
      setSession,
      hasPermission: (p: string) => user?.permissions.includes(p) ?? false,
    }),
    [user, tokens, isLoading, login, logout, setSession],
  );

  return <AuthContext value={value}>{children}</AuthContext>;
}

export function useAuth(): AuthContextValue {
  const ctx = use(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
