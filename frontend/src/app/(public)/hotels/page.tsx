import type { Metadata } from "next";
import Link from "next/link";
import Image from "next/image";
import { MapPin, Star } from "lucide-react";
import { money } from "@/lib/format";
import { getHotels } from "@/content/hotels";

export const metadata: Metadata = {
  title: "Hotels",
  description:
    "Browse hotels powered by StayFlow Cloud and book your next stay in seconds.",
};

// Incremental Static Regeneration: prerender at build, refresh hourly.
export const revalidate = 3600;

export default async function HotelsPage() {
  const hotels = await getHotels();

  return (
    <main className="mx-auto max-w-7xl px-4 py-12 sm:px-6">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">Find your stay</h1>
        <p className="mt-1 text-muted-foreground">
          {hotels.length} properties powered by StayFlow Cloud.
        </p>
      </div>

      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {hotels.map((hotel) => (
          <Link
            key={hotel.slug}
            href={`/hotels/${hotel.slug}`}
            className="group overflow-hidden rounded-xl border bg-card transition-shadow hover:shadow-md"
          >
            <div className="relative aspect-[16/10] overflow-hidden">
              <Image
                src={hotel.heroImageUrl ?? ""}
                alt={hotel.name ?? "Hotel"}
                fill
                sizes="(max-width: 768px) 100vw, 33vw"
                className="object-cover transition-transform duration-300 group-hover:scale-105"
              />
            </div>
            <div className="p-4">
              <div className="flex items-center justify-between">
                <h2 className="font-semibold">{hotel.name}</h2>
                <span className="flex items-center gap-1 text-sm">
                  <Star className="h-3.5 w-3.5 fill-warning text-warning" />
                  {hotel.rating}
                </span>
              </div>
              <p className="flex items-center gap-1 text-sm text-muted-foreground">
                <MapPin className="h-3.5 w-3.5" />
                {hotel.city}, {hotel.country}
              </p>
              <p className="mt-2 line-clamp-2 text-sm text-muted-foreground">
                {hotel.description}
              </p>
              <p className="mt-3 text-sm">
                from <span className="font-semibold">{money(hotel.fromRate)}</span>
                <span className="text-muted-foreground"> / night</span>
              </p>
            </div>
          </Link>
        ))}
      </div>
    </main>
  );
}
