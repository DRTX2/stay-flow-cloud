"use client";

import { useState } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { ConciergeBell, MoreHorizontal, Pencil } from "lucide-react";
import type { ServiceItem } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { humanizeEnum, money2 } from "@/lib/format";
import { ServiceFormDialog } from "./ServiceFormDialog";

function serviceColumns(onEdit: (s: ServiceItem) => void): ColumnDef<ServiceItem>[] {
  return [
    {
      accessorKey: "name",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Name" />,
      cell: ({ row }) => <span className="font-medium">{row.original.name ?? "—"}</span>,
    },
    {
      accessorKey: "category",
      header: "Category",
      cell: ({ row }) =>
        row.original.category ? (
          <Badge variant="secondary">{humanizeEnum(String(row.original.category))}</Badge>
        ) : (
          "—"
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

export function ServicesTable({ data }: { data: ServiceItem[] }) {
  const [editing, setEditing] = useState<ServiceItem | null>(null);

  return (
    <>
      <DataTable
        columns={serviceColumns(setEditing)}
        data={data}
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
      {editing && (
        <ServiceFormDialog
          key={editing.id}
          service={editing}
          open={!!editing}
          onOpenChange={(o) => !o && setEditing(null)}
        />
      )}
    </>
  );
}
