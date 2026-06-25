"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";

export interface ActionResult {
  ok: boolean;
  error?: string;
}

export async function setTenantFeatureAction(
  key: string,
  enabled: boolean,
): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/tenantfeatures/${key}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ enabled }),
  });
  if (!res.ok) return { ok: false, error: `Could not update feature (${res.status}).` };
  revalidatePath("/dashboard/tenant-features");
  return { ok: true };
}
