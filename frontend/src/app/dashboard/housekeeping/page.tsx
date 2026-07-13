import type { Metadata } from "next";
import { ClipboardList } from "lucide-react";
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
import type { HousekeepingTask, Room } from "@/types/api";
import { completeHousekeepingTaskAction, createHousekeepingTaskAction } from "./actions";

export const metadata: Metadata = { title: "Housekeeping | StayFlow" };

export default async function HousekeepingPage() {
  const [tasks, rooms] = await Promise.all([
    getPaged<HousekeepingTask>("/api/v1/housekeeping/tasks", { pageSize: 50 }),
    getPaged<Room>("/api/v1/rooms", { pageSize: 100 }),
  ]);
  const roomsById = new Map(rooms.items.map((room) => [room.id, room]));

  return (
    <div className="space-y-6">
      <PageHeader
        title="Housekeeping"
        description="Create room-clean tasks and clear rooms back to inspected/clean."
      />

      <Card>
        <CardHeader>
          <CardTitle>Create task</CardTitle>
          <CardDescription>
            Opening a housekeeping task marks the room dirty.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ActionForm
            action={createHousekeepingTaskAction}
            className="grid gap-3 md:grid-cols-[1fr_1fr_2fr_auto] md:items-end"
          >
            <div className="space-y-2">
              <Label htmlFor="housekeeping-room">Room</Label>
              <select
                id="housekeeping-room"
                name="roomId"
                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                required
              >
                <option value="">Select a room</option>
                {rooms.items.map((room) => (
                  <option key={room.id} value={room.id}>
                    {room.number} · {room.cleaningStatus ?? "Unknown"}
                  </option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="housekeeping-task-type">Task type</Label>
              <select
                id="housekeeping-task-type"
                name="taskType"
                className="h-10 w-full rounded-md border bg-background px-3 text-sm"
                defaultValue="Daily Clean"
              >
                <option>Daily Clean</option>
                <option>Departure Clean</option>
                <option>Deep Clean</option>
                <option>Turn Down Service</option>
              </select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="housekeeping-notes">Notes</Label>
              <Input
                id="housekeeping-notes"
                name="notes"
                placeholder="Notes for the attendant"
              />
            </div>
            <ActionSubmit pendingLabel="Adding…">Add task</ActionSubmit>
          </ActionForm>
        </CardContent>
      </Card>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Room board</CardTitle>
            <CardDescription>Live physical room and cleaning status.</CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3 sm:grid-cols-2">
            {rooms.items.map((room) => (
              <div key={room.id} className="rounded-lg border p-3">
                <div className="flex items-center justify-between gap-3">
                  <p className="font-medium">Room {room.number}</p>
                  <StatusBadge status={room.cleaningStatus} />
                </div>
                <p className="mt-1 text-xs text-muted-foreground">
                  {room.roomTypeName ?? "Room"} · {room.status ?? "Unknown"}
                </p>
              </div>
            ))}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Open tasks</CardTitle>
            <CardDescription>Complete tasks after inspection.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {tasks.items.length === 0 ? (
              <div className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground">
                <ClipboardList className="mx-auto mb-2 h-8 w-8" />
                No housekeeping tasks.
              </div>
            ) : (
              tasks.items.map((task) => {
                const room = roomsById.get(task.roomId);
                return (
                  <div key={task.id} className="rounded-lg border p-3">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="font-medium">{task.taskType ?? "Task"}</p>
                        <p className="text-xs text-muted-foreground">
                          Room {room?.number ?? task.roomId} · {task.notes ?? "No notes"}
                        </p>
                      </div>
                      <Badge variant="outline">{task.status ?? "Pending"}</Badge>
                    </div>
                    {task.status !== "Completed" && (
                      <ActionForm
                        action={completeHousekeepingTaskAction}
                        className="mt-3 flex flex-wrap items-center gap-2"
                      >
                        <input type="hidden" name="id" value={task.id} />
                        <Label htmlFor={`cleaning-status-${task.id}`} className="sr-only">
                          Final cleaning status
                        </Label>
                        <select
                          id={`cleaning-status-${task.id}`}
                          name="cleaningStatus"
                          className="h-9 rounded-md border bg-background px-3 text-sm"
                          defaultValue="Inspected"
                        >
                          <option>Inspected</option>
                          <option>Clean</option>
                        </select>
                        <ActionSubmit size="sm" pendingLabel="Completing…">
                          Complete
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
