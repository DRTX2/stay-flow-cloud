"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";

export async function rejectBookingEnquiryAction(
  id: string,
  reason?: string,
): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/bookingenquiries/${id}/reject`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ reason: reason || null }),
  });
  if (!res.ok) return fail(res, "Could not reject booking enquiry");
  revalidatePath("/dashboard/booking-enquiries");
  return ok;
}

export async function convertBookingEnquiryAction(
  id: string,
  roomId: string,
): Promise<ActionResult> {
  if (!roomId)
    return { ok: false, error: "Select a room before converting the enquiry." };
  const res = await apiFetch(`/api/v1/bookingenquiries/${id}/convert`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ roomId }),
  });
  if (!res.ok) return fail(res, "Could not convert booking enquiry");
  revalidatePath("/dashboard/booking-enquiries");
  revalidatePath("/dashboard/reservations");
  revalidatePath("/dashboard");
  return ok;
}
