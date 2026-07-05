import type { Metadata } from "next";
import { CalendarCheck } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { EmptyState } from "@/components/shared/EmptyState";
import { Card, CardContent } from "@/components/ui/card";
import { getList } from "@/server/api";
import { ReservationCard } from "@/features/portal/ReservationCard";
import type { Reservation } from "@/types/api";

export const metadata: Metadata = { title: "My Reservations" };

export default async function PortalReservationsPage() {
  let reservations: Reservation[] = [];
  let failed = false;

  try {
    reservations = await getList<Reservation>("/api/v1/reservations?pageSize=200");
  } catch {
    failed = true;
  }

  // Group reservations by status category for easier browsing.
  const active = reservations.filter(
    (r) => r.status === "Confirmed" || r.status === "CheckedIn",
  );
  const pending = reservations.filter((r) => r.status === "Pending");
  const past = reservations.filter(
    (r) => r.status === "CheckedOut" || r.status === "Cancelled",
  );

  const groups = [
    { title: "Active", items: active },
    { title: "Pending", items: pending },
    { title: "Past", items: past },
  ].filter((g) => g.items.length > 0);

  return (
    <div className="space-y-6">
      <PageHeader title="My Reservations" description="All your bookings in one place." />

      {failed && (
        <Card>
          <CardContent className="p-6 text-sm text-destructive">
            Could not load reservations. Please try again later.
          </CardContent>
        </Card>
      )}

      {!failed && groups.length === 0 && (
        <EmptyState
          icon={CalendarCheck}
          title="No reservations yet"
          description="You don't have any bookings. Explore our hotels and book your first stay."
        />
      )}

      {groups.map((group) => (
        <div key={group.title} className="space-y-3">
          <h2 className="text-lg font-semibold tracking-tight">{group.title}</h2>
          {group.items.map((r) => (
            <ReservationCard key={r.id} reservation={r} />
          ))}
        </div>
      ))}
    </div>
  );
}
