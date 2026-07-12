"use client";

import { useState, useTransition } from "react";
import { CalendarX } from "lucide-react";
import { toast } from "sonner";
import type { Reservation } from "@/types/api";
import type { ActionResult } from "@/server/actions";
import { EmptyState } from "@/components/shared/EmptyState";
import {
  DataTable,
  type ServerTableConfig,
} from "@/components/shared/data-table/DataTable";
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
  checkInReservationAction,
  checkOutReservationAction,
  confirmReservationAction,
  generateInvoiceAction,
  createFeedbackInvitationAction,
} from "@/app/dashboard/reservations/actions";
import { reservationColumns } from "./columns";

export function ReservationsTable({
  data,
  pagination,
}: {
  data: Reservation[];
  pagination?: ServerTableConfig;
}) {
  const [toCancel, setToCancel] = useState<Reservation | null>(null);
  const [pending, startTransition] = useTransition();

  function run(action: () => Promise<ActionResult>, success: string, failure: string) {
    startTransition(async () => {
      const result = await action();
      if (result.ok) toast.success(success);
      else toast.error(result.error ?? failure);
    });
  }

  function confirmCancel() {
    const target = toCancel;
    setToCancel(null);
    if (!target) return;
    run(
      () => cancelReservationAction(target.id),
      "Reservation cancelled",
      "Could not cancel reservation",
    );
  }

  const columns = reservationColumns({
    onConfirm: (r) =>
      run(
        () => confirmReservationAction(r.id),
        "Reservation confirmed",
        "Could not confirm reservation",
      ),
    onCheckIn: (r) =>
      run(() => checkInReservationAction(r.id), "Guest checked in", "Could not check in"),
    onCheckOut: (r) =>
      run(
        () => checkOutReservationAction(r.id),
        "Guest checked out",
        "Could not check out",
      ),
    onInvoice: (r) =>
      run(
        () => generateInvoiceAction(r.id),
        "Invoice generated",
        "Could not generate invoice",
      ),
    onFeedback: (r) =>
      startTransition(async () => {
        const result = await createFeedbackInvitationAction(r.id);
        if (!result.ok || !result.token) {
          toast.error(result.error ?? "Could not create feedback link");
          return;
        }
        const link = `${window.location.origin}/feedback#${result.token}`;
        try {
          await navigator.clipboard.writeText(link);
          toast.success("Feedback link copied. It expires in 30 days.");
        } catch {
          toast.error("The browser blocked clipboard access. Please try again.");
        }
      }),
    onCancel: setToCancel,
    pending,
  });

  return (
    <>
      <DataTable
        columns={columns}
        data={data}
        searchPlaceholder="Search reservations…"
        exportFileName="reservations.csv"
        serverPagination={pagination}
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
