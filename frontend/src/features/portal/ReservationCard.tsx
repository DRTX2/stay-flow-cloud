import { BedDouble, Calendar, Users } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { formatDate, money } from "@/lib/format";
import type { Reservation } from "@/types/api";

export function ReservationCard({ reservation }: { reservation: Reservation }) {
  const r = reservation;
  return (
    <Card className="transition-shadow hover:shadow-md">
      <CardContent className="p-5">
        <div className="flex items-start justify-between gap-4">
          <div className="space-y-3">
            {/* Confirmation code + status */}
            <div className="flex items-center gap-3">
              {r.confirmationCode && (
                <span className="font-mono text-sm font-medium text-muted-foreground">
                  {r.confirmationCode}
                </span>
              )}
              <StatusBadge status={r.status} />
            </div>

            {/* Dates */}
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Calendar className="h-4 w-4 shrink-0" />
              <span>
                {formatDate(r.checkIn)} → {formatDate(r.checkOut)}
              </span>
              {r.nights != null && (
                <span className="text-xs">
                  ({r.nights} night{r.nights !== 1 ? "s" : ""})
                </span>
              )}
            </div>

            {/* Room */}
            {r.roomNumber && (
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <BedDouble className="h-4 w-4 shrink-0" />
                <span>Room {r.roomNumber}</span>
              </div>
            )}

            {/* Guests */}
            {r.numberOfGuests != null && (
              <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <Users className="h-4 w-4 shrink-0" />
                <span>
                  {r.numberOfGuests} guest{r.numberOfGuests !== 1 ? "s" : ""}
                </span>
              </div>
            )}
          </div>

          {/* Price */}
          <div className="text-right">
            <div className="text-lg font-semibold tracking-tight">
              {money(r.total ?? r.totalPrice)}
            </div>
            <span className="text-xs text-muted-foreground">total</span>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
