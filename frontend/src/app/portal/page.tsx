import type { Metadata } from "next";
import Link from "next/link";
import { CalendarCheck, Clock, LogIn, LogOut } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { StatCard } from "@/components/shared/StatCard";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { getList } from "@/server/api";
import { requireUser } from "@/server/auth/current-user";
import { ReservationCard } from "@/features/portal/ReservationCard";
import type { Reservation } from "@/types/api";

export const metadata: Metadata = { title: "Home" };

export default async function PortalHomePage() {
  const user = await requireUser();

  let reservations: Reservation[] = [];
  let failed = false;

  try {
    reservations = await getList<Reservation>("/api/v1/portal/reservations");
  } catch {
    failed = true;
  }

  // Compute summary stats from the guest's reservations.
  const active = reservations.filter(
    (r) => r.status === "Confirmed" || r.status === "CheckedIn",
  );
  const upcoming = reservations.filter((r) => r.status === "Confirmed");
  const checkedIn = reservations.filter((r) => r.status === "CheckedIn");
  const past = reservations.filter(
    (r) => r.status === "CheckedOut" || r.status === "Cancelled",
  );

  return (
    <div className="space-y-6">
      <PageHeader
        title={`Welcome${user.name ? `, ${user.name}` : ""}`}
        description="Your guest portal — view reservations and manage your profile."
      />

      {failed && (
        <Card>
          <CardContent className="p-6 text-sm text-destructive">
            Could not load your reservations. Please try again later.
          </CardContent>
        </Card>
      )}

      {/* KPI row */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          label="Total Reservations"
          value={String(reservations.length)}
          icon={CalendarCheck}
        />
        <StatCard label="Upcoming" value={String(upcoming.length)} icon={Clock} />
        <StatCard label="Checked In" value={String(checkedIn.length)} icon={LogIn} />
        <StatCard label="Past Stays" value={String(past.length)} icon={LogOut} />
      </div>

      {/* Active / upcoming reservations */}
      {active.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Active & upcoming stays</CardTitle>
            <CardDescription>
              Your confirmed and in-progress reservations.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {active.map((r) => (
              <ReservationCard key={r.id} reservation={r} />
            ))}
          </CardContent>
        </Card>
      )}

      {active.length === 0 && !failed && (
        <Card>
          <CardContent className="p-8 text-center text-sm text-muted-foreground">
            You have no upcoming reservations. Browse our{" "}
            <Link href="/hotels" className="underline hover:text-foreground">
              hotels
            </Link>{" "}
            to book your next stay.
          </CardContent>
        </Card>
      )}
    </div>
  );
}
