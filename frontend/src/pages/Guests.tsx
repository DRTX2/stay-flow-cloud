import { DataTable } from "../components/DataTable";
import { useGuests } from "../api/hooks";
import type { Guest } from "../api/types";

export function Guests() {
  const query = useGuests();
  return (
    <section>
      <h1>Guests</h1>
      <DataTable<Guest>
        query={query}
        rowKey={(g) => g.id}
        columns={[
          { header: "Name", render: (g) => g.fullName ?? "—" },
          { header: "Email", render: (g) => g.email ?? "—" },
          { header: "Phone", render: (g) => g.phone ?? "—" },
          { header: "Document", render: (g) => g.documentId ?? "—" },
        ]}
      />
    </section>
  );
}
