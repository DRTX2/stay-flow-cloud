import Link from "next/link";
import { Hotel } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ThemeToggle } from "@/components/layout/ThemeToggle";

export function PublicHeader() {
  return (
    <header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <a href="#main-content" className="skip-link">
        Skip to main content
      </a>
      <div className="mx-auto flex h-16 max-w-7xl items-center gap-4 px-4 sm:px-6">
        <Link href="/" className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Hotel className="h-4 w-4" />
          </div>
          <span className="text-sm font-bold tracking-tight">
            StayFlow <span className="text-muted-foreground">Cloud</span>
          </span>
        </Link>

        <nav
          aria-label="Main navigation"
          className="ml-2 flex items-center gap-1 text-sm sm:ml-6"
        >
          <Link
            href="/hotels"
            className="rounded-md px-2 py-2 text-muted-foreground transition-colors hover:text-foreground sm:px-3"
          >
            <span className="sm:hidden">Stay</span>
            <span className="hidden sm:inline">Hotels</span>
          </Link>
          <Link
            href="/pricing"
            className="hidden rounded-md px-3 py-2 text-muted-foreground transition-colors hover:text-foreground sm:inline-flex"
          >
            Pricing
          </Link>
        </nav>

        <div className="ml-auto flex items-center gap-2">
          <ThemeToggle />
          <Button asChild variant="ghost" size="sm" className="hidden sm:inline-flex">
            <Link href="/dashboard">Dashboard</Link>
          </Button>
          <Button asChild size="sm">
            <Link href="/login">Sign in</Link>
          </Button>
        </div>
      </div>
    </header>
  );
}
