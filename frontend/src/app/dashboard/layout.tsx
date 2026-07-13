import type { ReactNode } from "react";
import { requireUser } from "@/server/auth/current-user";
import { AppShell } from "@/components/layout/AppShell";
import { getLocale } from "@/i18n/server";

/**
 * Authenticated layout. `requireUser` is defense-in-depth on top of the proxy/middleware: it
 * guarantees a session before rendering and supplies the decoded user to the shell chrome.
 */
export default async function DashboardLayout({ children }: { children: ReactNode }) {
  const [user, locale] = await Promise.all([requireUser(), getLocale()]);
  return (
    <AppShell
      locale={locale}
      user={{ name: user.name, email: user.email, tenantId: user.tenantId }}
    >
      {children}
    </AppShell>
  );
}
