"use client";

import { useTransition } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { ReceiptText, MoreHorizontal, CheckCircle2 } from "lucide-react";
import { toast } from "sonner";
import type { Invoice } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import {
  DataTable,
  type ServerTableConfig,
} from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { formatDate, money2 } from "@/lib/format";
import { payInvoiceAction } from "@/app/dashboard/invoices/actions";

function invoiceColumns(
  onPay: (i: Invoice) => void,
  pending: boolean,
): ColumnDef<Invoice>[] {
  return [
    {
      accessorKey: "number",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Invoice" />,
      cell: ({ row }) => (
        <span className="font-medium">{row.original.number ?? row.original.id}</span>
      ),
    },
    {
      accessorKey: "issuedAtUtc",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Issued" />,
      cell: ({ row }) => formatDate(row.original.issuedAtUtc),
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => <StatusBadge status={row.original.status} />,
    },
    {
      accessorKey: "taxTotal",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Tax" />,
      cell: ({ row }) => money2(row.original.taxTotal, row.original.currency),
    },
    {
      accessorKey: "total",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Total" />,
      cell: ({ row }) => (
        <span className="font-medium tabular-nums">
          {money2(row.original.total, row.original.currency)}
        </span>
      ),
    },
    {
      id: "actions",
      enableHiding: false,
      cell: ({ row }) => {
        const paid = (row.original.status ?? "").toLowerCase() === "paid";
        return (
          <div className="text-right">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8"
                  disabled={pending}
                >
                  <MoreHorizontal className="h-4 w-4" />
                  <span className="sr-only">Open menu</span>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuLabel>Actions</DropdownMenuLabel>
                <DropdownMenuItem disabled={paid} onClick={() => onPay(row.original)}>
                  <CheckCircle2 className="h-4 w-4" /> Mark as paid
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        );
      },
    },
  ];
}

export function InvoicesTable({
  data,
  pagination,
}: {
  data: Invoice[];
  pagination?: ServerTableConfig;
}) {
  const [pending, startTransition] = useTransition();

  function handlePay(invoice: Invoice) {
    startTransition(async () => {
      const result = await payInvoiceAction(invoice.id);
      if (result.ok) toast.success("Invoice marked as paid");
      else toast.error(result.error ?? "Could not mark invoice paid");
    });
  }

  return (
    <DataTable
      columns={invoiceColumns(handlePay, pending)}
      data={data}
      searchPlaceholder="Search invoices…"
      exportFileName="invoices.csv"
      serverPagination={pagination}
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
