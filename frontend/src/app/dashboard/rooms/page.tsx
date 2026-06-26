import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList, getPaged } from "@/server/api";
import { parsePageParams, type SearchParams } from "@/lib/pagination";
import type { Room, RoomType } from "@/types/api";
import { RoomsTable } from "@/features/rooms/RoomsTable";
import { CreateRoomDialog } from "@/features/rooms/CreateRoomDialog";

export const metadata: Metadata = { title: "Rooms" };

export default async function RoomsPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  const { page, pageSize } = parsePageParams(await searchParams);
  const [rooms, roomTypes] = await Promise.all([
    getPaged<Room>("/api/v1/rooms", { page, pageSize }),
    getList<RoomType>("/api/v1/roomtypes"),
  ]);
  return (
    <div className="space-y-6">
      <PageHeader
        title="Rooms"
        description="Inventory and live room status."
        actions={<CreateRoomDialog roomTypes={roomTypes} />}
      />
      <RoomsTable
        data={rooms.items}
        pagination={{
          page: rooms.page,
          pageSize: rooms.pageSize,
          totalCount: rooms.totalCount,
          totalPages: rooms.totalPages,
        }}
      />
    </div>
  );
}
