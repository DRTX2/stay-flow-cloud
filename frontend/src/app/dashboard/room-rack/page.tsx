import type { Metadata } from "next";
import { CalendarDays } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { getJson } from "@/server/api";
import type { RoomRack } from "@/types/api";

export const metadata: Metadata = { title: "Room Rack | StayFlow" };

export default async function RoomRackPage() {
  const rack = await getJson<RoomRack>("/api/v1/analytics/room-rack");

  return (
    <div className="space-y-6">
      <PageHeader
        title="Room Rack"
        description={`Reservations and room readiness from ${rack.from ?? "today"} to ${rack.to ?? "the next 14 days"}.`}
      />

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CalendarDays className="h-5 w-5" /> Rack view
          </CardTitle>
          <CardDescription>
            Use this to spot occupancy, room blocks, dirty rooms, and out-of-service
            inventory.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {(rack.rooms ?? []).map((room) => (
            <div key={room.roomId} className="rounded-lg border p-3">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="font-medium">Room {room.roomNumber ?? "TBD"}</p>
                  <p className="text-xs text-muted-foreground">
                    {room.roomTypeName ?? "Room"}
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <StatusBadge status={room.roomStatus} />
                  <StatusBadge status={room.cleaningStatus} />
                </div>
              </div>
              <div className="mt-3 grid gap-2 md:grid-cols-2 xl:grid-cols-3">
                {(room.reservations ?? []).length === 0 ? (
                  <div className="rounded-md border border-dashed p-3 text-sm text-muted-foreground">
                    No reservations in range.
                  </div>
                ) : (
                  room.reservations?.map((reservation) => (
                    <div
                      key={reservation.reservationId}
                      className="rounded-md bg-muted/50 p-3"
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="font-medium">
                            {reservation.guestName ?? "Guest"}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            {reservation.checkIn} to {reservation.checkOut}
                          </p>
                        </div>
                        <Badge variant="outline">{reservation.status ?? "Pending"}</Badge>
                      </div>
                      <p className="mt-2 text-xs text-muted-foreground">
                        {reservation.confirmationCode}
                      </p>
                    </div>
                  ))
                )}
              </div>
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  );
}
