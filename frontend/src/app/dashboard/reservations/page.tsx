import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { Guest, Reservation, Room } from "@/types/api";
import { ReservationsTable } from "@/features/reservations/ReservationsTable";
import { CreateReservationDialog } from "@/features/reservations/CreateReservationDialog";

export const metadata: Metadata = { title: "Reservations" };

function guestName(g: Guest): string {
  return (
    g.fullName ?? (`${g.firstName ?? ""} ${g.lastName ?? ""}`.trim() || g.email || g.id)
  );
}

export default async function ReservationsPage() {
  // Reservations for the table; guests + rooms feed the create dialog's selects and let us
  // resolve the human-readable guest/room labels the list endpoint returns only as ids.
  const [reservations, guests, rooms] = await Promise.all([
    getList<Reservation>("/api/v1/reservations"),
    getList<Guest>("/api/v1/guests"),
    getList<Room>("/api/v1/rooms"),
  ]);

  const guestById = new Map(guests.map((g) => [g.id, g]));
  const roomById = new Map(rooms.map((r) => [r.id, r]));

  const enriched: Reservation[] = reservations.map((r) => {
    const guest = r.guestId ? guestById.get(r.guestId) : undefined;
    const room = r.roomId ? roomById.get(r.roomId) : undefined;
    return {
      ...r,
      guestName: r.guestName ?? (guest ? guestName(guest) : undefined),
      roomNumber: r.roomNumber ?? room?.number,
      total: r.total ?? r.totalPrice,
    };
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Reservations"
        description="Manage bookings across the property."
        actions={<CreateReservationDialog guests={guests} rooms={rooms} />}
      />
      <ReservationsTable data={enriched} />
    </div>
  );
}
