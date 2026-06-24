import { useQuery } from "@tanstack/react-query";
import { getList } from "@/services/list";
import type { Guest } from "@/types/api";

export const guestsKey = ["guests"] as const;

export function useGuests() {
  return useQuery({
    queryKey: guestsKey,
    queryFn: () => getList<Guest>("/api/v1/guests"),
  });
}
