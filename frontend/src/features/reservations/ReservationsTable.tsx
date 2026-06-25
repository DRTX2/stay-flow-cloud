"use client";

import { useState, useTransition } from "react";
import { CalendarX } from "lucide-react";
import { toast } from "sonner";
import type { Reservation } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
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
  cancelReservationAction,
  generateInvoiceAction,
} from "@/app/dashboard/reservations/actions";
import { reservationColumns } from "./columns";

export function ReservationsTable({ data }: { data: Reservation[] }) {
  const [toCancel, setToCancel] = useState<Reservation | null>(null);
  const [pending, startTransition] = useTransition();

  function handleInvoice(r: Reservation) {
    startTransition(async () => {
      const result = await generateInvoiceAction(r.id);
      if (result.ok) toast.success("Invoice generated");
      else toast.error(result.error ?? "Could not generate invoice");
    });
  }

  function confirmCancel() {
    const target = toCancel;
    setToCancel(null);
    if (!target) return;
    startTransition(async () => {
      const result = await cancelReservationAction(target.id);
      if (result.ok) toast.success("Reservation cancelled");
      else toast.error(result.error ?? "Could not cancel reservation");
    });
  }

  const columns = reservationColumns({
    onCancel: setToCancel,
    onInvoice: handleInvoice,
    pending,
  });

  return (
    <>
      <DataTable
        columns={columns}
        data={data}
        searchPlaceholder="Search reservations…"
        exportFileName="reservations.csv"
        emptyState={
          <EmptyState
            icon={CalendarX}
            title="No reservations yet"
            description="Create your first reservation to see it here."
          />
        }
      />

      <Dialog open={!!toCancel} onOpenChange={(o) => !o && setToCancel(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancel reservation?</DialogTitle>
            <DialogDescription>
              This cancels the booking for{" "}
              <span className="font-medium">{toCancel?.guestName}</span>. This action
              cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setToCancel(null)}>
              Keep it
            </Button>
            <Button variant="destructive" onClick={confirmCancel} disabled={pending}>
              Cancel reservation
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
