import { apiFetch } from "@/server/api";

export async function POST() {
  const response = await apiFetch("/api/v1/notifications/read-all", { method: "POST" });
  return new Response(null, { status: response.status });
}
