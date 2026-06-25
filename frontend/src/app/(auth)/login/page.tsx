import Link from "next/link";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Sign in",
  robots: { index: false, follow: false },
};

const ERROR_MESSAGES: Record<string, string> = {
  state_mismatch: "Your sign-in session expired. Please try again.",
  missing_params: "The sign-in response was incomplete. Please try again.",
  exchange_failed: "We couldn't complete sign-in. Please try again.",
  access_denied: "Sign-in was cancelled.",
};

export default async function LoginPage({
  searchParams,
}: {
  searchParams: Promise<{ error?: string; redirect?: string }>;
}) {
  const { error, redirect } = await searchParams;
  const message = error
    ? (ERROR_MESSAGES[error] ?? "Something went wrong during sign-in.")
    : null;
  const loginHref = redirect
    ? `/api/auth/login?redirect=${encodeURIComponent(redirect)}`
    : "/api/auth/login";

  return (
    <main className="flex min-h-screen items-center justify-center px-6">
      <div className="w-full max-w-sm rounded-xl border bg-card p-8 shadow-sm">
        <div className="mb-6 text-center">
          <div className="mx-auto mb-3 flex h-11 w-11 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <span className="text-lg font-bold">S</span>
          </div>
          <h1 className="text-xl font-semibold">Welcome back</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            Sign in to your StayFlow Cloud workspace
          </p>
        </div>

        {message && (
          <p
            role="alert"
            className="mb-4 rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm text-destructive"
          >
            {message}
          </p>
        )}

        <a
          href={loginHref}
          className="flex w-full items-center justify-center rounded-md bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground transition hover:opacity-90"
        >
          Continue to sign in
        </a>

        <p className="mt-6 text-center text-xs text-muted-foreground">
          <Link href="/" className="underline-offset-4 hover:underline">
            ← Back to home
          </Link>
        </p>
      </div>
    </main>
  );
}
