import type { ReactNode } from "react";
import { requireStaffUser } from "@/server/auth/current-user";
import { AppShell } from "@/components/layout/AppShell";
import { getLocale } from "@/i18n/server";

/**
 * Authenticated layout. `requireUser` is defense-in-depth on top of the proxy/middleware: it
 * guarantees a session before rendering and supplies the decoded user to the shell chrome.
 */
export default async function DashboardLayout({ children }: { children: ReactNode }) {
  const [user, locale] = await Promise.all([requireStaffUser(), getLocale()]);
  return (
    <AppShell
      locale={locale}
      user={{ name: user.name, email: user.email, tenantId: user.tenantId }}
      claims={{ permissions: user.permissions, roles: user.roles }}
    >
      {children}
    </AppShell>
  );
}
