"use client";

import { useState, useTransition } from "react";
import { CalendarDays, Mail, UserRound } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { EmptyState } from "@/components/shared/EmptyState";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { formatDate } from "@/lib/format";
import type { BookingEnquiry, Room } from "@/types/api";
import {
  convertBookingEnquiryAction,
  rejectBookingEnquiryAction,
} from "@/app/dashboard/booking-enquiries/actions";

function EnquiryCard({ enquiry, rooms }: { enquiry: BookingEnquiry; rooms: Room[] }) {
  const [pending, startTransition] = useTransition();
  const [roomId, setRoomId] = useState("");
  const [reason, setReason] = useState("");
  const compatibleRooms = rooms.filter((room) => room.roomTypeId === enquiry.roomTypeId);
  const isPending = enquiry.status.toLowerCase() === "pending";

  function convert() {
    startTransition(async () => {
      const result = await convertBookingEnquiryAction(enquiry.id, roomId);
      result.ok
        ? toast.success("Enquiry converted to a reservation")
        : toast.error(result.error);
    });
  }

  function reject() {
    startTransition(async () => {
      const result = await rejectBookingEnquiryAction(enquiry.id, reason);
      result.ok ? toast.success("Enquiry rejected") : toast.error(result.error);
    });
  }

  return (
    <Card>
      <CardHeader className="gap-3 pb-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <CardTitle className="text-base">{enquiry.fullName}</CardTitle>
          <p className="mt-1 font-mono text-xs text-muted-foreground">
            {enquiry.reference}
          </p>
        </div>
        <StatusBadge status={enquiry.status} />
      </CardHeader>
      <CardContent className="space-y-4">
        <dl className="grid gap-3 text-sm sm:grid-cols-2">
          <div className="flex gap-2">
            <Mail className="mt-0.5 h-4 w-4 text-muted-foreground" />
            <div>
              <dt className="sr-only">Email</dt>
              <dd className="break-all">{enquiry.email}</dd>
            </div>
          </div>
          <div className="flex gap-2">
            <UserRound className="mt-0.5 h-4 w-4 text-muted-foreground" />
            <div>
              <dt className="sr-only">Guests and room type</dt>
              <dd>
                {enquiry.numberOfGuests} guest(s), {enquiry.roomTypeName}
              </dd>
            </div>
          </div>
          <div className="flex gap-2 sm:col-span-2">
            <CalendarDays className="mt-0.5 h-4 w-4 text-muted-foreground" />
            <div>
              <dt className="sr-only">Stay dates</dt>
              <dd>
                {formatDate(enquiry.checkIn)} to {formatDate(enquiry.checkOut)}
              </dd>
            </div>
          </div>
        </dl>

        {enquiry.rejectionReason && (
          <p className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
            {enquiry.rejectionReason}
          </p>
        )}
        {enquiry.reservationId && (
          <p className="text-sm text-muted-foreground">
            Reservation created:{" "}
            <span className="font-mono">{enquiry.reservationId}</span>
          </p>
        )}

        {isPending && (
          <div className="grid gap-4 border-t pt-4 lg:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor={`room-${enquiry.id}`}>Assign compatible room</Label>
              <Select value={roomId} onValueChange={setRoomId} disabled={pending}>
                <SelectTrigger id={`room-${enquiry.id}`}>
                  <SelectValue placeholder="Select room" />
                </SelectTrigger>
                <SelectContent>
                  {compatibleRooms.map((room) => (
                    <SelectItem key={room.id} value={room.id}>
                      Room {room.number} · capacity {room.capacity}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Button
                className="w-full"
                onClick={convert}
                disabled={pending || !roomId}
                aria-busy={pending}
              >
                Convert to reservation
              </Button>
              {compatibleRooms.length === 0 && (
                <p className="text-xs text-destructive">
                  No rooms of this type are configured.
                </p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor={`reason-${enquiry.id}`}>Rejection reason</Label>
              <Input
                id={`reason-${enquiry.id}`}
                value={reason}
                onChange={(event) => setReason(event.target.value)}
                placeholder="Optional reason"
                disabled={pending}
              />
              <Button
                className="w-full"
                variant="outline"
                onClick={reject}
                disabled={pending}
                aria-busy={pending}
              >
                Reject enquiry
              </Button>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export function BookingEnquiriesView({
  enquiries,
  rooms,
}: {
  enquiries: BookingEnquiry[];
  rooms: Room[];
}) {
  if (enquiries.length === 0) {
    return (
      <EmptyState
        title="No booking enquiries"
        description="New requests from the public booking form will appear here."
      />
    );
  }
  return (
    <div className="grid gap-4 xl:grid-cols-2">
      {enquiries.map((enquiry) => (
        <EnquiryCard key={enquiry.id} enquiry={enquiry} rooms={rooms} />
      ))}
    </div>
  );
}
