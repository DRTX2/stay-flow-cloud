"use client";

import { useState, useTransition } from "react";
import type { ColumnDef } from "@tanstack/react-table";
import { BedDouble, MoreHorizontal, Tag } from "lucide-react";
import { toast } from "sonner";
import type { Room } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import {
  DataTable,
  type ServerTableConfig,
} from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { money2 } from "@/lib/format";
import { updateRoomPriceAction } from "@/app/dashboard/rooms/actions";

function roomColumns(onPrice: (r: Room) => void): ColumnDef<Room>[] {
  return [
    {
      accessorKey: "number",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Number" />,
      cell: ({ row }) => (
        <span className="font-medium">{row.original.number ?? "—"}</span>
      ),
    },
    {
      accessorKey: "roomTypeName",
      header: ({ column }) => <DataTableColumnHeader column={column} title="Type" />,
      cell: ({ row }) => row.original.roomTypeName ?? "—",
    },
    {
      accessorKey: "basePrice",
      header: ({ column }) => (
        <DataTableColumnHeader column={column} title="Base price" />
      ),
      cell: ({ row }) => money2(row.original.basePrice),
    },
    {
      accessorKey: "capacity",
      header: "Capacity",
      cell: ({ row }) => row.original.capacity ?? "—",
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
              <DropdownMenuItem onClick={() => onPrice(row.original)}>
                <Tag className="h-4 w-4" /> Update price
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ];
}

export function RoomsTable({
  data,
  pagination,
}: {
  data: Room[];
  pagination?: ServerTableConfig;
}) {
  const [pricing, setPricing] = useState<Room | null>(null);
  const [price, setPrice] = useState("");
  const [pending, startTransition] = useTransition();

  function openPrice(r: Room) {
    setPrice(r.basePrice != null ? String(r.basePrice) : "");
    setPricing(r);
  }

  function submitPrice() {
    const target = pricing;
    const value = Number(price);
    if (!target || Number.isNaN(value) || value < 0) {
      toast.error("Enter a valid price");
      return;
    }
    startTransition(async () => {
      const result = await updateRoomPriceAction(target.id, value);
      if (result.ok) {
        toast.success(`Price updated for room ${target.number ?? ""}`.trim());
        setPricing(null);
      } else {
        toast.error(result.error ?? "Could not update price");
      }
    });
  }

  return (
    <>
      <DataTable
        columns={roomColumns(openPrice)}
        data={data}
        searchPlaceholder="Search rooms…"
        exportFileName="rooms.csv"
        serverPagination={pagination}
        emptyState={
          <EmptyState
            icon={BedDouble}
            title="No rooms"
            description="Rooms will appear here once configured for this tenant."
          />
        }
      />

      <Dialog open={!!pricing} onOpenChange={(o) => !o && setPricing(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Update price</DialogTitle>
            <DialogDescription>
              Set a new base price for room{" "}
              <span className="font-medium">{pricing?.number}</span>.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-2">
            <Label htmlFor="room-price">Base price</Label>
            <Input
              id="room-price"
              type="number"
              min={0}
              step="0.01"
              value={price}
              onChange={(e) => setPrice(e.target.value)}
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setPricing(null)}>
              Cancel
            </Button>
            <Button onClick={submitPrice} disabled={pending}>
              Save
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
