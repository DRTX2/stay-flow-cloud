"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { ChevronDown, Hotel } from "lucide-react";
import { cn } from "@/lib/utils";
import { getNavLabel, getVisibleNavSections, type NavClaims, type NavItem } from "./nav";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";
import type { Locale } from "@/i18n/config";

function isActive(pathname: string, item: NavItem): boolean {
  return item.exact ? pathname === item.href : pathname.startsWith(item.href);
}

export function Sidebar({
  collapsed,
  locale,
  claims,
  onNavigate,
}: {
  collapsed: boolean;
  locale: Locale;
  claims: NavClaims;
  onNavigate?: () => void;
}) {
  const pathname = usePathname();
  const sections = getVisibleNavSections(claims);
  const activeSection = sections.find((section) =>
    section.items.some((item) => isActive(pathname, item)),
  );
  const activeSectionId = activeSection?.id;
  const [openSections, setOpenSections] = useState(
    () => new Set<string>(["today", ...(activeSectionId ? [activeSectionId] : [])]),
  );
  const homeHref = sections[0]?.items[0]?.href ?? "/dashboard";

  useEffect(() => {
    if (!activeSectionId) return;
    // Reveal the destination when navigation happened outside the sidebar (for example, ⌘K).
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setOpenSections((current) =>
      current.has(activeSectionId) ? current : new Set(current).add(activeSectionId),
    );
  }, [activeSectionId]);

  function toggleSection(id: string) {
    setOpenSections((current) => {
      const next = new Set(current);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  }

  return (
    <div className="flex h-full flex-col">
      <Link
        href={homeHref}
        aria-label="StayFlow Cloud home"
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

      <nav aria-label="Primary navigation" className="flex-1 overflow-y-auto px-3 py-3">
        {sections.map((section) => {
          const open = collapsed || openSections.has(section.id);
          const title = locale === "es" ? section.titleEs : section.title;
          return (
            <div
              key={section.id}
              className="border-b border-border/60 py-1 last:border-0"
            >
              {!collapsed && (
                <button
                  type="button"
                  aria-expanded={open}
                  aria-controls={`nav-group-${section.id}`}
                  onClick={() => toggleSection(section.id)}
                  className="flex w-full items-center justify-between rounded-md px-2 py-2 text-left text-xs font-semibold uppercase tracking-wider text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                >
                  {title}
                  <ChevronDown
                    className={cn(
                      "h-3.5 w-3.5 transition-transform",
                      open && "rotate-180",
                    )}
                  />
                </button>
              )}
              <ul
                id={`nav-group-${section.id}`}
                className={cn("space-y-1 py-1", !open && "hidden")}
              >
                {section.items.map((item) => {
                  const active = isActive(pathname, item);
                  const label = getNavLabel(item, locale);
                  const link = (
                    <Link
                      href={item.href}
                      onClick={onNavigate}
                      aria-current={active ? "page" : undefined}
                      aria-label={collapsed ? label : undefined}
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
          );
        })}
      </nav>
    </div>
  );
}
