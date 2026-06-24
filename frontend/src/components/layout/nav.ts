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
  to: string;
  label: string;
  icon: LucideIcon;
  end?: boolean;
}

export interface NavSection {
  title: string;
  items: NavItem[];
}

export const navSections: NavSection[] = [
  {
    title: "Overview",
    items: [{ to: "/", label: "Dashboard", icon: LayoutDashboard, end: true }],
  },
  {
    title: "Operations",
    items: [
      { to: "/reservations", label: "Reservations", icon: CalendarCheck },
      { to: "/rooms", label: "Rooms", icon: BedDouble },
      { to: "/room-types", label: "Room Types", icon: LayoutGrid },
      { to: "/guests", label: "Guests", icon: Users },
      { to: "/services", label: "Services", icon: ConciergeBell },
      { to: "/invoices", label: "Invoices", icon: ReceiptText },
    ],
  },
  {
    title: "Administration",
    items: [
      { to: "/documents", label: "Documents", icon: FileText },
      { to: "/tenant-features", label: "Tenant Features", icon: ToggleRight },
      { to: "/audit", label: "Audit Log", icon: ScrollText },
    ],
  },
];

export const allNavItems: NavItem[] = navSections.flatMap((s) => s.items);
