"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";
import type { CreateGuestRequest } from "@/types/api";

export type { ActionResult };

/** Update the explicitly linked guest profile. */
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
