import { useQuery } from "@tanstack/react-query";
import { api } from "./client";
import type {
  DashboardSummary,
  Guest,
  Reservation,
  RevenuePoint,
  Room,
  ServiceItem,
} from "./types";

// The API returns either a bare array or a paged envelope ({ items: [...] }); normalize to an array.
function unwrapList<T>(data: unknown): T[] {
  if (Array.isArray(data)) return data as T[];
  if (data && typeof data === "object" && "items" in data) {
    const items = (data as { items: unknown }).items;
    if (Array.isArray(items)) return items as T[];
  }
  return [];
}

async function getList<T>(url: string): Promise<T[]> {
  const { data } = await api.get(url);
  return unwrapList<T>(data);
}

export function useDashboard() {
  return useQuery({
    queryKey: ["dashboard"],
    queryFn: async () => {
      const { data } = await api.get<DashboardSummary>(
        "/api/v1/analytics/dashboard",
      );
      return data;
    },
  });
}

export function useRevenue() {
  return useQuery({
    queryKey: ["revenue"],
    queryFn: () => getList<RevenuePoint>("/api/v1/analytics/revenue"),
  });
}

export function useReservations() {
  return useQuery({
    queryKey: ["reservations"],
    queryFn: () => getList<Reservation>("/api/v1/reservations"),
  });
}

export function useRooms() {
  return useQuery({
    queryKey: ["rooms"],
    queryFn: () => getList<Room>("/api/v1/rooms"),
  });
}

export function useGuests() {
  return useQuery({
    queryKey: ["guests"],
    queryFn: () => getList<Guest>("/api/v1/guests"),
  });
}

export function useServices() {
  return useQuery({
    queryKey: ["services"],
    queryFn: () => getList<ServiceItem>("/api/v1/services"),
  });
}
