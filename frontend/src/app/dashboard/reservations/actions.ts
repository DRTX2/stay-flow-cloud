"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import type { CreateReservationRequest } from "@/types/api";

export interface ActionResult {
  ok: boolean;
  error?: string;
}

export async function createReservationAction(
  input: CreateReservationRequest,
): Promise<ActionResult> {
  const res = await apiFetch("/api/v1/reservations", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok)
    return { ok: false, error: `Could not create reservation (${res.status}).` };
  revalidatePath("/dashboard/reservations");
  revalidatePath("/dashboard");
  return { ok: true };
}

export async function cancelReservationAction(id: string): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/reservations/${id}/cancel`, { method: "POST" });
  if (!res.ok)
    return { ok: false, error: `Could not cancel reservation (${res.status}).` };
  revalidatePath("/dashboard/reservations");
  revalidatePath("/dashboard");
  return { ok: true };
}

export async function generateInvoiceAction(
  reservationId: string,
): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/reservations/${reservationId}/invoice`, {
    method: "POST",
  });
  if (!res.ok) return { ok: false, error: `Could not generate invoice (${res.status}).` };
  revalidatePath("/dashboard/invoices");
  revalidatePath("/dashboard/reservations");
  return { ok: true };
}
