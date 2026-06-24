import { useMemo } from "react";
import { ConciergeBell } from "lucide-react";
import type { ColumnDef } from "@tanstack/react-table";
import type { ServiceItem } from "@/types/api";
import { PageHeader } from "@/components/shared/PageHeader";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { money2 } from "@/lib/format";
import { useServices } from "./api";

export function ServicesPage() {
  const { data, isLoading } = useServices();

  const columns = useMemo<ColumnDef<ServiceItem>[]>(
    () => [
      {
        accessorKey: "name",
        header: ({ column }) => <DataTableColumnHeader column={column} title="Name" />,
        cell: ({ row }) => (
          <span className="font-medium">{row.original.name ?? "—"}</span>
        ),
      },
      {
        accessorKey: "description",
        header: "Description",
        cell: ({ row }) => (
          <span className="text-muted-foreground">{row.original.description ?? "—"}</span>
        ),
      },
      {
        accessorKey: "price",
        header: ({ column }) => <DataTableColumnHeader column={column} title="Price" />,
        cell: ({ row }) => money2(row.original.price),
      },
    ],
    [],
  );

  return (
    <div className="space-y-6">
      <PageHeader title="Services" description="Sellable extras attached to stays." />
      <DataTable
        columns={columns}
        data={data ?? []}
        isLoading={isLoading}
        searchPlaceholder="Search services…"
        exportFileName="services.csv"
        emptyState={
          <EmptyState
            icon={ConciergeBell}
            title="No services"
            description="Add services such as breakfast, spa or transfers."
          />
        }
      />
    </div>
  );
}
