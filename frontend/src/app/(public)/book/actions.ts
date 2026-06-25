"use server";

import { apiFetch } from "@/server/api";
import type { BookingRequest } from "@/types/api";

export interface BookingResult {
  ok: boolean;
  reference?: string;
  error?: string;
}

/** Forwards a public booking enquiry to the anonymous backend endpoint and returns the reference. */
export async function createBookingAction(input: BookingRequest): Promise<BookingResult> {
  const res = await apiFetch("/api/v1/public/bookings", {
    method: "POST",
    auth: false,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });

  if (!res.ok) {
    return { ok: false, error: `Could not submit your booking (${res.status}).` };
  }

  const data = (await res.json().catch(() => null)) as { reference?: string } | null;
  return { ok: true, reference: data?.reference };
}
