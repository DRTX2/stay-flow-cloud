import { Suspense } from "react";
import { Outlet } from "react-router-dom";
import { cn } from "@/lib/utils";
import { useAppSelector } from "@/store/hooks";
import { Sidebar } from "./Sidebar";
import { Topbar } from "./Topbar";
import { CommandPalette } from "./CommandPalette";
import { PageSkeleton } from "@/components/shared/PageSkeleton";

export function AppShell() {
  const collapsed = useAppSelector((s) => s.ui.sidebarCollapsed);

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
        <Topbar collapsed={collapsed} />
        <main className="flex-1 p-4 sm:p-6">
          <div className="mx-auto w-full max-w-7xl">
            <Suspense fallback={<PageSkeleton />}>
              <Outlet />
            </Suspense>
          </div>
        </main>
      </div>

      <CommandPalette />
    </div>
  );
}
