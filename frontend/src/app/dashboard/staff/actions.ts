"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";

export async function createStaffUserAction(formData: FormData): Promise<void> {
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

  if (!res.ok) throw new Error(`Could not create staff user (${res.status}).`);
  revalidatePath("/dashboard/staff");
}

export async function updateStaffRolesAction(formData: FormData): Promise<void> {
  const id = String(formData.get("id") ?? "");
  const roles = formData.getAll("roles").map(String).filter(Boolean);

  const res = await apiFetch(`/api/v1/staff/${id}/roles`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ roles }),
  });

  if (!res.ok) throw new Error(`Could not update staff roles (${res.status}).`);
  revalidatePath("/dashboard/staff");
}
