import { DataTable } from "../components/DataTable";
import { useServices } from "../api/hooks";
import { money } from "../format";
import type { ServiceItem } from "../api/types";

export function Services() {
  const query = useServices();
  return (
    <section>
      <h1>Services</h1>
      <DataTable<ServiceItem>
        query={query}
        rowKey={(s) => s.id}
        columns={[
          { header: "Name", render: (s) => s.name ?? "—" },
          { header: "Description", render: (s) => s.description ?? "—" },
          { header: "Price", render: (s) => money(s.price) },
        ]}
      />
    </section>
  );
}
