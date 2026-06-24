import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// The SPA runs on :5173 (the redirect URI seeded for the OpenIddict `stayflow-spa` client).
// API calls go to VITE_API_URL (default http://localhost:8080); in dev we also proxy /api and
// /connect so the browser can talk to the API without CORS gymnastics.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Forward API + OAuth2/login endpoints to the ASP.NET Core API so the SPA is same-origin.
      "/api": { target: "http://localhost:8080", changeOrigin: true },
      "/connect": { target: "http://localhost:8080", changeOrigin: true },
      "/account": { target: "http://localhost:8080", changeOrigin: true },
      "/signin-google": { target: "http://localhost:8080", changeOrigin: true },
      "/signin-microsoft": { target: "http://localhost:8080", changeOrigin: true },
      "/signin-github": { target: "http://localhost:8080", changeOrigin: true },
    },
  },
});
