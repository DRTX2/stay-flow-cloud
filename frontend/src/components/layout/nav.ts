import {
  CalendarCheck,
  BedDouble,
  LayoutGrid,
  Users,
  ReceiptText,
  ConciergeBell,
  LayoutDashboard,
  ScrollText,
  ToggleRight,
  FileText,
  type LucideIcon,
} from "lucide-react";

export interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
  /** Match the href exactly (used for the dashboard index). */
  exact?: boolean;
}

export interface NavSection {
  title: string;
  items: NavItem[];
}

export const navSections: NavSection[] = [
  {
    title: "Overview",
    items: [
      { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard, exact: true },
    ],
  },
  {
    title: "Operations",
    items: [
      { href: "/dashboard/reservations", label: "Reservations", icon: CalendarCheck },
      { href: "/dashboard/rooms", label: "Rooms", icon: BedDouble },
      { href: "/dashboard/room-types", label: "Room Types", icon: LayoutGrid },
      { href: "/dashboard/guests", label: "Guests", icon: Users },
      { href: "/dashboard/services", label: "Services", icon: ConciergeBell },
      { href: "/dashboard/invoices", label: "Invoices", icon: ReceiptText },
    ],
  },
  {
    title: "Administration",
    items: [
      { href: "/dashboard/documents", label: "Documents", icon: FileText },
      {
        href: "/dashboard/tenant-features",
        label: "Tenant Features",
        icon: ToggleRight,
      },
      { href: "/dashboard/audit", label: "Audit Log", icon: ScrollText },
    ],
  },
];

export const allNavItems: NavItem[] = navSections.flatMap((s) => s.items);
