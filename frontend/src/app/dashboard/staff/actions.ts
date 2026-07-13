"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import type { ActionState } from "@/components/shared/ActionForm";

export async function createStaffUserAction(
  _state: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const roles = formData.getAll("roles").map(String).filter(Boolean);
  const body = {
    fullName: String(formData.get("fullName") ?? ""),
    email: String(formData.get("email") ?? ""),
    password: String(formData.get("password") ?? ""),
    roles,
  };

  const res = await apiFetch("/api/v1/staff", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!res.ok) return { error: `Could not create staff user (${res.status}).` };
  revalidatePath("/dashboard/staff");
  return { success: "Staff user created." };
}

export async function updateStaffRolesAction(
  _state: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const id = String(formData.get("id") ?? "");
  const roles = formData.getAll("roles").map(String).filter(Boolean);

  const res = await apiFetch(`/api/v1/staff/${id}/roles`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ roles }),
  });

  if (!res.ok) return { error: `Could not update staff roles (${res.status}).` };
  revalidatePath("/dashboard/staff");
  return { success: "Staff role updated." };
}
