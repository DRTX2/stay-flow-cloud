import Link from "next/link";
import Image from "next/image";
import {
  ArrowRight,
  BedDouble,
  CalendarCheck,
  ClipboardList,
  Hotel,
  ShieldCheck,
  Star,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { money } from "@/lib/format";
import { getHotels } from "@/content/hotels";

// Marketing landing is fully static (SSG).
export const dynamic = "force-static";

const PRODUCT_FLOWS = [
  {
    icon: Hotel,
    title: "Launch a property",
    body: "Set up rooms, room types, pricing, services, staff roles and tenant features for one hotel or a group.",
  },
  {
    icon: CalendarCheck,
    title: "Book to checkout",
    body: "Move a guest from public booking to reservation, check-in, service orders, invoice and payment status.",
  },
  {
    icon: ClipboardList,
    title: "Run daily operations",
    body: "Coordinate housekeeping, maintenance, room status, F&B orders, reports and operational visibility.",
  },
];

export default async function MarketingPage() {
  const hotels = await getHotels();
  const featured = hotels.slice(0, 3);

  return (
    <main>
      {/* Hero */}
      <section className="mx-auto max-w-7xl px-4 py-20 text-center sm:px-6 sm:py-28">
        <p className="mx-auto mb-4 w-fit rounded-full border px-3 py-1 text-xs font-medium text-muted-foreground">
          Modern hotel operating system
        </p>
        <h1 className="mx-auto max-w-3xl text-balance text-4xl font-bold tracking-tight sm:text-6xl">
          Run every stay from booking to checkout.
        </h1>
        <p className="mx-auto mt-5 max-w-2xl text-balance text-lg text-muted-foreground">
          StayFlow Cloud gives hotel teams one workflow for property setup, reservations,
          front desk operations, housekeeping, billing and guest self-service.
        </p>
        <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
          <Button asChild size="lg">
            <Link href="/hotels">
              Start a booking <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
          <Button asChild size="lg" variant="outline">
            <Link href="/dashboard">Open operator dashboard</Link>
          </Button>
        </div>
      </section>

      {/* Product flows */}
      <section className="border-t bg-muted/30">
        <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6">
          <div className="mb-8 max-w-2xl">
            <h2 className="text-2xl font-bold tracking-tight">
              Three workflows that prove the product.
            </h2>
            <p className="mt-2 text-muted-foreground">
              The cloud architecture, security and automation exist to make these hotel
              workflows reliable, not to distract from them.
            </p>
          </div>
          <div className="grid gap-4 lg:grid-cols-3">
            {PRODUCT_FLOWS.map((f) => (
              <Card key={f.title}>
                <CardContent className="p-6">
                  <div className="mb-4 flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
                    <f.icon className="h-5 w-5" />
                  </div>
                  <h3 className="font-semibold">{f.title}</h3>
                  <p className="mt-1 text-sm text-muted-foreground">{f.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Trust bar */}
      <section className="border-y">
        <div className="mx-auto grid max-w-7xl gap-4 px-4 py-8 text-sm text-muted-foreground sm:grid-cols-3 sm:px-6">
          <div className="flex items-center gap-2">
            <ShieldCheck className="h-4 w-4 text-primary" />
            Tenant isolation, RBAC and audit trails
          </div>
          <div className="flex items-center gap-2">
            <BedDouble className="h-4 w-4 text-primary" />
            Inventory, room status and operations in one place
          </div>
          <div className="flex items-center gap-2">
            <CalendarCheck className="h-4 w-4 text-primary" />
            Guest-facing booking plus staff-facing control
          </div>
        </div>
      </section>

      {/* Featured hotels */}
      <section className="mx-auto max-w-7xl px-4 py-16 sm:px-6">
        <div className="mb-8 flex items-end justify-between">
          <div>
            <h2 className="text-2xl font-bold tracking-tight">Featured stays</h2>
            <p className="mt-1 text-muted-foreground">
              A glimpse of the properties powered by StayFlow Cloud.
            </p>
          </div>
          <Button asChild variant="ghost" className="hidden sm:inline-flex">
            <Link href="/hotels">
              All hotels <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
        </div>
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {featured.map((hotel) => (
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
                  <h3 className="font-semibold">{hotel.name}</h3>
                  <span className="flex items-center gap-1 text-sm">
                    <Star className="h-3.5 w-3.5 fill-warning text-warning" />
                    {hotel.rating}
                  </span>
                </div>
                <p className="text-sm text-muted-foreground">
                  {hotel.city}, {hotel.country}
                </p>
                <p className="mt-2 text-sm">
                  <span className="font-semibold">{money(hotel.fromRate)}</span>
                  <span className="text-muted-foreground"> / night</span>
                </p>
              </div>
            </Link>
          ))}
        </div>
      </section>
    </main>
  );
}
