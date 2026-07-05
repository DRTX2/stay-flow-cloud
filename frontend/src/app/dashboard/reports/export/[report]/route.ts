import { apiFetch } from "@/server/api";

const reports = new Set(["occupancy", "revenue", "arrivals-departures", "night-audit"]);

export async function GET(
  request: Request,
  { params }: { params: Promise<{ report: string }> },
) {
  const { report } = await params;
  if (!reports.has(report)) {
    return new Response("Unknown report", { status: 404 });
  }

  const url = new URL(request.url);
  const query = url.searchParams.toString();
  const response = await apiFetch(
    `/api/v1/reports/${report}.csv${query ? `?${query}` : ""}`,
  );

  if (!response.ok) {
    return new Response("Could not export report", { status: response.status });
  }

  return new Response(response.body, {
    headers: {
      "Content-Type": response.headers.get("content-type") ?? "text/csv; charset=utf-8",
      "Content-Disposition":
        response.headers.get("content-disposition") ??
        `attachment; filename="stayflow-${report}.csv"`,
    },
  });
}
