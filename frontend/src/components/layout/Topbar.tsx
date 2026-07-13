"use client";

import { useState } from "react";
import { Menu, PanelLeftClose, PanelLeftOpen, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Breadcrumbs } from "@/components/shared/Breadcrumbs";
import { Sidebar } from "./Sidebar";
import { ThemeToggle } from "./ThemeToggle";
import { UserMenu, type UserMenuUser } from "./UserMenu";
import { useShell } from "./shell-context";
import { LocaleSwitcher } from "@/components/public/LocaleSwitcher";
import type { Locale } from "@/i18n/config";
import type { NavClaims } from "./nav";
import { NotificationBell } from "@/features/notifications/NotificationBell";

export function Topbar({
  user,
  locale,
  claims,
}: {
  user: UserMenuUser;
  locale: Locale;
  claims: NavClaims;
}) {
  const { collapsed, toggleCollapsed, setCommandOpen } = useShell();
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
          <Sidebar
            collapsed={false}
            locale={locale}
            claims={claims}
            onNavigate={() => setMobileOpen(false)}
          />
        </SheetContent>
      </Sheet>

      {/* Desktop collapse toggle */}
      <Button
        variant="ghost"
        size="icon"
        className="hidden md:inline-flex"
        onClick={toggleCollapsed}
        aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
      >
        {collapsed ? (
          <PanelLeftOpen className="h-5 w-5" />
        ) : (
          <PanelLeftClose className="h-5 w-5" />
        )}
      </Button>

      <Breadcrumbs locale={locale} claims={claims} />

      <div className="ml-auto flex items-center gap-1.5">
        <Button
          variant="outline"
          size="sm"
          className="h-8 gap-2 text-muted-foreground"
          onClick={() => setCommandOpen(true)}
          aria-label="Search navigation"
        >
          <Search className="h-4 w-4" />
          <span className="hidden sm:inline">Search…</span>
          <kbd className="pointer-events-none ml-2 hidden select-none rounded border bg-muted px-1.5 font-mono text-[10px] font-medium lg:inline">
            ⌘K
          </kbd>
        </Button>
        <ThemeToggle />
        <NotificationBell />
        <LocaleSwitcher locale={locale} />
        <UserMenu user={user} />
      </div>
    </header>
  );
}
