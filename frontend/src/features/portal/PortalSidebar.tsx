"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  CalendarCheck,
  Hotel,
  LayoutDashboard,
  User,
  type LucideIcon,
} from "lucide-react";
import { cn } from "@/lib/utils";
import type { Locale } from "@/i18n/config";

interface PortalNavItem {
  href: string;
  label: string;
  icon: LucideIcon;
  exact?: boolean;
}

const portalNav: PortalNavItem[] = [
  { href: "/portal", label: "Home", icon: LayoutDashboard, exact: true },
  { href: "/portal/reservations", label: "My Reservations", icon: CalendarCheck },
  { href: "/portal/profile", label: "My Profile", icon: User },
];

function isActive(pathname: string, item: PortalNavItem): boolean {
  return item.exact ? pathname === item.href : pathname.startsWith(item.href);
}

export function PortalSidebar({
  onNavigate,
  locale,
}: {
  onNavigate?: () => void;
  locale: Locale;
}) {
  const pathname = usePathname();

  return (
    <div className="flex h-full flex-col">
      <Link
        href="/portal"
        className="flex h-14 items-center gap-2 border-b px-4"
        onClick={onNavigate}
      >
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground">
          <Hotel className="h-4 w-4" />
        </div>
        <span className="text-sm font-bold tracking-tight">
          StayFlow <span className="text-muted-foreground">Portal</span>
        </span>
      </Link>

      <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-4">
        {portalNav.map((item) => {
          const active = isActive(pathname, item);
          const label =
            locale === "es"
              ? ({
                  Home: "Inicio",
                  "My Reservations": "Mis reservas",
                  "My Profile": "Mi perfil",
                }[item.label] ?? item.label)
              : item.label;
          return (
            <Link
              key={item.href}
              href={item.href}
              onClick={onNavigate}
              aria-current={active ? "page" : undefined}
              className={cn(
                "flex items-center gap-3 rounded-md px-2.5 py-2 text-sm font-medium transition-colors",
                "text-muted-foreground hover:bg-accent hover:text-accent-foreground",
                active && "bg-accent text-accent-foreground",
              )}
            >
              <item.icon className="h-4 w-4 shrink-0" />
              <span>{label}</span>
            </Link>
          );
        })}
      </nav>

      <div className="border-t px-4 py-3">
        <Link
          href="/dashboard"
          className="text-xs text-muted-foreground transition-colors hover:text-foreground"
        >
          ← {locale === "es" ? "Volver al panel" : "Back to Dashboard"}
        </Link>
      </div>
    </div>
  );
}
