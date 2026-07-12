"use client";

import { useState, useTransition } from "react";
import { Loader2, Plus, Utensils } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { EmptyState } from "@/components/shared/EmptyState";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { money } from "@/lib/format";
import type { Order, Reservation, ServiceItem } from "@/types/api";
import { createOrderAction, transitionOrderAction } from "@/app/dashboard/orders/actions";

function CreateOrderForm({
  reservations,
  services,
}: {
  reservations: Reservation[];
  services: ServiceItem[];
}) {
  const [pending, startTransition] = useTransition();
  const [reservationId, setReservationId] = useState("");
  const [serviceItemId, setServiceItemId] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [notes, setNotes] = useState("");

  function submit() {
    startTransition(async () => {
      const result = await createOrderAction({
        reservationId,
        serviceItemId,
        quantity,
        notes,
      });
      if (result.ok) {
        toast.success("Order placed");
        setServiceItemId("");
        setNotes("");
        setQuantity(1);
      } else toast.error(result.error);
    });
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <Plus className="h-4 w-4" />
          Place room order
        </CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
        <div className="space-y-2 xl:col-span-2">
          <Label htmlFor="order-reservation">Checked-in reservation</Label>
          <Select
            value={reservationId}
            onValueChange={setReservationId}
            disabled={pending}
          >
            <SelectTrigger id="order-reservation">
              <SelectValue placeholder="Select reservation" />
            </SelectTrigger>
            <SelectContent>
              {reservations.map((reservation) => (
                <SelectItem key={reservation.id} value={reservation.id}>
                  {reservation.confirmationCode ?? reservation.id} · Room{" "}
                  {reservation.roomNumber ?? "TBD"}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-2">
          <Label htmlFor="order-service">Service</Label>
          <Select
            value={serviceItemId}
            onValueChange={setServiceItemId}
            disabled={pending}
          >
            <SelectTrigger id="order-service">
              <SelectValue placeholder="Select item" />
            </SelectTrigger>
            <SelectContent>
              {services
                .filter((service) => service.isActive !== false)
                .map((service) => (
                  <SelectItem key={service.id} value={service.id}>
                    {service.name} · {money(service.price)}
                  </SelectItem>
                ))}
            </SelectContent>
          </Select>
        </div>
        <div className="space-y-2">
          <Label htmlFor="order-quantity">Quantity</Label>
          <Input
            id="order-quantity"
            type="number"
            min={1}
            max={50}
            value={quantity}
            onChange={(event) => setQuantity(Number(event.target.value))}
            disabled={pending}
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="order-notes">Notes</Label>
          <Input
            id="order-notes"
            value={notes}
            onChange={(event) => setNotes(event.target.value)}
            placeholder="Dietary needs, delivery notes"
            disabled={pending}
          />
        </div>
        <Button
          className="md:col-span-2 xl:col-span-5"
          onClick={submit}
          disabled={pending || !reservationId || !serviceItemId || quantity < 1}
          aria-busy={pending}
        >
          {pending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}Place order
        </Button>
      </CardContent>
    </Card>
  );
}

function OrderCard({ order }: { order: Order }) {
  const [pending, startTransition] = useTransition();
  const status = order.status ?? "Pending";
  function transition(action: "prepare" | "deliver" | "cancel") {
    startTransition(async () => {
      const result = await transitionOrderAction(order.id, action);
      result.ok
        ? toast.success(
            `Order ${action === "prepare" ? "is being prepared" : action === "deliver" ? "delivered" : "cancelled"}`,
          )
        : toast.error(result.error);
    });
  }
  return (
    <Card>
      <CardHeader className="flex-row items-start justify-between gap-3">
        <div>
          <CardTitle className="text-base">
            Order {order.id.slice(0, 8).toUpperCase()}
          </CardTitle>
          <p className="mt-1 text-xs text-muted-foreground">
            Reservation {order.reservationId}
          </p>
        </div>
        <StatusBadge status={status} />
      </CardHeader>
      <CardContent className="space-y-4">
        <ul className="space-y-2 text-sm">
          {(order.items ?? []).map((item) => (
            <li key={item.serviceItemId} className="flex justify-between gap-3">
              <span>
                {item.quantity} × {item.serviceName}
              </span>
              <span>{money(item.total)}</span>
            </li>
          ))}
        </ul>
        {order.notes && <p className="rounded-md bg-muted p-3 text-sm">{order.notes}</p>}
        <div className="flex items-center justify-between border-t pt-3">
          <span className="font-medium">Total</span>
          <span className="font-semibold">{money(order.totalAmount)}</span>
        </div>
        {(status === "Pending" || status === "Preparing") && (
          <div className="grid gap-2 sm:grid-cols-2">
            {status === "Pending" && (
              <Button onClick={() => transition("prepare")} disabled={pending}>
                Start preparing
              </Button>
            )}
            {status === "Preparing" && (
              <Button onClick={() => transition("deliver")} disabled={pending}>
                Mark delivered
              </Button>
            )}
            <Button
              variant="outline"
              onClick={() => transition("cancel")}
              disabled={pending}
            >
              Cancel
            </Button>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export function OrdersView({
  orders,
  reservations,
  services,
}: {
  orders: Order[];
  reservations: Reservation[];
  services: ServiceItem[];
}) {
  return (
    <div className="space-y-6">
      <CreateOrderForm reservations={reservations} services={services} />
      {orders.length === 0 ? (
        <EmptyState
          icon={Utensils}
          title="No room orders"
          description="Place the first food, beverage or amenity order for a checked-in guest."
        />
      ) : (
        <div className="grid gap-4 xl:grid-cols-2">
          {orders.map((order) => (
            <OrderCard key={order.id} order={order} />
          ))}
        </div>
      )}
    </div>
  );
}
