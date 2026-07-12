"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";

function refreshOrders() {
  revalidatePath("/dashboard/orders");
  revalidatePath("/dashboard");
}

export async function createOrderAction(input: {
  reservationId: string;
  serviceItemId: string;
  quantity: number;
  notes?: string;
}): Promise<ActionResult> {
  const res = await apiFetch("/api/v1/orders", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      reservationId: input.reservationId,
      notes: input.notes || null,
      items: [{ serviceItemId: input.serviceItemId, quantity: input.quantity }],
    }),
  });
  if (!res.ok) return fail(res, "Could not place order");
  refreshOrders();
  return ok;
}

export async function transitionOrderAction(
  id: string,
  transition: "prepare" | "deliver" | "cancel",
): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/orders/${id}/${transition}`, { method: "POST" });
  if (!res.ok) return fail(res, `Could not ${transition} order`);
  refreshOrders();
  return ok;
}
