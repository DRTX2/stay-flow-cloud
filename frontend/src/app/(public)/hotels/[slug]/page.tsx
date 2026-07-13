import type { Metadata } from "next";
import Link from "next/link";
import Image from "next/image";
import { notFound } from "next/navigation";
import { Hotel, Users } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { money } from "@/lib/format";
import { getHotelBySlug } from "@/content/hotels";
import { getLocale } from "@/i18n/server";

export const dynamic = "force-dynamic";

export async function generateMetadata({
  params,
}: {
  params: Promise<{ slug: string }>;
}): Promise<Metadata> {
  const { slug } = await params;
  const hotel = await getHotelBySlug(slug);
  if (!hotel) return { title: "Hotel not found" };

  const title = `${hotel.name} · StayFlow Cloud`;
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
  const locale = await getLocale();

  // Structured data for rich results.
  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "Hotel",
    name: hotel.name,
    description: hotel.description,
    image: hotel.heroImageUrl,
    additionalType: hotel.propertyType,
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
          ← {locale === "es" ? "Todos los hoteles" : "All hotels"}
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
            <Hotel className="h-4 w-4" />
            {hotel.propertyType}
          </p>
        </div>
        <div className="flex items-center gap-3">
          {hotel.fromRate != null && (
            <Badge variant="secondary">
              {locale === "es" ? "desde" : "from"} {money(hotel.fromRate)} /{" "}
              {locale === "es" ? "noche" : "night"}
            </Badge>
          )}
        </div>
      </div>

      <p className="mt-4 max-w-3xl text-muted-foreground">{hotel.description}</p>

      <h2 className="mt-10 text-2xl font-bold tracking-tight">
        {locale === "es" ? "Habitaciones" : "Rooms"}
      </h2>
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
                    <span className="text-muted-foreground">
                      {" "}
                      / {locale === "es" ? "noche" : "night"}
                    </span>
                  </p>
                  <Button asChild size="sm">
                    <Link href={`/book?hotel=${hotel.slug}&roomType=${rt.id}`}>
                      {locale === "es" ? "Reservar" : "Book"}
                    </Link>
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
