"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";

export async function createWorkOrderAction(formData: FormData) {
  const roomId = String(formData.get("roomId") ?? "");
  const description = String(formData.get("description") ?? "");
  const priority = String(formData.get("priority") ?? "Medium");

  if (!description) return;

  await apiFetch("/api/v1/maintenance/work-orders", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ roomId: roomId || null, description, priority }),
  });
  revalidatePath("/dashboard/maintenance");
  revalidatePath("/dashboard/rooms");
  revalidatePath("/dashboard");
}

export async function resolveWorkOrderAction(formData: FormData) {
  const id = String(formData.get("id") ?? "");
  const notes = String(formData.get("notes") ?? "Resolved from StayFlow.");

  if (!id) return;

  await apiFetch(`/api/v1/maintenance/work-orders/${id}/resolve`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ notes }),
  });
  revalidatePath("/dashboard/maintenance");
  revalidatePath("/dashboard/rooms");
  revalidatePath("/dashboard");
}
