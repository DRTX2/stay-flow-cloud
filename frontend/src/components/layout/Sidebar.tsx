"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Hotel } from "lucide-react";
import { cn } from "@/lib/utils";
import { navSections, type NavItem } from "./nav";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import type { Locale } from "@/i18n/config";

const ES_LABELS: Record<string, string> = {
  "/dashboard": "Panel diario",
  "/dashboard/reservations": "Reservas",
  "/dashboard/booking-enquiries": "Solicitudes",
  "/dashboard/room-rack": "Calendario",
  "/dashboard/rooms": "Habitaciones",
  "/dashboard/room-types": "Tipos de habitación",
  "/dashboard/guests": "Huéspedes",
  "/dashboard/services": "Servicios",
  "/dashboard/orders": "Órdenes (A&B)",
  "/dashboard/housekeeping": "Housekeeping",
  "/dashboard/maintenance": "Mantenimiento",
  "/dashboard/invoices": "Facturas",
  "/dashboard/reports": "Reportes",
  "/dashboard/feedback": "Opiniones",
  "/dashboard/documents": "Documentos",
  "/dashboard/staff": "Personal y roles",
  "/dashboard/tenant-features": "Plan y módulos",
  "/dashboard/audit": "Auditoría",
  "/dashboard/integrations": "Integraciones",
};

function isActive(pathname: string, item: NavItem): boolean {
  return item.exact ? pathname === item.href : pathname.startsWith(item.href);
}

export function Sidebar({
  collapsed,
  locale,
  onNavigate,
}: {
  collapsed: boolean;
  locale: Locale;
  onNavigate?: () => void;
}) {
  const pathname = usePathname();

  return (
    <div className="flex h-full flex-col">
      <Link
        href="/dashboard"
        className="flex h-14 items-center gap-2 border-b px-4"
        onClick={onNavigate}
      >
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary text-primary-foreground">
          <Hotel className="h-4 w-4" />
        </div>
        {!collapsed && (
          <span className="text-sm font-bold tracking-tight">
            StayFlow <span className="text-muted-foreground">Cloud</span>
          </span>
        )}
      </Link>

      <nav className="flex-1 space-y-6 overflow-y-auto px-3 py-4">
        {navSections.map((section) => (
          <div key={section.title}>
            {!collapsed && (
              <p className="mb-2 px-2 text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {locale === "es"
                  ? section.title === "Overview"
                    ? "Resumen"
                    : section.title === "Operations"
                      ? "Operación"
                      : "Administración"
                  : section.title}
              </p>
            )}
            <ul className="space-y-1">
              {section.items.map((item) => {
                const active = isActive(pathname, item);
                const label =
                  locale === "es" ? (ES_LABELS[item.href] ?? item.label) : item.label;
                const link = (
                  <Link
                    href={item.href}
                    onClick={onNavigate}
                    aria-current={active ? "page" : undefined}
                    className={cn(
                      "flex items-center gap-3 rounded-md px-2.5 py-2 text-sm font-medium transition-colors",
                      "text-muted-foreground hover:bg-accent hover:text-accent-foreground",
                      active && "bg-accent text-accent-foreground",
                      collapsed && "justify-center",
                    )}
                  >
                    <item.icon className="h-4 w-4 shrink-0" />
                    {!collapsed && <span>{label}</span>}
                  </Link>
                );
                return (
                  <li key={item.href}>
                    {collapsed ? (
                      <Tooltip>
                        <TooltipTrigger asChild>{link}</TooltipTrigger>
                        <TooltipContent side="right">{label}</TooltipContent>
                      </Tooltip>
                    ) : (
                      link
                    )}
                  </li>
                );
              })}
            </ul>
          </div>
        ))}
      </nav>
    </div>
  );
}
