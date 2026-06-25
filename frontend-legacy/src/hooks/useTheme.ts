import { useEffect } from "react";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setTheme, type Theme } from "@/store/uiSlice";

/**
 * Applies the selected theme to <html> and keeps it in sync with the OS when "system".
 * Returns the current theme + a setter.
 */
export function useTheme() {
  const theme = useAppSelector((s) => s.ui.theme);
  const dispatch = useAppDispatch();

  useEffect(() => {
    const root = document.documentElement;
    const media = window.matchMedia("(prefers-color-scheme: dark)");

    const apply = () => {
      const dark = theme === "dark" || (theme === "system" && media.matches);
      root.classList.toggle("dark", dark);
    };

    apply();
    if (theme === "system") {
      media.addEventListener("change", apply);
      return () => media.removeEventListener("change", apply);
    }
  }, [theme]);

  return { theme, setTheme: (t: Theme) => dispatch(setTheme(t)) };
}
