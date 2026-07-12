import {
  BarChart3,
  CalendarDays,
  CalendarCheck,
  BedDouble,
  LayoutGrid,
  Users,
  ReceiptText,
  ConciergeBell,
  LayoutDashboard,
  Plug,
  ScrollText,
  ToggleRight,
  FileText,
  ClipboardList,
  Wrench,
  Utensils,
  Inbox,
  MessageSquareText,
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
      { href: "/dashboard/booking-enquiries", label: "Booking Enquiries", icon: Inbox },
      { href: "/dashboard/room-rack", label: "Room Rack", icon: CalendarDays },
      { href: "/dashboard/rooms", label: "Rooms", icon: BedDouble },
      { href: "/dashboard/room-types", label: "Room Types", icon: LayoutGrid },
      { href: "/dashboard/guests", label: "Guests", icon: Users },
      { href: "/dashboard/services", label: "Services", icon: ConciergeBell },
      { href: "/dashboard/orders", label: "Orders (F&B)", icon: Utensils },
      { href: "/dashboard/housekeeping", label: "Housekeeping", icon: ClipboardList },
      { href: "/dashboard/maintenance", label: "Maintenance", icon: Wrench },
      { href: "/dashboard/invoices", label: "Invoices", icon: ReceiptText },
      { href: "/dashboard/reports", label: "Reports", icon: BarChart3 },
      { href: "/dashboard/feedback", label: "Guest Feedback", icon: MessageSquareText },
    ],
  },
  {
    title: "Administration",
    items: [
      { href: "/dashboard/documents", label: "Documents", icon: FileText },
      { href: "/dashboard/staff", label: "Staff & Roles", icon: Users },
      {
        href: "/dashboard/tenant-features",
        label: "Plan & Features",
        icon: ToggleRight,
      },
      { href: "/dashboard/audit", label: "Audit Log", icon: ScrollText },
      { href: "/dashboard/integrations", label: "Integrations", icon: Plug },
    ],
  },
];

export const allNavItems: NavItem[] = navSections.flatMap((s) => s.items);
