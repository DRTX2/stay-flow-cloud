import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { asArray, decodeJwt } from "./jwt";
import { beginLogin, logoutUrl, refresh } from "./oidc";
import {
  clearTokens,
  loadTokens,
  saveTokens,
  type TokenSet,
} from "./tokens";

interface AuthUser {
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

  // On mount, silently refresh if the stored access token is expired but a refresh token exists.
  useEffect(() => {
    let cancelled = false;
    (async () => {
      const current = loadTokens();
      if (current && current.expiresAt <= Date.now() && current.refreshToken) {
        try {
          const next = await refresh(current.refreshToken);
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

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
