import type { Metadata } from "next";
import { Suspense } from "react";
import { PageHeader } from "@/components/shared/PageHeader";
import {
  DashboardOverview,
  DashboardOverviewSkeleton,
} from "@/features/dashboard/DashboardOverview";

export const metadata: Metadata = { title: "Dashboard" };

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Dashboard"
        description="Executive overview of revenue, occupancy and operations."
      />
      {/* Stream the analytics: the shell + header render instantly, KPIs/charts fill in. */}
      <Suspense fallback={<DashboardOverviewSkeleton />}>
        <DashboardOverview />
      </Suspense>
    </div>
  );
}
