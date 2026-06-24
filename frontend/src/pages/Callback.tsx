import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { completeLogin } from "../auth/oidc";

export function Callback() {
  const navigate = useNavigate();
  const { setSession } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const ran = useRef(false); // guard against React 18 StrictMode double-invoke

  useEffect(() => {
    if (ran.current) return;
    ran.current = true;

    const params = new URLSearchParams(window.location.search);
    const code = params.get("code");
    const state = params.get("state");
    const oauthError = params.get("error");

    if (oauthError) {
      setError(oauthError);
      return;
    }
    if (!code || !state) {
      setError("Missing authorization code.");
      return;
    }

    completeLogin(code, state)
      .then((tokens) => {
        setSession(tokens);
        navigate("/", { replace: true });
      })
      .catch((e: unknown) =>
        setError(e instanceof Error ? e.message : "Sign-in failed."),
      );
  }, [navigate, setSession]);

  return (
    <div className="centered">
      {error ? (
        <div className="card">
          <h3>Sign-in failed</h3>
          <p className="error">{error}</p>
          <a href="/login">Try again</a>
        </div>
      ) : (
        <div>Completing sign-in…</div>
      )}
    </div>
  );
}
