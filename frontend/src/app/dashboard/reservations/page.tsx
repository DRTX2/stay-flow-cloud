import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { Guest, Reservation, Room } from "@/types/api";
import { ReservationsTable } from "@/features/reservations/ReservationsTable";
import { CreateReservationDialog } from "@/features/reservations/CreateReservationDialog";

export const metadata: Metadata = { title: "Reservations" };

export default async function ReservationsPage() {
  // Reservations for the table; guests + rooms feed the create dialog's selects.
  const [reservations, guests, rooms] = await Promise.all([
    getList<Reservation>("/api/v1/reservations"),
    getList<Guest>("/api/v1/guests"),
    getList<Room>("/api/v1/rooms"),
  ]);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Reservations"
        description="Manage bookings across the property."
        actions={<CreateReservationDialog guests={guests} rooms={rooms} />}
      />
      <ReservationsTable data={reservations} />
    </div>
  );
}
