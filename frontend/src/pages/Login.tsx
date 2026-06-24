import { Navigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function Login() {
  const { isAuthenticated, login } = useAuth();
  if (isAuthenticated) return <Navigate to="/" replace />;

  return (
    <div className="centered">
      <div className="card login">
        <div className="brand big">
          StayFlow<span>Cloud</span>
        </div>
        <p>Multi-tenant hospitality management platform.</p>
        <button className="primary" onClick={() => void login()}>
          Sign in
        </button>
        <small>
          Authorization Code + PKCE via OpenIddict. Social sign-in (Google /
          Microsoft / GitHub) is available on the hosted login when configured.
        </small>
      </div>
    </div>
  );
}
