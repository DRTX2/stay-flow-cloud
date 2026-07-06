import type { NextConfig } from "next";

const apiTarget =
  process.env.API_PROXY_TARGET ?? process.env.API_INTERNAL_URL ?? "http://localhost:8080";

const nextConfig: NextConfig = {
  reactStrictMode: true,
  // Self-contained server bundle for the Docker runtime image.
  output: "standalone",
  // React Compiler (stable in React 19) auto-memoizes components — no manual memo/useMemo.
  reactCompiler: true,
  // Source maps in prod for readable stack traces (this is a portfolio app, not a secret).
  productionBrowserSourceMaps: true,
  images: {
    // Public marketing/listing imagery is served from these hosts.
    remotePatterns: [
      { protocol: "https", hostname: "images.unsplash.com" },
      { protocol: "https", hostname: "**.stayflow.cloud" },
    ],
  },
  async rewrites() {
    // Same-origin proxy to the .NET API for any client-side calls that still hit it directly
    // (server components/actions call the API server-to-server via the API client instead).
    return [
      { source: "/api/backend/:path*", destination: `${apiTarget}/api/:path*` },
      { source: "/connect/:path*", destination: `${apiTarget}/connect/:path*` },
    ];
  },
};

export default nextConfig;
