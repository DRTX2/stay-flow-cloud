import type { MetadataRoute } from "next";

export default function manifest(): MetadataRoute.Manifest {
  return {
    name: "StayFlow Cloud",
    short_name: "StayFlow",
    description: "Hospitality management platform for modern hotels.",
    start_url: "/",
    display: "standalone",
    background_color: "#ffffff",
    theme_color: "#0a0f1a",
  };
}
