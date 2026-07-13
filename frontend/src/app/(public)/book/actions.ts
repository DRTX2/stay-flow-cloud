"use server";

import { apiFetch } from "@/server/api";
import type { BookingRequest, PublicAvailability } from "@/types/api";

export interface BookingResult {
  ok: boolean;
  reference?: string;
  error?: string;
}

export async function checkAvailabilityAction(
  input: Pick<
    BookingRequest,
    "hotelSlug" | "roomTypeId" | "checkIn" | "checkOut" | "guests"
  >,
): Promise<{ ok: boolean; availability?: PublicAvailability; error?: string }> {
  const query = new URLSearchParams({
    roomTypeId: input.roomTypeId,
    checkIn: input.checkIn,
    checkOut: input.checkOut,
    guests: String(input.guests),
  });
  const res = await apiFetch(
    `/api/v1/public/hotels/${encodeURIComponent(input.hotelSlug)}/availability?${query}`,
    { auth: false },
  );
  if (!res.ok)
    return { ok: false, error: `Availability could not be checked (${res.status}).` };
  return { ok: true, availability: (await res.json()) as PublicAvailability };
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
