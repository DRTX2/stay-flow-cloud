"use client";

import { useState, type ReactNode } from "react";
import { Menu } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { ThemeToggle } from "@/components/layout/ThemeToggle";
import { UserMenu, type UserMenuUser } from "@/components/layout/UserMenu";
import { PortalSidebar } from "./PortalSidebar";

function PortalTopbar({ user }: { user: UserMenuUser }) {
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-2 border-b bg-background/95 px-3 backdrop-blur supports-[backdrop-filter]:bg-background/60 sm:px-4">
      {/* Mobile sidebar */}
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            aria-label="Open navigation"
          >
            <Menu className="h-5 w-5" />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="w-64 p-0">
          <SheetTitle className="sr-only">Navigation</SheetTitle>
          <PortalSidebar onNavigate={() => setMobileOpen(false)} />
        </SheetContent>
      </Sheet>

      <span className="text-sm font-medium text-muted-foreground md:hidden">
        Guest Portal
      </span>

      <div className="ml-auto flex items-center gap-1.5">
        <ThemeToggle />
        <UserMenu user={user} />
      </div>
    </header>
  );
}

/**
 * Client portal shell — a lighter variant of the dashboard `AppShell` with simplified navigation
 * and portal-specific branding. Used for the guest-facing authenticated area.
 */
export function PortalShell({
  user,
  children,
}: {
  user: UserMenuUser;
  children: ReactNode;
}) {
  return (
    <div className="flex min-h-screen w-full bg-muted/30">
      <aside className="hidden w-64 shrink-0 border-r bg-background md:block">
        <div className="sticky top-0 h-screen">
          <PortalSidebar />
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <PortalTopbar user={user} />
        <main className="flex-1 p-4 sm:p-6">
          <div className="mx-auto w-full max-w-5xl">{children}</div>
        </main>
      </div>
    </div>
  );
}
