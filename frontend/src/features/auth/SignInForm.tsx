"use client";

import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { LocaleSwitcher } from "@/components/public/LocaleSwitcher";
import { dictionaries, type Locale } from "@/i18n/config";

export interface ExternalProvider {
  scheme: string;
  displayName: string;
}

export function SignInForm({
  apiOrigin,
  locale,
  providers,
}: {
  apiOrigin: string;
  locale: Locale;
  providers: ExternalProvider[];
}) {
  const params = useSearchParams();
  const returnUrl = params.get("ReturnUrl") ?? "/";
  const error = params.get("error");
  const copy = dictionaries[locale];
  const message = error
    ? error === "invalid_credentials"
      ? copy.auth.invalid
      : copy.auth.failed
    : null;
  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-6">
      <div className="w-full max-w-sm space-y-6">
        <div className="flex justify-end">
          <LocaleSwitcher locale={locale} />
        </div>
        <div className="text-center">
          <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-lg">
            <span className="text-xl font-bold">S</span>
          </div>
          <h1 className="text-2xl font-semibold tracking-tight">{copy.auth.welcome}</h1>
          <p className="mt-1 text-sm text-muted-foreground">{copy.auth.subtitle}</p>
        </div>
        {message && (
          <p
            role="alert"
            className="rounded-md border border-destructive/40 bg-destructive/10 px-3 py-2 text-sm text-destructive"
          >
            {message}
          </p>
        )}
        {providers.length > 0 && (
          <div className="space-y-3 rounded-xl border bg-card p-5 shadow-sm">
            <div className="grid gap-2">
              {providers.map((provider) => (
                <a
                  key={provider.scheme}
                  href={`${apiOrigin}/account/external?provider=${encodeURIComponent(provider.scheme)}&returnUrl=${encodeURIComponent(returnUrl)}`}
                  className="flex h-10 items-center justify-center rounded-md border bg-background px-4 text-sm font-medium transition-colors hover:bg-muted"
                >
                  {copy.auth.submit} · {provider.displayName}
                </a>
              ))}
            </div>
            <div className="flex items-center gap-3 text-xs text-muted-foreground">
              <span className="h-px flex-1 bg-border" />
              {copy.auth.or}
              <span className="h-px flex-1 bg-border" />
            </div>
          </div>
        )}
        <form
          method="POST"
          action={`${apiOrigin}/account/login`}
          className="space-y-4 rounded-xl border bg-card p-6 shadow-sm"
        >
          <input type="hidden" name="returnUrl" value={returnUrl} />
          <div className="space-y-1.5">
            <label htmlFor="email" className="block text-sm font-medium">
              {copy.auth.email}
            </label>
            <input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              required
              placeholder="admin@stayflow.local"
              className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring"
            />
          </div>
          <div className="space-y-1.5">
            <label htmlFor="password" className="block text-sm font-medium">
              {copy.auth.password}
            </label>
            <input
              id="password"
              name="password"
              type="password"
              autoComplete="current-password"
              required
              placeholder="••••••••"
              className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring"
            />
          </div>
          <button
            type="submit"
            className="flex w-full items-center justify-center rounded-md bg-primary px-4 py-2.5 text-sm font-medium text-primary-foreground hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-ring"
          >
            {copy.auth.submit}
          </button>
        </form>
        <p className="text-center text-xs text-muted-foreground">
          <Link href="/" className="underline-offset-4 hover:underline">
            ← {copy.auth.back}
          </Link>
        </p>
      </div>
    </main>
  );
}
