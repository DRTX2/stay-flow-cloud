import { useQuery } from "@tanstack/react-query";
import { getList } from "@/services/list";
import type { ServiceItem } from "@/types/api";

export function useServices() {
  return useQuery({
    queryKey: ["services"],
    queryFn: () => getList<ServiceItem>("/api/v1/services"),
  });
}
