import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { Room } from "@/types/api";
import { RoomsTable } from "@/features/rooms/RoomsTable";

export const metadata: Metadata = { title: "Rooms" };

export default async function RoomsPage() {
  const rooms = await getList<Room>("/api/v1/rooms");
  return (
    <div className="space-y-6">
      <PageHeader title="Rooms" description="Inventory and live room status." />
      <RoomsTable data={rooms} />
    </div>
  );
}
