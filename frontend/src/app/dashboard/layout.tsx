import type { ReactNode } from "react";
import { requireUser } from "@/server/auth/current-user";
import { AppShell } from "@/components/layout/AppShell";

/**
 * Authenticated layout. `requireUser` is defense-in-depth on top of the proxy/middleware: it
 * guarantees a session before rendering and supplies the decoded user to the shell chrome.
 */
export default async function DashboardLayout({ children }: { children: ReactNode }) {
  const user = await requireUser();
  return (
    <AppShell user={{ name: user.name, email: user.email, tenantId: user.tenantId }}>
      {children}
    </AppShell>
  );
}
