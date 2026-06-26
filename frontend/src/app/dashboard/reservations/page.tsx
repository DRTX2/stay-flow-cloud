import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList, getPaged } from "@/server/api";
import { parsePageParams, type SearchParams } from "@/lib/pagination";
import type { Guest, Reservation, Room } from "@/types/api";
import { ReservationsTable } from "@/features/reservations/ReservationsTable";
import { CreateReservationDialog } from "@/features/reservations/CreateReservationDialog";

export const metadata: Metadata = { title: "Reservations" };

function guestName(g: Guest): string {
  return (
    g.fullName ?? (`${g.firstName ?? ""} ${g.lastName ?? ""}`.trim() || g.email || g.id)
  );
}

export default async function ReservationsPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  const { page, pageSize } = parsePageParams(await searchParams);

  // The reservations table is server-paged; guests + rooms feed the create dialog's selects and
  // let us resolve the human-readable labels the list endpoint returns only as ids (fetched with
  // a generous page so look-ups cover the whole tenant, not just the current reservations page).
  const [reservations, guests, rooms] = await Promise.all([
    getPaged<Reservation>("/api/v1/reservations", { page, pageSize }),
    getList<Guest>("/api/v1/guests?pageSize=200"),
    getList<Room>("/api/v1/rooms?pageSize=200"),
  ]);

  const guestById = new Map(guests.map((g) => [g.id, g]));
  const roomById = new Map(rooms.map((r) => [r.id, r]));

  const enriched: Reservation[] = reservations.items.map((r) => {
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
      <ReservationsTable
        data={enriched}
        pagination={{
          page: reservations.page,
          pageSize: reservations.pageSize,
          totalCount: reservations.totalCount,
          totalPages: reservations.totalPages,
        }}
      />
    </div>
  );
}
