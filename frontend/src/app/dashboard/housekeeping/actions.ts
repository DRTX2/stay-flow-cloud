"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import type { ActionState } from "@/components/shared/ActionForm";

export async function createHousekeepingTaskAction(
  _state: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const roomId = String(formData.get("roomId") ?? "");
  const taskType = String(formData.get("taskType") ?? "Daily Clean");
  const notes = String(formData.get("notes") ?? "");

  if (!roomId) return { error: "Select a room." };

  const response = await apiFetch("/api/v1/housekeeping/tasks", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ roomId, taskType, notes: notes || null }),
  });
  if (!response.ok) return { error: `Could not create the task (${response.status}).` };
  revalidatePath("/dashboard/housekeeping");
  revalidatePath("/dashboard");
  return { success: "Housekeeping task created." };
}

export async function completeHousekeepingTaskAction(
  _state: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const id = String(formData.get("id") ?? "");
  const cleaningStatus = String(formData.get("cleaningStatus") ?? "Inspected");

  if (!id) return { error: "The housekeeping task is missing." };

  const response = await apiFetch(`/api/v1/housekeeping/tasks/${id}/complete`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ cleaningStatus }),
  });
  if (!response.ok) return { error: `Could not complete the task (${response.status}).` };
  revalidatePath("/dashboard/housekeeping");
  revalidatePath("/dashboard/rooms");
  revalidatePath("/dashboard");
  return { success: "Housekeeping task completed." };
}
