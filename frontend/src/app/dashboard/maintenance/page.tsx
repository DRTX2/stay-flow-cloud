import type { Metadata } from "next";
import { Wrench } from "lucide-react";
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
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ActionForm, ActionSubmit } from "@/components/shared/ActionForm";
import { getPaged } from "@/server/api";
import type { Room, WorkOrder } from "@/types/api";
import { createWorkOrderAction, resolveWorkOrderAction } from "./actions";

export const metadata: Metadata = { title: "Maintenance | StayFlow" };

export default async function MaintenancePage() {
  const [workOrders, rooms] = await Promise.all([
    getPaged<WorkOrder>("/api/v1/maintenance/work-orders", { pageSize: 50 }),
    getPaged<Room>("/api/v1/rooms", { pageSize: 100 }),
  ]);
  const roomsById = new Map(rooms.items.map((room) => [room.id, room]));

  return (
    <div className="space-y-6">
      <PageHeader
        title="Maintenance"
        description="Put rooms under maintenance or out of service, then resolve work orders."
      />

      <Card>
        <CardHeader>
          <CardTitle>Create work order</CardTitle>
          <CardDescription>
            Urgent room work orders mark the room out of service.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ActionForm
            action={createWorkOrderAction}
            className="grid gap-3 md:grid-cols-[1fr_1fr_2fr_auto] md:items-end"
          >
            <div className="space-y-2">
              <Label htmlFor="maintenance-room">Room</Label>
              <select
                id="maintenance-room"
                name="roomId"
                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
              >
                <option value="">Property-level</option>
                {rooms.items.map((room) => (
                  <option key={room.id} value={room.id}>
                    {room.number} · {room.status ?? "Unknown"}
                  </option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="maintenance-priority">Priority</Label>
              <select
                id="maintenance-priority"
                name="priority"
                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                defaultValue="Medium"
              >
                <option>Low</option>
                <option>Medium</option>
                <option>High</option>
                <option>Urgent</option>
              </select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="maintenance-description">Issue</Label>
              <Input
                id="maintenance-description"
                name="description"
                placeholder="Describe the issue"
                required
              />
            </div>
            <ActionSubmit pendingLabel="Opening…">Open</ActionSubmit>
          </ActionForm>
        </CardContent>
      </Card>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Room availability impact</CardTitle>
            <CardDescription>
              Maintenance statuses removed from sellable inventory.
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3 sm:grid-cols-2">
            {rooms.items.map((room) => (
              <div key={room.id} className="rounded-lg border p-3">
                <div className="flex items-center justify-between gap-3">
                  <p className="font-medium">Room {room.number}</p>
                  <StatusBadge status={room.status} />
                </div>
                <p className="mt-1 text-xs text-muted-foreground">
                  Cleaning: {room.cleaningStatus ?? "Unknown"}
                </p>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Work orders</CardTitle>
            <CardDescription>
              Resolve orders to restore room service when safe.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {workOrders.items.length === 0 ? (
              <div className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground">
                <Wrench className="mx-auto mb-2 h-8 w-8" />
                No maintenance work orders.
              </div>
            ) : (
              workOrders.items.map((workOrder) => {
                const room = workOrder.roomId ? roomsById.get(workOrder.roomId) : null;
                return (
                  <div key={workOrder.id} className="rounded-lg border p-3">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="font-medium">
                          {workOrder.description ?? "Work order"}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {room ? `Room ${room.number}` : "Property-level"} ·{" "}
                          {workOrder.priority ?? "Medium"}
                        </p>
                      </div>
                      <Badge variant="outline">{workOrder.status ?? "Open"}</Badge>
                    </div>
                    {workOrder.status !== "Resolved" &&
                      workOrder.status !== "Cancelled" && (
                        <ActionForm
                          action={resolveWorkOrderAction}
                          className="mt-3 flex flex-wrap items-center gap-2"
                        >
                          <input type="hidden" name="id" value={workOrder.id} />
                          <Label
                            htmlFor={`resolution-notes-${workOrder.id}`}
                            className="sr-only"
                          >
                            Resolution notes
                          </Label>
                          <Input
                            id={`resolution-notes-${workOrder.id}`}
                            name="notes"
                            placeholder="Resolution notes"
                            className="max-w-xs"
                          />
                          <ActionSubmit size="sm" pendingLabel="Resolving…">
                            Resolve
                          </ActionSubmit>
                        </ActionForm>
                      )}
                  </div>
                );
              })
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
