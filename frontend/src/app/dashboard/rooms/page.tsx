import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { Room, RoomType } from "@/types/api";
import { RoomsTable } from "@/features/rooms/RoomsTable";
import { CreateRoomDialog } from "@/features/rooms/CreateRoomDialog";

export const metadata: Metadata = { title: "Rooms" };

export default async function RoomsPage() {
  const [rooms, roomTypes] = await Promise.all([
    getList<Room>("/api/v1/rooms"),
    getList<RoomType>("/api/v1/roomtypes"),
  ]);
  return (
    <div className="space-y-6">
      <PageHeader
        title="Rooms"
        description="Inventory and live room status."
        actions={<CreateRoomDialog roomTypes={roomTypes} />}
      />
      <RoomsTable data={rooms} />
    </div>
  );
}
