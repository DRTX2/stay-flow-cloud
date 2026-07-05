import type { ReactNode } from "react";
import type { Metadata } from "next";
import { requireUser } from "@/server/auth/current-user";
import { PortalShell } from "@/features/portal/PortalShell";

export const metadata: Metadata = {
  title: { default: "Guest Portal", template: "%s · StayFlow Portal" },
};

/**
 * Authenticated layout for the guest portal. Uses `requireUser` for defense-in-depth on top of
 * the middleware. Redirects to login if there is no session. The PortalShell provides a lighter
 * sidebar and topbar designed for guest-facing interactions.
 */
export default async function PortalLayout({ children }: { children: ReactNode }) {
  const user = await requireUser();
  return (
    <PortalShell user={{ name: user.name, email: user.email, tenantId: user.tenantId }}>
      {children}
    </PortalShell>
  );
}
