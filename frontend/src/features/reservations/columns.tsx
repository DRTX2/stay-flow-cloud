"use client";

import type { ColumnDef } from "@tanstack/react-table";
import {
  MoreHorizontal,
  Ban,
  FileText,
  Check,
  LogIn,
  LogOut,
  MessageSquareText,
} from "lucide-react";
import type { Reservation } from "@/types/api";
import { formatDate, money } from "@/lib/format";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { StatusBadge } from "@/components/shared/StatusBadge";

interface Actions {
  onConfirm: (r: Reservation) => void;
  onCheckIn: (r: Reservation) => void;
  onCheckOut: (r: Reservation) => void;
  onCancel: (r: Reservation) => void;
  onInvoice: (r: Reservation) => void;
  onFeedback: (r: Reservation) => void;
  pending?: boolean;
}

export function reservationColumns({
  onConfirm,
  onCheckIn,
  onCheckOut,
  onCancel,
  onInvoice,
  onFeedback,
  pending,
}: Actions): ColumnDef<Reservation>[] {
  return [
    {
      accessorKey: "guestName",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Guest" />,
      cell: ({ row }) => (
        <span className="font-medium">{row.original.guestName ?? "—"}</span>
      ),
    },
    {
      accessorKey: "roomNumber",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Room" />,
      cell: ({ row }) => row.original.roomNumber ?? "—",
    },
    {
      accessorKey: "checkIn",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Check-in" />,
      cell: ({ row }) => formatDate(row.original.checkIn),
    },
    {
      accessorKey: "checkOut",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Check-out" />,
      cell: ({ row }) => formatDate(row.original.checkOut),
    },
    {
      accessorKey: "status",
      header: "Status",
      cell: ({ row }) => <StatusBadge status={row.original.status} />,
    },
    {
      accessorKey: "total",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Total" />,
      cell: ({ row }) => (
        <span className="tabular-nums">
          {money(row.original.total ?? row.original.totalPrice)}
        </span>
      ),
    },
    {
      id: "actions",
      enableHiding: false,
      cell: ({ row }) => {
        const r = row.original;
        const status = (r.status ?? "").toLowerCase();
        const isPending = status === "pending";
        const isConfirmed = status === "confirmed";
        const isCheckedIn = status === "checkedin";
        const terminal = status === "cancelled" || status === "checkedout";
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
                {isPending && (
                  <DropdownMenuItem onClick={() => onConfirm(r)}>
                    <Check className="h-4 w-4" /> Confirm
                  </DropdownMenuItem>
                )}
                {isConfirmed && (
                  <DropdownMenuItem onClick={() => onCheckIn(r)}>
                    <LogIn className="h-4 w-4" /> Check in
                  </DropdownMenuItem>
                )}
                {isCheckedIn && (
                  <DropdownMenuItem onClick={() => onCheckOut(r)}>
                    <LogOut className="h-4 w-4" /> Check out
                  </DropdownMenuItem>
                )}
                <DropdownMenuItem onClick={() => onInvoice(r)}>
                  <FileText className="h-4 w-4" /> Generate invoice
                </DropdownMenuItem>
                {status === "checkedout" && (
                  <DropdownMenuItem onClick={() => onFeedback(r)}>
                    <MessageSquareText className="h-4 w-4" /> Copy feedback link
                  </DropdownMenuItem>
                )}
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  disabled={terminal}
                  className="text-destructive focus:text-destructive"
                  onClick={() => onCancel(r)}
                >
                  <Ban className="h-4 w-4" /> Cancel
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        );
      },
    },
  ];
}
