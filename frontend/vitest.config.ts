import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";
import { fileURLToPath, URL } from "node:url";

// Component/unit tests. Next-specific build features (RSC, the compiler) aren't needed here;
// @vitejs/plugin-react handles JSX and jsdom provides the DOM.
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) },
  },
  test: {
    environment: "jsdom",
    globals: true,
    setupFiles: ["./src/test/setup.ts"],
    css: true,
    exclude: ["e2e/**", "node_modules/**", ".next/**"],
    coverage: { provider: "v8", reporter: ["text", "html"] },
  },
});
