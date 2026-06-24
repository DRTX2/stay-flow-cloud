import { Menu, PanelLeftClose, PanelLeftOpen, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet";
import { useAppDispatch } from "@/store/hooks";
import { setCommandOpen, toggleSidebar } from "@/store/uiSlice";
import { Breadcrumbs } from "@/components/shared/Breadcrumbs";
import { Sidebar } from "./Sidebar";
import { ThemeToggle } from "./ThemeToggle";
import { UserMenu } from "./UserMenu";

export function Topbar({ collapsed }: { collapsed: boolean }) {
  const dispatch = useAppDispatch();

  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-2 border-b bg-background/95 px-3 backdrop-blur supports-[backdrop-filter]:bg-background/60 sm:px-4">
      {/* Mobile sidebar */}
      <Sheet>
        <SheetTrigger asChild>
          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            aria-label="Open navigation"
          >
            <Menu className="h-5 w-5" />
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="w-64 p-0">
          <Sidebar collapsed={false} />
        </SheetContent>
      </Sheet>

      {/* Desktop collapse toggle */}
      <Button
        variant="ghost"
        size="icon"
        className="hidden md:inline-flex"
        onClick={() => dispatch(toggleSidebar())}
        aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
      >
        {collapsed ? (
          <PanelLeftOpen className="h-5 w-5" />
        ) : (
          <PanelLeftClose className="h-5 w-5" />
        )}
      </Button>

      <Breadcrumbs />

      <div className="ml-auto flex items-center gap-1.5">
        <Button
          variant="outline"
          size="sm"
          className="hidden h-8 gap-2 text-muted-foreground sm:flex"
          onClick={() => dispatch(setCommandOpen(true))}
        >
          <Search className="h-4 w-4" />
          <span>Search…</span>
          <kbd className="pointer-events-none ml-2 hidden select-none rounded border bg-muted px-1.5 font-mono text-[10px] font-medium lg:inline">
            ⌘K
          </kbd>
        </Button>
        <ThemeToggle />
        <UserMenu />
      </div>
    </header>
  );
}
