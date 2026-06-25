import { useMemo, useState } from "react";
import { CalendarX } from "lucide-react";
import type { Reservation } from "@/types/api";
import { PageHeader } from "@/components/shared/PageHeader";
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
import { useCancelReservation, useGenerateInvoice, useReservations } from "./api";
import { reservationColumns } from "./columns";
import { CreateReservationDialog } from "./CreateReservationDialog";

export function ReservationsPage() {
  const { data, isLoading } = useReservations();
  const cancel = useCancelReservation();
  const invoice = useGenerateInvoice();
  const [toCancel, setToCancel] = useState<Reservation | null>(null);

  const columns = useMemo(
    () =>
      reservationColumns({
        onCancel: setToCancel,
        onInvoice: (r) => invoice.mutate(r.id),
      }),
    [invoice],
  );

  return (
    <div className="space-y-6">
      <PageHeader
        title="Reservations"
        description="Manage bookings across the property."
        actions={<CreateReservationDialog />}
      />

      <DataTable
        columns={columns}
        data={data ?? []}
        isLoading={isLoading}
        searchPlaceholder="Search reservations…"
        exportFileName="reservations.csv"
        emptyState={
          <EmptyState
            icon={CalendarX}
            title="No reservations yet"
            description="Create your first reservation to see it here."
            action={<CreateReservationDialog />}
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
            <Button
              variant="destructive"
              onClick={() => {
                if (toCancel) cancel.mutate(toCancel.id);
                setToCancel(null);
              }}
            >
              Cancel reservation
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
