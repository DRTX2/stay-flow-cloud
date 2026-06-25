"use client";

import { ThemeProvider } from "next-themes";
import { Toaster } from "@/components/ui/sonner";
import type { ReactNode } from "react";

/**
 * Client-side providers shared by the whole app. Kept deliberately thin: only state that genuinely
 * needs the client lives here (theme + toasts). Data fetching is done in server components/actions,
 * so there is no global Query/Redux provider at the root — those are added as client islands where a
 * specific interactive feature needs them.
 */
export function Providers({ children }: { children: ReactNode }) {
  return (
    <ThemeProvider
      attribute="class"
      defaultTheme="system"
      enableSystem
      disableTransitionOnChange
    >
      {children}
      <Toaster position="top-right" />
    </ThemeProvider>
  );
}
