import { Fragment } from "react";
import { Link, useLocation } from "react-router-dom";
import { ChevronRight } from "lucide-react";

const LABELS: Record<string, string> = {
  "": "Dashboard",
  reservations: "Reservations",
  rooms: "Rooms",
  "room-types": "Room Types",
  guests: "Guests",
  invoices: "Invoices",
  services: "Services",
  analytics: "Analytics",
  audit: "Audit",
  "tenant-features": "Tenant Features",
  documents: "Documents",
  settings: "Settings",
};

function label(segment: string): string {
  return LABELS[segment] ?? segment.charAt(0).toUpperCase() + segment.slice(1);
}

export function Breadcrumbs() {
  const { pathname } = useLocation();
  const segments = pathname.split("/").filter(Boolean);

  const crumbs = [
    { href: "/", label: "Dashboard" },
    ...segments.map((seg, i) => ({
      href: "/" + segments.slice(0, i + 1).join("/"),
      label: label(seg),
    })),
  ];

  return (
    <nav aria-label="Breadcrumb" className="hidden md:block">
      <ol className="flex items-center gap-1.5 text-sm text-muted-foreground">
        {crumbs.map((c, i) => {
          const isLast = i === crumbs.length - 1;
          return (
            <Fragment key={c.href}>
              {i > 0 && <ChevronRight className="h-3.5 w-3.5" />}
              <li>
                {isLast ? (
                  <span className="font-medium text-foreground">{c.label}</span>
                ) : (
                  <Link to={c.href} className="transition-colors hover:text-foreground">
                    {c.label}
                  </Link>
                )}
              </li>
            </Fragment>
          );
        })}
      </ol>
    </nav>
  );
}
