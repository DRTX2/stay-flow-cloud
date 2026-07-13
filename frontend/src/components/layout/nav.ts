import {
  BarChart3,
  BedDouble,
  CalendarCheck,
  CalendarDays,
  ClipboardList,
  ConciergeBell,
  FileText,
  Inbox,
  LayoutDashboard,
  LayoutGrid,
  MessageSquareText,
  Plug,
  ReceiptText,
  ScrollText,
  ToggleRight,
  Users,
  Utensils,
  Wrench,
  type LucideIcon,
} from "lucide-react";
import type { Locale } from "@/i18n/config";

export interface NavClaims {
  permissions: string[];
  roles: string[];
}

export interface NavItem {
  href: string;
  label: string;
  labelEs: string;
  icon: LucideIcon;
  exact?: boolean;
  permissions?: string[];
  roles?: string[];
}

export interface NavSection {
  id: string;
  title: string;
  titleEs: string;
  items: NavItem[];
}

export const navSections: NavSection[] = [
  {
    id: "today",
    title: "Today",
    titleEs: "Hoy",
    items: [
      {
        href: "/dashboard",
        label: "Daily dashboard",
        labelEs: "Panel diario",
        icon: LayoutDashboard,
        exact: true,
        permissions: ["analytics:view"],
      },
    ],
  },
  {
    id: "front-desk",
    title: "Front desk",
    titleEs: "Recepción",
    items: [
      {
        href: "/dashboard/reservations",
        label: "Reservations",
        labelEs: "Reservas",
        icon: CalendarCheck,
        permissions: ["reservations:read"],
      },
      {
        href: "/dashboard/booking-enquiries",
        label: "Booking enquiries",
        labelEs: "Solicitudes",
        icon: Inbox,
        permissions: ["reservations:read"],
      },
      {
        href: "/dashboard/guests",
        label: "Guests",
        labelEs: "Huéspedes",
        icon: Users,
        permissions: ["guests:read"],
      },
    ],
  },
  {
    id: "property-operations",
    title: "Property operations",
    titleEs: "Operación del hotel",
    items: [
      {
        href: "/dashboard/room-rack",
        label: "Room rack",
        labelEs: "Calendario",
        icon: CalendarDays,
        permissions: ["analytics:view"],
      },
      {
        href: "/dashboard/housekeeping",
        label: "Housekeeping",
        labelEs: "Housekeeping",
        icon: ClipboardList,
        permissions: ["housekeeping:manage"],
      },
      {
        href: "/dashboard/maintenance",
        label: "Maintenance",
        labelEs: "Mantenimiento",
        icon: Wrench,
        permissions: ["maintenance:manage"],
      },
    ],
  },
  {
    id: "inventory-billing",
    title: "Inventory & billing",
    titleEs: "Inventario y facturación",
    items: [
      {
        href: "/dashboard/rooms",
        label: "Rooms",
        labelEs: "Habitaciones",
        icon: BedDouble,
        permissions: ["rooms:read"],
      },
      {
        href: "/dashboard/room-types",
        label: "Room types",
        labelEs: "Tipos de habitación",
        icon: LayoutGrid,
        permissions: ["rooms:read"],
      },
      {
        href: "/dashboard/services",
        label: "Services",
        labelEs: "Servicios",
        icon: ConciergeBell,
        permissions: ["services:read"],
      },
      {
        href: "/dashboard/orders",
        label: "Orders (F&B)",
        labelEs: "Órdenes (A&B)",
        icon: Utensils,
        permissions: ["orders:manage"],
      },
      {
        href: "/dashboard/invoices",
        label: "Invoices",
        labelEs: "Facturas",
        icon: ReceiptText,
        permissions: ["billing:read"],
      },
      {
        href: "/dashboard/documents",
        label: "Documents",
        labelEs: "Documentos",
        icon: FileText,
        permissions: ["billing:read"],
      },
    ],
  },
  {
    id: "insights",
    title: "Insights",
    titleEs: "Análisis",
    items: [
      {
        href: "/dashboard/reports",
        label: "Reports",
        labelEs: "Reportes",
        icon: BarChart3,
        permissions: ["analytics:view"],
      },
      {
        href: "/dashboard/feedback",
        label: "Guest feedback",
        labelEs: "Opiniones",
        icon: MessageSquareText,
        permissions: ["feedback:read"],
      },
      {
        href: "/dashboard/audit",
        label: "Audit log",
        labelEs: "Auditoría",
        icon: ScrollText,
        permissions: ["analytics:view"],
      },
    ],
  },
  {
    id: "administration",
    title: "Administration",
    titleEs: "Administración",
    items: [
      {
        href: "/dashboard/staff",
        label: "Staff & roles",
        labelEs: "Personal y roles",
        icon: Users,
        permissions: ["staff:manage"],
      },
      {
        href: "/dashboard/tenant-features",
        label: "Plan & features",
        labelEs: "Plan y módulos",
        icon: ToggleRight,
        permissions: ["features:manage"],
      },
      {
        href: "/dashboard/integrations",
        label: "Integrations",
        labelEs: "Integraciones",
        icon: Plug,
        roles: ["SuperAdmin", "HotelOwner", "Admin"],
      },
    ],
  },
];

export const allNavItems = navSections.flatMap((section) => section.items);

export function canAccessNavItem(item: NavItem, claims: NavClaims): boolean {
  const hasPermission =
    !item.permissions ||
    item.permissions.some((permission) => claims.permissions.includes(permission));
  const hasRole = !item.roles || item.roles.some((role) => claims.roles.includes(role));
  return hasPermission && hasRole;
}

export function getVisibleNavSections(claims: NavClaims): NavSection[] {
  return navSections
    .map((section) => ({
      ...section,
      items: section.items.filter((item) => canAccessNavItem(item, claims)),
    }))
    .filter((section) => section.items.length > 0);
}

export function getNavLabel(item: NavItem, locale: Locale): string {
  return locale === "es" ? item.labelEs : item.label;
}

export function getRouteItem(pathname: string): NavItem | undefined {
  return [...allNavItems]
    .sort((a, b) => b.href.length - a.href.length)
    .find((item) =>
      item.exact ? pathname === item.href : pathname.startsWith(item.href),
    );
}
