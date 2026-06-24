import { useQuery } from "@tanstack/react-query";
import { getList } from "@/services/list";
import type { RoomType } from "@/types/api";

export function useRoomTypes() {
  return useQuery({
    queryKey: ["room-types"],
    queryFn: () => getList<RoomType>("/api/v1/roomtypes"),
  });
}
