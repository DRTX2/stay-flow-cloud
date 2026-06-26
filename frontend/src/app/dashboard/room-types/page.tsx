import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { RoomType } from "@/types/api";
import { RoomTypesTable } from "@/features/room-types/RoomTypesTable";
import { RoomTypeFormDialog } from "@/features/room-types/RoomTypeFormDialog";

export const metadata: Metadata = { title: "Room Types" };

export default async function RoomTypesPage() {
  const roomTypes = await getList<RoomType>("/api/v1/roomtypes");
  return (
    <div className="space-y-6">
      <PageHeader
        title="Room Types"
        description="Rate plans and occupancy templates."
        actions={<RoomTypeFormDialog />}
      />
      <RoomTypesTable data={roomTypes} />
    </div>
  );
}
