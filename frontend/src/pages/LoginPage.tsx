import { Navigate } from "react-router-dom";
import { Hotel, ShieldCheck } from "lucide-react";
import { useAuth } from "@/features/auth/AuthContext";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

export function LoginPage() {
  const { isAuthenticated, login } = useAuth();
  if (isAuthenticated) return <Navigate to="/" replace />;

  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      {/* Brand panel */}
      <div className="relative hidden flex-col justify-between bg-primary p-12 text-primary-foreground lg:flex">
        <div className="flex items-center gap-2 text-lg font-bold">
          <Hotel className="h-6 w-6" />
          StayFlow Cloud
        </div>
        <div className="space-y-4">
          <h2 className="text-3xl font-semibold leading-tight">
            The operating system for modern hospitality.
          </h2>
          <p className="max-w-md text-primary-foreground/80">
            Reservations, rooms, guests, billing and analytics — multi-tenant, secure and
            real-time, in one platform.
          </p>
        </div>
        <p className="text-sm text-primary-foreground/60">
          © {new Date().getFullYear()} StayFlow Cloud
        </p>
      </div>

      {/* Auth panel */}
      <div className="flex items-center justify-center p-6">
        <Card className="w-full max-w-sm">
          <CardContent className="space-y-6 p-8">
            <div className="space-y-2 text-center">
              <div className="mx-auto flex h-11 w-11 items-center justify-center rounded-xl bg-primary text-primary-foreground lg:hidden">
                <Hotel className="h-5 w-5" />
              </div>
              <h1 className="text-xl font-semibold">Welcome back</h1>
              <p className="text-sm text-muted-foreground">
                Sign in to your StayFlow workspace.
              </p>
            </div>

            <Button className="w-full" size="lg" onClick={() => void login()}>
              Sign in
            </Button>

            <div className="flex items-start gap-2 rounded-lg border bg-muted/40 p-3 text-xs text-muted-foreground">
              <ShieldCheck className="mt-0.5 h-4 w-4 shrink-0" />
              <span>
                Secured with OAuth2 Authorization Code + PKCE (OpenIddict). Social sign-in
                is available on the hosted login when configured.
              </span>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
