"use client";

import { useState } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { Users, MoreHorizontal, Pencil } from "lucide-react";
import type { Guest } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { initials } from "@/lib/format";
import { GuestFormDialog } from "./GuestFormDialog";

function guestName(g: Guest): string {
  return (
    g.fullName ?? (`${g.firstName ?? ""} ${g.lastName ?? ""}`.trim() || g.email || "—")
  );
}

function guestColumns(onEdit: (g: Guest) => void): ColumnDef<Guest>[] {
  return [
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

export function GuestsTable({ data }: { data: Guest[] }) {
  const [editing, setEditing] = useState<Guest | null>(null);

  return (
    <>
      <DataTable
        columns={guestColumns(setEditing)}
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
      {editing && (
        <GuestFormDialog
          key={editing.id}
          guest={editing}
          open={!!editing}
          onOpenChange={(o) => !o && setEditing(null)}
        />
      )}
    </>
  );
}
