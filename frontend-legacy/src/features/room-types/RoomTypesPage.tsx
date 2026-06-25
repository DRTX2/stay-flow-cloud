import { useMemo } from "react";
import { LayoutGrid } from "lucide-react";
import type { ColumnDef } from "@tanstack/react-table";
import type { RoomType } from "@/types/api";
import { PageHeader } from "@/components/shared/PageHeader";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { money } from "@/lib/format";
import { useRoomTypes } from "./api";

export function RoomTypesPage() {
  const { data, isLoading } = useRoomTypes();

  const columns = useMemo<ColumnDef<RoomType>[]>(
    () => [
      {
        accessorKey: "name",
        header: ({ column }) => <DataTableColumnHeader column={column} title="Name" />,
        cell: ({ row }) => (
          <span className="font-medium">{row.original.name ?? "—"}</span>
        ),
      },
      {
        accessorKey: "baseRate",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} title="Base rate" />
        ),
        cell: ({ row }) => money(row.original.baseRate),
      },
      {
        accessorKey: "maxOccupancy",
        header: ({ column }) => (
          <DataTableColumnHeader column={column} title="Max occupancy" />
        ),
        cell: ({ row }) => row.original.maxOccupancy ?? "—",
      },
      {
        accessorKey: "description",
        header: "Description",
        cell: ({ row }) => (
          <span className="text-muted-foreground">{row.original.description ?? "—"}</span>
        ),
      },
    ],
    [],
  );

  return (
    <div className="space-y-6">
      <PageHeader title="Room Types" description="Rate plans and occupancy templates." />
      <DataTable
        columns={columns}
        data={data ?? []}
        isLoading={isLoading}
        searchPlaceholder="Search room types…"
        exportFileName="room-types.csv"
        emptyState={
          <EmptyState
            icon={LayoutGrid}
            title="No room types"
            description="Define room types to enable pricing and bookings."
          />
        }
      />
    </div>
  );
}
