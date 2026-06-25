"use client";

import type { ReactNode } from "react";
import { cn } from "@/lib/utils";
import { ShellProvider, useShell } from "./shell-context";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";
import { CommandPalette } from "./CommandPalette";
import type { UserMenuUser } from "./UserMenu";

function ShellInner({ user, children }: { user: UserMenuUser; children: ReactNode }) {
  const { collapsed } = useShell();

  return (
    <div className="flex min-h-screen w-full bg-muted/30">
      <aside
        className={cn(
          "hidden shrink-0 border-r bg-background transition-[width] duration-200 md:block",
          collapsed ? "w-[68px]" : "w-64",
        )}
      >
        <div className="sticky top-0 h-screen">
          <Sidebar collapsed={collapsed} />
        </div>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar user={user} />
        <main className="flex-1 p-4 sm:p-6">
          <div className="mx-auto w-full max-w-7xl">{children}</div>
        </main>
      </div>

      <CommandPalette />
    </div>
  );
}

/** Authenticated dashboard chrome. `user` comes from the server layout (decoded access token). */
export function AppShell({
  user,
  children,
}: {
  user: UserMenuUser;
  children: ReactNode;
}) {
  return (
    <ShellProvider>
      <ShellInner user={user}>{children}</ShellInner>
    </ShellProvider>
  );
}
