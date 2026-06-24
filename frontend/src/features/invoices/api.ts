import { useQuery } from "@tanstack/react-query";
import { getList } from "@/services/list";
import type { Invoice } from "@/types/api";

export function useInvoices() {
  return useQuery({
    queryKey: ["invoices"],
    queryFn: () => getList<Invoice>("/api/v1/invoices"),
  });
}
