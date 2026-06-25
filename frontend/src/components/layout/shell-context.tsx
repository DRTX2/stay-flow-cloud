"use client";

import {
  createContext,
  use,
  useCallback,
  useEffect,
  useState,
  type ReactNode,
} from "react";

interface ShellState {
  collapsed: boolean;
  toggleCollapsed: () => void;
  commandOpen: boolean;
  setCommandOpen: (open: boolean) => void;
}

const ShellContext = createContext<ShellState | null>(null);

const STORAGE_KEY = "stayflow.sidebar.collapsed";

/**
 * Client-only UI state for the dashboard shell: sidebar collapse (persisted to localStorage) and the
 * command-palette open flag. Kept as a small context instead of a global store — it is only relevant
 * inside the authenticated shell.
 */
export function ShellProvider({ children }: { children: ReactNode }) {
  const [collapsed, setCollapsed] = useState(false);
  const [commandOpen, setCommandOpen] = useState(false);

  // Hydrate the persisted preference after mount (localStorage is unavailable during SSR).
  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setCollapsed(localStorage.getItem(STORAGE_KEY) === "1");
  }, []);

  const toggleCollapsed = useCallback(() => {
    setCollapsed((prev) => {
      const next = !prev;
      localStorage.setItem(STORAGE_KEY, next ? "1" : "0");
      return next;
    });
  }, []);

  return (
    <ShellContext value={{ collapsed, toggleCollapsed, commandOpen, setCommandOpen }}>
      {children}
    </ShellContext>
  );
}

export function useShell(): ShellState {
  const ctx = use(ShellContext);
  if (!ctx) throw new Error("useShell must be used within ShellProvider");
  return ctx;
}
