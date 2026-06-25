"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { ReceiptText } from "lucide-react";
import type { Invoice } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { formatDate, money2 } from "@/lib/format";

const columns: ColumnDef<Invoice>[] = [
  {
    accessorKey: "number",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Invoice" />,
    cell: ({ row }) => (
      <span className="font-medium">{row.original.number ?? row.original.id}</span>
    ),
  },
  {
    accessorKey: "issuedOn",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Issued" />,
    cell: ({ row }) => formatDate(row.original.issuedOn),
  },
  {
    accessorKey: "status",
    header: "Status",
    cell: ({ row }) => <StatusBadge status={row.original.status} />,
  },
  {
    accessorKey: "tax",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Tax" />,
    cell: ({ row }) => money2(row.original.tax),
  },
  {
    accessorKey: "total",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Total" />,
    cell: ({ row }) => (
      <span className="font-medium tabular-nums">{money2(row.original.total)}</span>
    ),
  },
];

export function InvoicesTable({ data }: { data: Invoice[] }) {
  return (
    <DataTable
      columns={columns}
      data={data}
      searchPlaceholder="Search invoices…"
      exportFileName="invoices.csv"
      emptyState={
        <EmptyState
          icon={ReceiptText}
          title="No invoices"
          description="Invoices generated from reservations appear here."
        />
      }
    />
  );
}
