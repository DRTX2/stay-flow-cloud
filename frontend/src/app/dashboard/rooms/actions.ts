"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";
import type { CreateRoomRequest } from "@/types/api";

export type { ActionResult };

export async function createRoomAction(input: CreateRoomRequest): Promise<ActionResult> {
  const res = await apiFetch("/api/v1/rooms", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) return fail(res, "Could not create room");
  revalidatePath("/dashboard/rooms");
  return ok;
}

export async function updateRoomPriceAction(
  id: string,
  newPrice: number,
): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/rooms/${id}/price`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ newPrice }),
  });
  if (!res.ok) return fail(res, "Could not update price");
  revalidatePath("/dashboard/rooms");
  return ok;
}
