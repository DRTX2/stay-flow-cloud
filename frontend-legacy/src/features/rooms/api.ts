import { useQuery } from "@tanstack/react-query";
import { getList } from "@/services/list";
import type { Room } from "@/types/api";

export const roomsKey = ["rooms"] as const;

export function useRooms() {
  return useQuery({
    queryKey: roomsKey,
    queryFn: () => getList<Room>("/api/v1/rooms"),
  });
}
