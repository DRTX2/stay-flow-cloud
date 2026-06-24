import { DataTable } from "../components/DataTable";
import { useReservations } from "../api/hooks";
import { date, money } from "../format";
import type { Reservation } from "../api/types";

export function Reservations() {
  const query = useReservations();
  return (
    <section>
      <h1>Reservations</h1>
      <DataTable<Reservation>
        query={query}
        rowKey={(r) => r.id}
        columns={[
          { header: "Guest", render: (r) => r.guestName ?? "—" },
          { header: "Room", render: (r) => r.roomNumber ?? "—" },
          { header: "Check-in", render: (r) => date(r.checkIn) },
          { header: "Check-out", render: (r) => date(r.checkOut) },
          {
            header: "Status",
            render: (r) => <span className="badge">{r.status ?? "—"}</span>,
          },
          { header: "Total", render: (r) => money(r.total) },
        ]}
      />
    </section>
  );
}
