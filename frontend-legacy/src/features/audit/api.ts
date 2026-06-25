import { useQuery } from "@tanstack/react-query";
import { getList } from "@/services/list";
import type { AuditEntry } from "@/types/api";

export function useAudit() {
  return useQuery({
    queryKey: ["audit"],
    queryFn: () => getList<AuditEntry>("/api/v1/audit"),
  });
}
