"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";

export type { ActionResult };

export async function payInvoiceAction(id: string): Promise<ActionResult> {
  const res = await apiFetch(`/api/v1/invoices/${id}/pay`, { method: "POST" });
  if (!res.ok) return fail(res, "Could not mark invoice paid");
  revalidatePath("/dashboard/invoices");
  revalidatePath("/dashboard");
  return ok;
}
