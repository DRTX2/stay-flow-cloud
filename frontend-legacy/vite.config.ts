import { defineConfig } from "vitest/config";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import babel from "@rolldown/plugin-babel";
import { fileURLToPath, URL } from "node:url";

// The SPA runs on :5173 (the redirect URI seeded for the OpenIddict `stayflow-spa` client).
// In dev, API + OAuth/login endpoints are proxied so the browser stays same-origin (no CORS,
// and the OAuth2 token exchange just works against /connect/token).
const proxyTarget = process.env.VITE_PROXY_TARGET ?? "http://localhost:8080";
const proxy = Object.fromEntries(
  [
    "/api",
    "/connect",
    "/account",
    "/signin-google",
    "/signin-microsoft",
    "/signin-github",
  ].map((path) => [path, { target: proxyTarget, changeOrigin: true }]),
);

export default defineConfig({
  plugins: [
    react(),
    // React Compiler (stable in React 19) auto-memoizes components, so most manual
    // useMemo/useCallback/React.memo is no longer needed.
    babel({ presets: [reactCompilerPreset()] }),
  ],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  server: {
    port: 5173,
    proxy,
  },
  build: {
    // Source maps for prod debugging; Rolldown (Vite 8) handles vendor chunking automatically.
    sourcemap: true,
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/test/setup.ts"],
    css: true,
    exclude: ["**/node_modules/**", "**/e2e/**"],
    coverage: {
      provider: "v8",
      reporter: ["text", "html"],
    },
  },
});
