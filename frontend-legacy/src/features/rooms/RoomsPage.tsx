import { useMemo } from "react";
import { BedDouble } from "lucide-react";
import type { ColumnDef } from "@tanstack/react-table";
import type { Room } from "@/types/api";
import { PageHeader } from "@/components/shared/PageHeader";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { useRooms } from "./api";

export function RoomsPage() {
  const { data, isLoading } = useRooms();

  const columns = useMemo<ColumnDef<Room>[]>(
    () => [
      {
        accessorKey: "number",
        header: ({ column }) => <DataTableColumnHeader column={column} title="Number" />,
        cell: ({ row }) => (
          <span className="font-medium">{row.original.number ?? "—"}</span>
        ),
      },
      {
        accessorKey: "roomTypeName",
        header: ({ column }) => <DataTableColumnHeader column={column} title="Type" />,
        cell: ({ row }) => row.original.roomTypeName ?? "—",
      },
      {
        accessorKey: "floor",
        header: ({ column }) => <DataTableColumnHeader column={column} title="Floor" />,
        cell: ({ row }) => row.original.floor ?? "—",
      },
      {
        accessorKey: "status",
        header: "Status",
        cell: ({ row }) => <StatusBadge status={row.original.status} />,
      },
    ],
    [],
  );

  return (
    <div className="space-y-6">
      <PageHeader title="Rooms" description="Inventory and live room status." />
      <DataTable
        columns={columns}
        data={data ?? []}
        isLoading={isLoading}
        searchPlaceholder="Search rooms…"
        exportFileName="rooms.csv"
        emptyState={
          <EmptyState
            icon={BedDouble}
            title="No rooms"
            description="Rooms will appear here once configured for this tenant."
          />
        }
      />
    </div>
  );
}
