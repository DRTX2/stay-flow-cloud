import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Monitor, Moon, Sun } from "lucide-react";
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { useAppDispatch, useAppSelector } from "@/store/hooks";
import { setCommandOpen, setTheme } from "@/store/uiSlice";
import { allNavItems } from "./nav";

/** Global ⌘K / Ctrl+K command palette for navigation + quick actions. */
export function CommandPalette() {
  const open = useAppSelector((s) => s.ui.commandOpen);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "k" && (e.metaKey || e.ctrlKey)) {
        e.preventDefault();
        dispatch(setCommandOpen(!open));
      }
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [dispatch, open]);

  const run = (fn: () => void) => {
    dispatch(setCommandOpen(false));
    fn();
  };

  return (
    <CommandDialog open={open} onOpenChange={(v) => dispatch(setCommandOpen(v))}>
      <CommandInput placeholder="Type a command or search…" />
      <CommandList>
        <CommandEmpty>No results found.</CommandEmpty>
        <CommandGroup heading="Navigation">
          {allNavItems.map((item) => (
            <CommandItem
              key={item.to}
              value={item.label}
              onSelect={() => run(() => navigate(item.to))}
            >
              <item.icon />
              {item.label}
            </CommandItem>
          ))}
        </CommandGroup>
        <CommandGroup heading="Theme">
          <CommandItem
            value="Light theme"
            onSelect={() => run(() => dispatch(setTheme("light")))}
          >
            <Sun /> Light
          </CommandItem>
          <CommandItem
            value="Dark theme"
            onSelect={() => run(() => dispatch(setTheme("dark")))}
          >
            <Moon /> Dark
          </CommandItem>
          <CommandItem
            value="System theme"
            onSelect={() => run(() => dispatch(setTheme("system")))}
          >
            <Monitor /> System
          </CommandItem>
        </CommandGroup>
      </CommandList>
    </CommandDialog>
  );
}
