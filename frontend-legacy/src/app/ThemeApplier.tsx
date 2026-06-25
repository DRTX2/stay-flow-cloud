import { useTheme } from "@/hooks/useTheme";

/** Side-effect-only component that keeps <html> in sync with the selected theme. */
export function ThemeApplier() {
  useTheme();
  return null;
}
