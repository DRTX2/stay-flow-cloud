import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Loader2 } from "lucide-react";
import { useAuth } from "@/features/auth/AuthContext";
import { completeLogin } from "@/services/auth/oidc";
import { Button } from "@/components/ui/button";

export function CallbackPage() {
  const navigate = useNavigate();
  const { setSession } = useAuth();

  // Parse the callback once, during render, so the synchronous validation result is the initial
  // state — no synchronous setState inside the effect.
  const [params] = useState(() => new URLSearchParams(window.location.search));
  const code = params.get("code");
  const state = params.get("state");
  const [error, setError] = useState<string | null>(() => {
    if (params.get("error")) return params.get("error");
    if (!params.get("code") || !params.get("state")) return "Missing authorization code.";
    return null;
  });
  const ran = useRef(false); // guard React 19 StrictMode double-invoke

  useEffect(() => {
    if (error || !code || !state) return;
    if (ran.current) return;
    ran.current = true;

    // setError here runs inside an async callback, not synchronously in the effect body.
    completeLogin(code, state)
      .then((tokens) => {
        setSession(tokens);
        navigate("/", { replace: true });
      })
      .catch((e: unknown) =>
        setError(e instanceof Error ? e.message : "Sign-in failed."),
      );
  }, [code, state, error, navigate, setSession]);

  return (
    <div className="flex min-h-screen items-center justify-center p-6">
      {error ? (
        <div className="max-w-sm space-y-4 text-center">
          <h1 className="text-lg font-semibold">Sign-in failed</h1>
          <p className="text-sm text-destructive">{error}</p>
          <Button asChild>
            <Link to="/login">Try again</Link>
          </Button>
        </div>
      ) : (
        <div className="flex items-center gap-2 text-muted-foreground">
          <Loader2 className="h-5 w-5 animate-spin" />
          Completing sign-in…
        </div>
      )}
    </div>
  );
}
