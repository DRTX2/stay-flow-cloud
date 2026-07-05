"use server";

import { revalidatePath } from "next/cache";
import { apiFetch } from "@/server/api";

export async function runSampleStayAction() {
  await apiFetch("/api/v1/demo/sample-stay", { method: "POST" });
  revalidatePath("/dashboard");
  revalidatePath("/dashboard/guests");
  revalidatePath("/dashboard/reservations");
  revalidatePath("/dashboard/invoices");
  revalidatePath("/dashboard/housekeeping");
  revalidatePath("/dashboard/rooms");
}
