import { useQuery } from "@tanstack/react-query";
import { getList } from "@/services/list";
import type { DocumentItem } from "@/types/api";

export function useDocuments() {
  return useQuery({
    queryKey: ["documents"],
    queryFn: () => getList<DocumentItem>("/api/v1/documents"),
  });
}
