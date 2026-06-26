"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";
import type { CreateRoomTypeRequest } from "@/types/api";

export type { ActionResult };

export async function createRoomTypeAction(
  input: CreateRoomTypeRequest,
): Promise<ActionResult> {
  const res = await apiFetch("/api/v1/roomtypes", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) return fail(res, "Could not create room type");
  revalidatePath("/dashboard/room-types");
  return ok;
}
