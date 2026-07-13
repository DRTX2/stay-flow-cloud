"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useTheme } from "next-themes";
import { Monitor, Moon, Sun } from "lucide-react";
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { useShell } from "./shell-context";
import { getNavLabel, getVisibleNavSections, type NavClaims } from "./nav";
import type { Locale } from "@/i18n/config";

/** Global ⌘K / Ctrl+K command palette for navigation + quick actions. */
export function CommandPalette({
  claims,
  locale,
}: {
  claims: NavClaims;
  locale: Locale;
}) {
  const { commandOpen, setCommandOpen } = useShell();
  const { setTheme } = useTheme();
  const router = useRouter();
  const sections = getVisibleNavSections(claims);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "k" && (e.metaKey || e.ctrlKey)) {
        e.preventDefault();
        setCommandOpen(!commandOpen);
      }
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [commandOpen, setCommandOpen]);

  const run = (fn: () => void) => {
    setCommandOpen(false);
    fn();
  };

  return (
    <CommandDialog open={commandOpen} onOpenChange={setCommandOpen}>
      <CommandInput placeholder="Type a command or search…" />
      <CommandList>
        <CommandEmpty>No results found.</CommandEmpty>
        {sections.map((section) => (
          <CommandGroup
            key={section.id}
            heading={locale === "es" ? section.titleEs : section.title}
          >
            {section.items.map((item) => {
              const label = getNavLabel(item, locale);
              return (
                <CommandItem
                  key={item.href}
                  value={`${label} ${section.title}`}
                  onSelect={() => run(() => router.push(item.href))}
                >
                  <item.icon />
                  {label}
                </CommandItem>
              );
            })}
          </CommandGroup>
        ))}
        <CommandGroup heading="Theme">
          <CommandItem value="Light theme" onSelect={() => run(() => setTheme("light"))}>
            <Sun /> Light
          </CommandItem>
          <CommandItem value="Dark theme" onSelect={() => run(() => setTheme("dark"))}>
            <Moon /> Dark
          </CommandItem>
          <CommandItem
            value="System theme"
            onSelect={() => run(() => setTheme("system"))}
          >
            <Monitor /> System
          </CommandItem>
        </CommandGroup>
      </CommandList>
    </CommandDialog>
  );
}
