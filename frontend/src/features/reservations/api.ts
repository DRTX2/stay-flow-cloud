import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { http } from "@/services/http";
import { getList } from "@/services/list";
import type { CreateReservationRequest, Reservation } from "@/types/api";

export const reservationsKey = ["reservations"] as const;

export function useReservations() {
  return useQuery({
    queryKey: reservationsKey,
    queryFn: () => getList<Reservation>("/api/v1/reservations"),
  });
}

export function useCreateReservation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (body: CreateReservationRequest) => {
      const { data } = await http.post<Reservation>("/api/v1/reservations", body);
      return data;
    },
    onSuccess: () => {
      toast.success("Reservation created");
      void qc.invalidateQueries({ queryKey: reservationsKey });
      void qc.invalidateQueries({ queryKey: ["analytics"] });
    },
    onError: () => toast.error("Could not create reservation"),
  });
}

export function useCancelReservation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await http.post(`/api/v1/reservations/${id}/cancel`);
    },
    // Optimistically mark the row as Cancelled.
    onMutate: async (id) => {
      await qc.cancelQueries({ queryKey: reservationsKey });
      const prev = qc.getQueryData<Reservation[]>(reservationsKey);
      qc.setQueryData<Reservation[]>(reservationsKey, (old) =>
        (old ?? []).map((r) => (r.id === id ? { ...r, status: "Cancelled" } : r)),
      );
      return { prev };
    },
    onError: (_e, _id, ctx) => {
      if (ctx?.prev) qc.setQueryData(reservationsKey, ctx.prev);
      toast.error("Could not cancel reservation");
    },
    onSuccess: () => toast.success("Reservation cancelled"),
    onSettled: () => {
      void qc.invalidateQueries({ queryKey: reservationsKey });
      void qc.invalidateQueries({ queryKey: ["analytics"] });
    },
  });
}

export function useGenerateInvoice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (reservationId: string) => {
      const { data } = await http.post(`/api/v1/reservations/${reservationId}/invoice`);
      return data;
    },
    onSuccess: () => {
      toast.success("Invoice generated");
      void qc.invalidateQueries({ queryKey: ["invoices"] });
    },
    onError: () => toast.error("Could not generate invoice"),
  });
}
