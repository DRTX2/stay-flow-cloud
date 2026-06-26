"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { ConciergeBell } from "lucide-react";
import type { ServiceItem } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { Badge } from "@/components/ui/badge";
import { humanizeEnum, money2 } from "@/lib/format";

const columns: ColumnDef<ServiceItem>[] = [
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
];

export function ServicesTable({ data }: { data: ServiceItem[] }) {
  return (
    <DataTable
      columns={columns}
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
  );
}
