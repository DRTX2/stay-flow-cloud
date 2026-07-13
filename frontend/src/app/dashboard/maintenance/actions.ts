"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import type { ActionState } from "@/components/shared/ActionForm";

export async function createWorkOrderAction(
  _state: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const roomId = String(formData.get("roomId") ?? "");
  const description = String(formData.get("description") ?? "");
  const priority = String(formData.get("priority") ?? "Medium");

  if (!description) return { error: "Describe the maintenance issue." };

  const response = await apiFetch("/api/v1/maintenance/work-orders", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ roomId: roomId || null, description, priority }),
  });
  if (!response.ok)
    return { error: `Could not create the work order (${response.status}).` };
  revalidatePath("/dashboard/maintenance");
  revalidatePath("/dashboard/rooms");
  revalidatePath("/dashboard");
  return { success: "Work order created." };
}

export async function resolveWorkOrderAction(
  _state: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const id = String(formData.get("id") ?? "");
  const notes = String(formData.get("notes") ?? "Resolved from StayFlow.");

  if (!id) return { error: "The work order is missing." };

  const response = await apiFetch(`/api/v1/maintenance/work-orders/${id}/resolve`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ notes }),
  });
  if (!response.ok)
    return { error: `Could not resolve the work order (${response.status}).` };
  revalidatePath("/dashboard/maintenance");
  revalidatePath("/dashboard/rooms");
  revalidatePath("/dashboard");
  return { success: "Work order resolved." };
}
