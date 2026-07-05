import type { Metadata } from "next";
import { Suspense } from "react";
import { PageHeader } from "@/components/shared/PageHeader";
import { Skeleton } from "@/components/ui/skeleton";
import { getJson } from "@/server/api";
import type { DashboardSummary, RevenuePoint, RevenueReport } from "@/types/api";
import { ReportsView } from "@/features/reports/ReportsView";

export const metadata: Metadata = { title: "Reports" };

async function ReportsData() {
  let summary: DashboardSummary | null = null;
  let points: RevenuePoint[] = [];
  let failed = false;

  try {
    const revenueReport: RevenueReport | null = await getJson<RevenueReport>(
      "/api/v1/analytics/revenue",
    );
    [summary] = await Promise.all([
      getJson<DashboardSummary>("/api/v1/analytics/dashboard"),
    ]);
    points = revenueReport?.daily ?? [];
  } catch {
    failed = true;
  }

  return <ReportsView summary={summary} points={points} failed={failed} />;
}

function ReportsSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-16 w-full rounded-xl" />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        {Array.from({ length: 6 }).map((_, i) => (
          <Skeleton key={i} className="h-28 w-full rounded-xl" />
        ))}
      </div>
      <Skeleton className="h-[380px] w-full rounded-xl" />
      <div className="grid gap-4 lg:grid-cols-2">
        <Skeleton className="h-[360px] w-full rounded-xl" />
        <Skeleton className="h-[360px] w-full rounded-xl" />
      </div>
    </div>
  );
}

export default function ReportsPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Reports"
        description="Detailed analytics and operational reports."
      />
      <Suspense fallback={<ReportsSkeleton />}>
        <ReportsData />
      </Suspense>
    </div>
  );
}
