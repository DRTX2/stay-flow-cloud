import Link from "next/link";
import Image from "next/image";
import {
  ArrowRight,
  BedDouble,
  CalendarCheck,
  ClipboardList,
  Hotel,
  KeyRound,
  ShieldCheck,
  Star,
  TrendingUp,
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

const METRICS = [
  { value: "24/7", label: "front desk visibility" },
  { value: "8+", label: "hotel workflows connected" },
  { value: "0", label: "handoff spreadsheets" },
];

export default async function MarketingPage() {
  const hotels = await getHotels();
  const featured = hotels.slice(0, 3);

  return (
    <main id="main-content" tabIndex={-1} className="overflow-hidden">
      {/* Hero */}
      <section className="relative mx-auto grid max-w-7xl gap-12 px-4 py-16 sm:px-6 sm:py-24 lg:grid-cols-[1.02fr_0.98fr] lg:items-center">
        <div className="absolute left-1/2 top-10 -z-10 h-72 w-72 -translate-x-1/2 rounded-full bg-primary/10 blur-3xl" />
        <div>
          <p className="mb-5 w-fit rounded-full border border-primary/20 bg-background/70 px-3 py-1 text-xs font-semibold uppercase tracking-[0.24em] text-primary shadow-sm backdrop-blur">
            Modern hotel operating system
          </p>
          <h1 className="max-w-3xl text-balance text-5xl font-black tracking-tight text-foreground sm:text-7xl">
            Run every stay from booking to checkout.
          </h1>
          <p className="mt-6 max-w-2xl text-balance text-lg leading-8 text-muted-foreground">
            StayFlow Cloud connects public booking, reservations, front desk,
            housekeeping, billing and guest self-service in one production-ready operating
            layer for hotels.
          </p>
          <div className="mt-9 flex flex-wrap items-center gap-3">
            <Button asChild size="lg" className="shadow-lg shadow-primary/20">
              <Link href="/hotels">
                Start a booking <ArrowRight className="ml-2 h-4 w-4" />
              </Link>
            </Button>
            <Button
              asChild
              size="lg"
              variant="outline"
              className="bg-background/70 backdrop-blur"
            >
              <Link href="/dashboard">Open operator dashboard</Link>
            </Button>
          </div>
          <div className="mt-10 grid max-w-xl gap-3 sm:grid-cols-3">
            {METRICS.map((metric) => (
              <div
                key={metric.label}
                className="rounded-2xl border bg-card/75 p-4 shadow-sm backdrop-blur"
              >
                <p className="text-2xl font-black text-primary">{metric.value}</p>
                <p className="mt-1 text-xs font-medium uppercase tracking-wide text-muted-foreground">
                  {metric.label}
                </p>
              </div>
            ))}
          </div>
        </div>

        <div className="relative">
          <div className="absolute -inset-6 -z-10 rounded-[2rem] bg-gradient-to-br from-primary/15 via-accent/30 to-transparent blur-2xl" />
          <div className="rounded-[2rem] border bg-card/85 p-4 shadow-2xl shadow-primary/10 backdrop-blur">
            <div className="overflow-hidden rounded-[1.5rem] border bg-background">
              <div className="flex items-center justify-between border-b bg-muted/40 px-5 py-4">
                <div>
                  <p className="text-sm font-semibold">Live operations</p>
                  <p className="text-xs text-muted-foreground">Today at Ocean Vista</p>
                </div>
                <span className="rounded-full bg-success/10 px-3 py-1 text-xs font-semibold text-success">
                  Stable
                </span>
              </div>
              <div className="grid gap-3 p-5">
                {[
                  ["Arrivals", "18", "6 VIP rooms pre-assigned"],
                  ["Ready rooms", "42", "Housekeeping synced"],
                  ["Open invoices", "$8.4k", "12 pending payments"],
                ].map(([label, value, detail]) => (
                  <div
                    key={label}
                    className="flex items-center justify-between rounded-2xl border bg-card p-4 shadow-sm"
                  >
                    <div>
                      <p className="text-sm font-semibold">{label}</p>
                      <p className="text-xs text-muted-foreground">{detail}</p>
                    </div>
                    <p className="text-2xl font-black tracking-tight">{value}</p>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Product flows */}
      <section className="border-y bg-muted/40">
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
          <div className="grid gap-5 lg:grid-cols-3">
            {PRODUCT_FLOWS.map((f) => (
              <Card
                key={f.title}
                className="group border-primary/10 bg-card/80 shadow-sm transition-all hover:-translate-y-1 hover:shadow-xl hover:shadow-primary/10"
              >
                <CardContent className="p-6">
                  <div className="mb-5 flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br from-primary/15 to-accent text-primary transition-transform group-hover:scale-105">
                    <f.icon className="h-5 w-5" />
                  </div>
                  <h3 className="text-lg font-bold">{f.title}</h3>
                  <p className="mt-2 text-sm leading-6 text-muted-foreground">{f.body}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </section>

      {/* Trust bar */}
      <section>
        <div className="mx-auto grid max-w-7xl gap-4 px-4 py-10 text-sm text-muted-foreground sm:grid-cols-3 sm:px-6">
          <div className="flex items-center gap-3 rounded-2xl border bg-card/70 p-4 shadow-sm">
            <ShieldCheck className="h-5 w-5 text-primary" />
            Tenant isolation, RBAC and audit trails
          </div>
          <div className="flex items-center gap-3 rounded-2xl border bg-card/70 p-4 shadow-sm">
            <BedDouble className="h-5 w-5 text-primary" />
            Inventory, room status and operations in one place
          </div>
          <div className="flex items-center gap-3 rounded-2xl border bg-card/70 p-4 shadow-sm">
            <CalendarCheck className="h-5 w-5 text-primary" />
            Guest-facing booking plus staff-facing control
          </div>
        </div>
      </section>

      <section className="mx-auto max-w-7xl px-4 pb-6 sm:px-6">
        <div className="grid gap-5 rounded-[2rem] border bg-primary p-6 text-primary-foreground shadow-2xl shadow-primary/20 md:grid-cols-3 md:p-8">
          <div className="md:col-span-1">
            <p className="text-sm font-semibold uppercase tracking-[0.22em] text-primary-foreground/70">
              Built for operators
            </p>
            <h2 className="mt-3 text-3xl font-black tracking-tight">
              Less admin drag. More stay control.
            </h2>
          </div>
          <div className="grid gap-4 sm:grid-cols-3 md:col-span-2">
            {[
              {
                icon: KeyRound,
                title: "Role-based access",
                body: "Keep staff actions scoped by tenant and responsibility.",
              },
              {
                icon: TrendingUp,
                title: "Operational signals",
                body: "Surface occupancy, revenue and service status fast.",
              },
              {
                icon: ShieldCheck,
                title: "Audit-ready flows",
                body: "Trace reservation, billing and profile changes.",
              },
            ].map((item) => (
              <div
                key={item.title}
                className="rounded-2xl border border-white/15 bg-white/10 p-4 backdrop-blur"
              >
                <item.icon className="h-5 w-5" />
                <h3 className="mt-4 font-bold">{item.title}</h3>
                <p className="mt-2 text-sm leading-6 text-primary-foreground/75">
                  {item.body}
                </p>
              </div>
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
              className="group overflow-hidden rounded-2xl border bg-card shadow-sm transition-all hover:-translate-y-1 hover:shadow-xl hover:shadow-primary/10"
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
              <div className="p-5">
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
