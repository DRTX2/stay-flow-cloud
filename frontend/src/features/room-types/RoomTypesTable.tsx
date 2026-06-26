"use client";

import { useState } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { LayoutGrid, MoreHorizontal, Pencil } from "lucide-react";
import type { RoomType } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { money } from "@/lib/format";
import { RoomTypeFormDialog } from "./RoomTypeFormDialog";

function roomTypeColumns(onEdit: (rt: RoomType) => void): ColumnDef<RoomType>[] {
  return [
    {
      accessorKey: "name",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Name" />,
      cell: ({ row }) => <span className="font-medium">{row.original.name ?? "—"}</span>,
    },
    {
      accessorKey: "baseRate",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Base rate" />,
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
    {
      id: "actions",
      enableHiding: false,
      cell: ({ row }) => (
        <div className="text-right">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreHorizontal className="h-4 w-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>Actions</DropdownMenuLabel>
              <DropdownMenuItem onClick={() => onEdit(row.original)}>
                <Pencil className="h-4 w-4" /> Edit
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ];
}

export function RoomTypesTable({ data }: { data: RoomType[] }) {
  const [editing, setEditing] = useState<RoomType | null>(null);

  return (
    <>
      <DataTable
        columns={roomTypeColumns(setEditing)}
        data={data}
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
      {editing && (
        <RoomTypeFormDialog
          key={editing.id}
          roomType={editing}
          open={!!editing}
          onOpenChange={(o) => !o && setEditing(null)}
        />
      )}
    </>
  );
}
