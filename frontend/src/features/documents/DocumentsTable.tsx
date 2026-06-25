"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { FileText } from "lucide-react";
import type { DocumentItem } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { formatDate } from "@/lib/format";

function humanSize(bytes?: number): string {
  if (bytes == null) return "—";
  const units = ["B", "KB", "MB", "GB"];
  let n = bytes;
  let i = 0;
  while (n >= 1024 && i < units.length - 1) {
    n /= 1024;
    i++;
  }
  return `${n.toFixed(i === 0 ? 0 : 1)} ${units[i]}`;
}

const columns: ColumnDef<DocumentItem>[] = [
  {
    accessorKey: "name",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Name" />,
    cell: ({ row }) => (
      <span className="font-medium">{row.original.name ?? row.original.key}</span>
    ),
  },
  {
    accessorKey: "contentType",
    header: "Type",
    cell: ({ row }) => row.original.contentType ?? "—",
  },
  {
    accessorKey: "size",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Size" />,
    cell: ({ row }) => humanSize(row.original.size),
  },
  {
    accessorKey: "uploadedOn",
    header: ({ column }) => <DataTableColumnHeader column={column} title="Uploaded" />,
    cell: ({ row }) => formatDate(row.original.uploadedOn),
  },
];

export function DocumentsTable({ data }: { data: DocumentItem[] }) {
  return (
    <DataTable
      columns={columns}
      data={data}
      searchPlaceholder="Search documents…"
      exportFileName="documents.csv"
      emptyState={
        <EmptyState
          icon={FileText}
          title="No documents"
          description="Uploaded documents for this tenant will appear here."
        />
      }
    />
  );
}
