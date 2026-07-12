"use server";

import { apiFetch } from "@/server/api";
import { fail, ok, type ActionResult } from "@/server/actions";

export async function submitFeedbackAction(input: {
  token: string;
  rating: number;
  comment?: string;
}): Promise<ActionResult> {
  const res = await apiFetch("/api/v1/public/feedback", {
    method: "POST",
    auth: false,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(input),
  });
  if (!res.ok) return fail(res, "Could not submit feedback");
  return ok;
}
