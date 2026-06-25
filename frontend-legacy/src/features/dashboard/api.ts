import { useQuery } from "@tanstack/react-query";
import { http } from "@/services/http";
import { getList } from "@/services/list";
import type { DashboardSummary, RevenuePoint } from "@/types/api";

export function useDashboard() {
  return useQuery({
    queryKey: ["analytics", "dashboard"],
    queryFn: async () => {
      const { data } = await http.get<DashboardSummary>("/api/v1/analytics/dashboard");
      return data;
    },
  });
}

export function useRevenue() {
  return useQuery({
    queryKey: ["analytics", "revenue"],
    queryFn: () => getList<RevenuePoint>("/api/v1/analytics/revenue"),
  });
}
