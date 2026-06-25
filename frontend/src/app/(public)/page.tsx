import Link from "next/link";
import Image from "next/image";
import {
  ArrowRight,
  BarChart3,
  CalendarCheck,
  CreditCard,
  ShieldCheck,
  Star,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { money } from "@/lib/format";
import { getHotels } from "@/content/hotels";

// Marketing landing is fully static (SSG).
export const dynamic = "force-static";

const FEATURES = [
  {
    icon: CalendarCheck,
    title: "Reservations & front desk",
    body: "Take bookings, manage check-ins and room status from one fast console.",
  },
  {
    icon: CreditCard,
    title: "Billing & invoicing",
    body: "Generate invoices from stays with tax handling and multi-currency support.",
  },
  {
    icon: BarChart3,
    title: "Executive analytics",
    body: "Revenue, occupancy, ADR and RevPAR with live dashboards.",
  },
  {
    icon: ShieldCheck,
    title: "Secure multi-tenant",
    body: "Tenant isolation, OAuth2/OIDC, audit trails and role-based access.",
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
          Hospitality management platform
        </p>
        <h1 className="mx-auto max-w-3xl text-balance text-4xl font-bold tracking-tight sm:text-6xl">
          Run your hotel on a platform built for modern hospitality.
        </h1>
        <p className="mx-auto mt-5 max-w-2xl text-balance text-lg text-muted-foreground">
          Reservations, front desk, billing and analytics — multi-tenant, secure and fast.
          Give guests a beautiful booking experience and your team a powerful console.
        </p>
        <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
          <Button asChild size="lg">
            <Link href="/hotels">
              Explore hotels <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
          <Button asChild size="lg" variant="outline">
            <Link href="/pricing">View pricing</Link>
          </Button>
        </div>
      </section>

      {/* Features */}
      <section className="border-t bg-muted/30">
        <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6">
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {FEATURES.map((f) => (
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
