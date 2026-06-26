"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";
import type { CreateGuestRequest } from "@/types/api";

export type { ActionResult };

export async function createGuestAction(
  input: CreateGuestRequest,
): Promise<ActionResult> {
  const res = await apiFetch("/api/v1/guests", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) return fail(res, "Could not create guest");
  revalidatePath("/dashboard/guests");
  return ok;
}

export async function updateGuestAction(
  id: string,
  input: CreateGuestRequest,
): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/guests/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) return fail(res, "Could not update guest");
  revalidatePath("/dashboard/guests");
  return ok;
}
