import { Suspense } from "react";
import { SignInForm } from "@/features/auth/SignInForm";
import { getLocale } from "@/i18n/server";
import { getJson } from "@/server/api";
import type { ExternalProvider } from "@/features/auth/SignInForm";

export default async function SignInPage() {
  const locale = await getLocale();
  const providers = await getJson<ExternalProvider[]>("/account/external/providers", {
    auth: false,
  }).catch(() => []);
  return (
    <Suspense fallback={null}>
      <SignInForm
        locale={locale}
        providers={providers}
        apiOrigin={process.env.NEXT_PUBLIC_OIDC_AUTHORITY ?? "http://localhost:8080"}
      />
    </Suspense>
  );
}
