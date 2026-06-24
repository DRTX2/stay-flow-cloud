import { DataTable } from "../components/DataTable";
import { useRooms } from "../api/hooks";
import type { Room } from "../api/types";

export function Rooms() {
  const query = useRooms();
  return (
    <section>
      <h1>Rooms</h1>
      <DataTable<Room>
        query={query}
        rowKey={(r) => r.id}
        columns={[
          { header: "Number", render: (r) => r.number ?? "—" },
          { header: "Type", render: (r) => r.roomTypeName ?? "—" },
          { header: "Floor", render: (r) => r.floor ?? "—" },
          {
            header: "Status",
            render: (r) => <span className="badge">{r.status ?? "—"}</span>,
          },
        ]}
      />
    </section>
  );
}
