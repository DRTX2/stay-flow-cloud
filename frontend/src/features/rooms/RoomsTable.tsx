"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { BedDouble } from "lucide-react";
import type { Room } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { StatusBadge } from "@/components/shared/StatusBadge";

const columns: ColumnDef<Room>[] = [
  {
    accessorKey: "number",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Number" />,
    cell: ({ row }) => <span className="font-medium">{row.original.number ?? "—"}</span>,
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
];

export function RoomsTable({ data }: { data: Room[] }) {
  return (
    <DataTable
      columns={columns}
      data={data}
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
  );
}
