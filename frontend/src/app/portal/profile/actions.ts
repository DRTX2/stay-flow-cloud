"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";
import type { CreateGuestRequest } from "@/types/api";

export type { ActionResult };

/**
 * Update the guest profile. Uses PUT if we could resolve a guest id, POST otherwise.
 * In the current architecture the guest id is not embedded in the token, so we attempt
 * to fetch the guest list (the Customer role has `guests:read` on some setups) and match
 * by email. If that fails we fall back to creating a new guest record.
 *
 * A future iteration can embed the guest id as a token claim for a cleaner flow.
 */
export async function updateProfileAction(
  input: CreateGuestRequest,
): Promise<ActionResult> {
  // Try to find an existing guest by email so we can PUT instead of POST.
  let guestId: string | undefined;
  try {
    const listRes = await apiFetch(
      `/api/v1/guests?pageSize=1&search=${encodeURIComponent(input.email)}`,
    );
    if (listRes.ok) {
      const body = (await listRes.json()) as { items?: { id: string; email?: string }[] };
      const match = body.items?.find(
        (g) => g.email?.toLowerCase() === input.email.toLowerCase(),
      );
      if (match) guestId = match.id;
    }
  } catch {
    // Lookup failed — fall through to POST.
  }

  if (guestId) {
    const res = await apiFetch(`/api/v1/guests/${guestId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(input),
    });
    if (!res.ok) return fail(res, "Could not update profile");
  } else {
    const res = await apiFetch("/api/v1/guests", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(input),
    });
    if (!res.ok) return fail(res, "Could not save profile");
  }

  revalidatePath("/portal/profile");
  revalidatePath("/portal");
  return ok;
}
