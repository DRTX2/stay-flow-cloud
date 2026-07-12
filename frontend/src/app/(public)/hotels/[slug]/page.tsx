import type { Metadata } from "next";
import Link from "next/link";
import Image from "next/image";
import { notFound } from "next/navigation";
import { MapPin, Star, Users } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { money } from "@/lib/format";
import { getHotelBySlug, getHotelSlugs } from "@/content/hotels";

export const revalidate = 3600;

// Prerender every hotel at build; new slugs are generated on demand and cached (ISR).
export async function generateStaticParams() {
  const slugs = await getHotelSlugs();
  return slugs.map((slug) => ({ slug }));
}

export async function generateMetadata({
  params,
}: {
  params: Promise<{ slug: string }>;
}): Promise<Metadata> {
  const { slug } = await params;
  const hotel = await getHotelBySlug(slug);
  if (!hotel) return { title: "Hotel not found" };

  const title = `${hotel.name} — ${hotel.city}, ${hotel.country}`;
  return {
    title,
    description: hotel.description,
    alternates: { canonical: `/hotels/${hotel.slug}` },
    openGraph: {
      title,
      description: hotel.description,
      type: "website",
      url: `/hotels/${hotel.slug}`,
      images: hotel.heroImageUrl ? [{ url: hotel.heroImageUrl }] : undefined,
    },
  };
}

export default async function HotelDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  const hotel = await getHotelBySlug(slug);
  if (!hotel) notFound();

  // Structured data for rich results.
  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "Hotel",
    name: hotel.name,
    description: hotel.description,
    image: hotel.heroImageUrl,
    address: {
      "@type": "PostalAddress",
      addressLocality: hotel.city,
      addressCountry: hotel.country,
    },
    aggregateRating: hotel.rating
      ? { "@type": "AggregateRating", ratingValue: hotel.rating, bestRating: 5 }
      : undefined,
    priceRange: hotel.fromRate ? `from ${money(hotel.fromRate)}` : undefined,
  };

  return (
    <main id="main-content" tabIndex={-1} className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }}
      />

      <nav className="mb-4 text-sm text-muted-foreground">
        <Link href="/hotels" className="hover:text-foreground">
          ← All hotels
        </Link>
      </nav>

      <div className="relative aspect-[21/9] overflow-hidden rounded-2xl">
        <Image
          src={hotel.heroImageUrl ?? ""}
          alt={hotel.name ?? "Hotel"}
          fill
          priority
          sizes="100vw"
          className="object-cover"
        />
      </div>

      <div className="mt-6 flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{hotel.name}</h1>
          <p className="mt-1 flex items-center gap-1 text-muted-foreground">
            <MapPin className="h-4 w-4" />
            {hotel.city}, {hotel.country}
          </p>
        </div>
        <div className="flex items-center gap-3">
          <span className="flex items-center gap-1 text-sm font-medium">
            <Star className="h-4 w-4 fill-warning text-warning" />
            {hotel.rating}
          </span>
          <Badge variant="secondary">from {money(hotel.fromRate)} / night</Badge>
        </div>
      </div>

      <p className="mt-4 max-w-3xl text-muted-foreground">{hotel.description}</p>

      {hotel.amenities && hotel.amenities.length > 0 && (
        <div className="mt-4 flex flex-wrap gap-2">
          {hotel.amenities.map((a) => (
            <Badge key={a} variant="outline">
              {a}
            </Badge>
          ))}
        </div>
      )}

      <h2 className="mt-10 text-2xl font-bold tracking-tight">Rooms</h2>
      <div className="mt-4 grid gap-6 lg:grid-cols-2">
        {(hotel.roomTypes ?? []).map((rt) => (
          <Card key={rt.id} className="overflow-hidden">
            <div className="grid sm:grid-cols-[40%_1fr]">
              <div className="relative aspect-[4/3] sm:aspect-auto">
                <Image
                  src={rt.imageUrl ?? hotel.heroImageUrl ?? ""}
                  alt={rt.name ?? "Room"}
                  fill
                  sizes="(max-width: 640px) 100vw, 240px"
                  className="object-cover"
                />
              </div>
              <CardContent className="flex flex-col gap-2 p-5">
                <div className="flex items-center justify-between">
                  <h3 className="font-semibold">{rt.name}</h3>
                  <span className="flex items-center gap-1 text-xs text-muted-foreground">
                    <Users className="h-3.5 w-3.5" />
                    {rt.maxOccupancy}
                  </span>
                </div>
                <p className="flex-1 text-sm text-muted-foreground">{rt.description}</p>
                <div className="flex items-center justify-between">
                  <p className="text-sm">
                    <span className="font-semibold">{money(rt.baseRate)}</span>
                    <span className="text-muted-foreground"> / night</span>
                  </p>
                  <Button asChild size="sm">
                    <Link href={`/book?hotel=${hotel.slug}&roomType=${rt.id}`}>Book</Link>
                  </Button>
                </div>
              </CardContent>
            </div>
          </Card>
        ))}
      </div>
    </main>
  );
}
