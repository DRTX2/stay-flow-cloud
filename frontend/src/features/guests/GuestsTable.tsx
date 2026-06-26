"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { Users } from "lucide-react";
import type { Guest } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { initials } from "@/lib/format";

function guestName(g: Guest): string {
  return (
    g.fullName ?? (`${g.firstName ?? ""} ${g.lastName ?? ""}`.trim() || g.email || "—")
  );
}

const columns: ColumnDef<Guest>[] = [
  {
    id: "name",
    accessorFn: guestName,
    header: ({ column }) => <DataTableColumnHeader column={column} title="Guest" />,
    cell: ({ row }) => (
      <div className="flex items-center gap-2">
        <Avatar className="h-7 w-7">
          <AvatarFallback className="text-xs">
            {initials(guestName(row.original))}
          </AvatarFallback>
        </Avatar>
        <span className="font-medium">{guestName(row.original)}</span>
      </div>
    ),
  },
  {
    accessorKey: "email",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Email" />,
    cell: ({ row }) => row.original.email ?? "—",
  },
  {
    accessorKey: "phone",
    header: "Phone",
    cell: ({ row }) => row.original.phone ?? "—",
  },
  {
    accessorKey: "documentNumber",
    header: "Document",
    cell: ({ row }) => row.original.documentNumber ?? "—",
  },
];

export function GuestsTable({ data }: { data: Guest[] }) {
  return (
    <DataTable
      columns={columns}
      data={data}
      searchPlaceholder="Search guests…"
      exportFileName="guests.csv"
      emptyState={
        <EmptyState
          icon={Users}
          title="No guests"
          description="Guests appear here as reservations are made."
        />
      }
    />
  );
}
