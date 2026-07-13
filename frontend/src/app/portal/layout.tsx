import type { ReactNode } from "react";
import type { Metadata } from "next";
import { requireUser } from "@/server/auth/current-user";
import { PortalShell } from "@/features/portal/PortalShell";
import { getLocale } from "@/i18n/server";

export const metadata: Metadata = {
  title: { default: "Guest Portal", template: "%s · StayFlow Portal" },
};

/**
 * Authenticated layout for the guest portal. Uses `requireUser` for defense-in-depth on top of
 * the middleware. Redirects to login if there is no session. The PortalShell provides a lighter
 * sidebar and topbar designed for guest-facing interactions.
 */
export default async function PortalLayout({ children }: { children: ReactNode }) {
  const [user, locale] = await Promise.all([requireUser(), getLocale()]);
  return (
    <PortalShell
      locale={locale}
      user={{ name: user.name, email: user.email, tenantId: user.tenantId }}
    >
      {children}
    </PortalShell>
  );
}
