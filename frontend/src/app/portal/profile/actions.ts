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
  const res = await apiFetch("/api/v1/portal/profile", {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      firstName: input.firstName,
      lastName: input.lastName,
      phone: input.phone,
      documentNumber: input.documentNumber,
    }),
  });
  if (!res.ok) return fail(res, "Could not update profile");

  revalidatePath("/portal/profile");
  revalidatePath("/portal");
  return ok;
}
