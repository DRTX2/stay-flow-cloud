import type { MetadataRoute } from "next";
import { getHotelSlugs } from "@/content/hotels";

const base = process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const now = new Date();
  const slugs = await getHotelSlugs();

  const staticRoutes: MetadataRoute.Sitemap = [
    { url: `${base}/`, lastModified: now, changeFrequency: "weekly", priority: 1 },
    { url: `${base}/hotels`, lastModified: now, changeFrequency: "daily", priority: 0.8 },
    {
      url: `${base}/pricing`,
      lastModified: now,
      changeFrequency: "monthly",
      priority: 0.6,
    },
  ];

  const hotelRoutes: MetadataRoute.Sitemap = slugs.map((slug) => ({
    url: `${base}/hotels/${slug}`,
    lastModified: now,
    changeFrequency: "weekly",
    priority: 0.7,
  }));

  return [...staticRoutes, ...hotelRoutes];
}
