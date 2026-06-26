"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";
import type { CreateServiceRequest } from "@/types/api";

export type { ActionResult };

export async function createServiceAction(
  input: CreateServiceRequest,
): Promise<ActionResult> {
  const res = await apiFetch("/api/v1/services", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) return fail(res, "Could not create service");
  revalidatePath("/dashboard/services");
  return ok;
}

export async function updateServiceAction(
  id: string,
  input: CreateServiceRequest,
): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/services/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) return fail(res, "Could not update service");
  revalidatePath("/dashboard/services");
  return ok;
}
