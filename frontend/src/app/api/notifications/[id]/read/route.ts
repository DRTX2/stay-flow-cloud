import { apiFetch } from "@/server/api";

export async function POST(
  _request: Request,
  context: { params: Promise<{ id: string }> },
) {
  const { id } = await context.params;
  const response = await apiFetch(
    `/api/v1/notifications/${encodeURIComponent(id)}/read`,
    {
      method: "POST",
    },
  );
  return new Response(null, { status: response.status });
}
