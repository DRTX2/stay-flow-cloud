import { createSlice, type PayloadAction } from "@reduxjs/toolkit";

export type Theme = "light" | "dark" | "system";

interface UiState {
  theme: Theme;
  sidebarCollapsed: boolean;
  commandOpen: boolean;
}

const THEME_KEY = "stayflow.theme";
const SIDEBAR_KEY = "stayflow.sidebar";

function initialTheme(): Theme {
  const stored = localStorage.getItem(THEME_KEY) as Theme | null;
  return stored ?? "system";
}

const initialState: UiState = {
  theme: initialTheme(),
  sidebarCollapsed: localStorage.getItem(SIDEBAR_KEY) === "1",
  commandOpen: false,
};

const uiSlice = createSlice({
  name: "ui",
  initialState,
  reducers: {
    setTheme(state, action: PayloadAction<Theme>) {
      state.theme = action.payload;
      localStorage.setItem(THEME_KEY, action.payload);
    },
    toggleSidebar(state) {
      state.sidebarCollapsed = !state.sidebarCollapsed;
      localStorage.setItem(SIDEBAR_KEY, state.sidebarCollapsed ? "1" : "0");
    },
    setSidebarCollapsed(state, action: PayloadAction<boolean>) {
      state.sidebarCollapsed = action.payload;
      localStorage.setItem(SIDEBAR_KEY, action.payload ? "1" : "0");
    },
    setCommandOpen(state, action: PayloadAction<boolean>) {
      state.commandOpen = action.payload;
    },
  },
});

export const { setTheme, toggleSidebar, setSidebarCollapsed, setCommandOpen } =
  uiSlice.actions;
export default uiSlice.reducer;
