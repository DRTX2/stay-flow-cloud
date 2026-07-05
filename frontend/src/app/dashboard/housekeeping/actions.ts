"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";

export async function createHousekeepingTaskAction(formData: FormData) {
  const roomId = String(formData.get("roomId") ?? "");
  const taskType = String(formData.get("taskType") ?? "Daily Clean");
  const notes = String(formData.get("notes") ?? "");

  if (!roomId) return;

  await apiFetch("/api/v1/housekeeping/tasks", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ roomId, taskType, notes: notes || null }),
  });
  revalidatePath("/dashboard/housekeeping");
  revalidatePath("/dashboard");
}

export async function completeHousekeepingTaskAction(formData: FormData) {
  const id = String(formData.get("id") ?? "");
  const cleaningStatus = String(formData.get("cleaningStatus") ?? "Inspected");

  if (!id) return;

  await apiFetch(`/api/v1/housekeeping/tasks/${id}/complete`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ cleaningStatus }),
  });
  revalidatePath("/dashboard/housekeeping");
  revalidatePath("/dashboard/rooms");
  revalidatePath("/dashboard");
}
