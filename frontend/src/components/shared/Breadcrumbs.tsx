"use client";

import { Fragment } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { ChevronRight } from "lucide-react";
import type { Locale } from "@/i18n/config";
import {
  getNavLabel,
  getRouteItem,
  getVisibleNavSections,
  type NavClaims,
} from "@/components/layout/nav";

function fallbackLabel(segment: string): string {
  return segment
    .replaceAll("-", " ")
    .replace(/^./, (character) => character.toUpperCase());
}

export function Breadcrumbs({ locale, claims }: { locale: Locale; claims: NavClaims }) {
  const pathname = usePathname();
  const sections = getVisibleNavSections(claims);
  const home = sections[0]?.items[0];
  const route = getRouteItem(pathname);
  const trailingSegments = route
    ? pathname.slice(route.href.length).split("/").filter(Boolean)
    : [];
  const crumbs = [
    ...(home ? [{ href: home.href, label: getNavLabel(home, locale) }] : []),
    ...(route && route.href !== home?.href
      ? [{ href: route.href, label: getNavLabel(route, locale) }]
      : []),
    ...trailingSegments.map((segment, index) => ({
      href: `${route?.href ?? "/dashboard"}/${trailingSegments.slice(0, index + 1).join("/")}`,
      label: fallbackLabel(segment),
    })),
  ];

  return (
    <nav aria-label="Breadcrumb" className="hidden md:block">
      <ol className="flex items-center gap-1.5 text-sm text-muted-foreground">
        {crumbs.map((crumb, index) => {
          const isLast = index === crumbs.length - 1;
          return (
            <Fragment key={crumb.href}>
              {index > 0 && <ChevronRight aria-hidden="true" className="h-3.5 w-3.5" />}
              <li>
                {isLast ? (
                  <span aria-current="page" className="font-medium text-foreground">
                    {crumb.label}
                  </span>
                ) : (
                  <Link
                    href={crumb.href}
                    className="transition-colors hover:text-foreground"
                  >
                    {crumb.label}
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
