import { apiFetch } from "@/server/api";

export async function GET() {
  const response = await apiFetch("/api/v1/notifications");
  return new Response(response.body, {
    status: response.status,
    headers: {
      "Content-Type": response.headers.get("Content-Type") ?? "application/json",
    },
  });
}
