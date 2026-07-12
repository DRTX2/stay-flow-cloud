"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";
import type { CreateReservationRequest } from "@/types/api";

export type { ActionResult };

export interface FeedbackInvitationResult extends ActionResult {
  token?: string;
  expiresAtUtc?: string;
}

function revalidateReservations() {
  revalidatePath("/dashboard/reservations");
  revalidatePath("/dashboard");
}

export async function createReservationAction(
  input: CreateReservationRequest,
): Promise<ActionResult> {
  const res = await apiFetch("/api/v1/reservations", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) return fail(res, "Could not create reservation");
  revalidateReservations();
  return ok;
}

/** POST a reservation lifecycle transition (confirm/cancel/check-in/check-out). */
async function transition(
  id: string,
  action: string,
  fallback: string,
): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/reservations/${id}/${action}`, { method: "POST" });
  if (!res.ok) return fail(res, fallback);
  revalidateReservations();
  return ok;
}

export async function confirmReservationAction(id: string): Promise<ActionResult> {
  return transition(id, "confirm", "Could not confirm reservation");
}

export async function checkInReservationAction(id: string): Promise<ActionResult> {
  return transition(id, "check-in", "Could not check in reservation");
}

export async function checkOutReservationAction(id: string): Promise<ActionResult> {
  return transition(id, "check-out", "Could not check out reservation");
}

export async function cancelReservationAction(id: string): Promise<ActionResult> {
  return transition(id, "cancel", "Could not cancel reservation");
}

export async function generateInvoiceAction(
  reservationId: string,
): Promise<ActionResult> {
  // Invoices are generated through the billing endpoint, keyed by reservation.
  const res = await apiFetch("/api/v1/invoices", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ reservationId }),
  });
  if (!res.ok) return fail(res, "Could not generate invoice");
  revalidatePath("/dashboard/invoices");
  revalidatePath("/dashboard/reservations");
  return ok;
}

export async function createFeedbackInvitationAction(
  id: string,
): Promise<FeedbackInvitationResult> {
  const res = await apiFetch(`/api/v1/reservations/${id}/feedback-invitation`, {
    method: "POST",
  });
  if (!res.ok) return fail(res, "Could not create feedback invitation");
  const data = (await res.json()) as { token: string; expiresAtUtc: string };
  return { ok: true, ...data };
}
