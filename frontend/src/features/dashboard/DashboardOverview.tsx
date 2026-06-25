import {
  BedDouble,
  CalendarCheck,
  DollarSign,
  Gauge,
  TrendingUp,
  Users,
} from "lucide-react";
import { StatCard } from "@/components/shared/StatCard";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { money, number, percent } from "@/lib/format";
import { getJson, getList } from "@/server/api";
import type { DashboardSummary, RevenuePoint } from "@/types/api";
import {
  CumulativeRevenueLine,
  OccupancyPie,
  RevenueAreaChart,
  RevenueBarChart,
} from "./charts";

/** Skeleton shown while the analytics data streams in (Suspense fallback). */
export function DashboardOverviewSkeleton() {
  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        {Array.from({ length: 6 }).map((_, i) => (
          <Skeleton key={i} className="h-28 w-full rounded-xl" />
        ))}
      </div>
      <div className="grid gap-4 lg:grid-cols-3">
        <Skeleton className="h-[360px] w-full rounded-xl lg:col-span-2" />
        <Skeleton className="h-[360px] w-full rounded-xl" />
      </div>
    </div>
  );
}

/**
 * Async server component that fetches analytics and renders the KPI cards + charts. Rendered inside a
 * Suspense boundary so the page shell streams immediately and this section fills in when ready.
 */
export async function DashboardOverview() {
  let summary: DashboardSummary | null = null;
  let points: RevenuePoint[] = [];
  let failed = false;

  try {
    [summary, points] = await Promise.all([
      getJson<DashboardSummary>("/api/v1/analytics/dashboard"),
      getList<RevenuePoint>("/api/v1/analytics/revenue"),
    ]);
  } catch {
    failed = true;
  }

  return (
    <div className="space-y-6">
      {failed && (
        <Card>
          <CardContent className="p-6 text-sm text-destructive">
            Could not load analytics. This view requires the{" "}
            <code className="rounded bg-muted px-1">analytics:view</code> permission.
          </CardContent>
        </Card>
      )}

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        <StatCard
          label="Revenue (30d)"
          value={money(summary?.revenue)}
          icon={DollarSign}
        />
        <StatCard
          label="Occupancy"
          value={percent(summary?.occupancyRate)}
          icon={Gauge}
        />
        <StatCard label="ADR" value={money(summary?.adr)} icon={TrendingUp} />
        <StatCard label="RevPAR" value={money(summary?.revPar)} icon={TrendingUp} />
        <StatCard
          label="Reservations"
          value={number(summary?.totalReservations)}
          icon={CalendarCheck}
        />
        <StatCard label="Guests" value={number(summary?.totalGuests)} icon={Users} />
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Revenue</CardTitle>
            <CardDescription>Trend across the reporting window.</CardDescription>
          </CardHeader>
          <CardContent>
            <Tabs defaultValue="area">
              <TabsList>
                <TabsTrigger value="area">Trend</TabsTrigger>
                <TabsTrigger value="bar">By period</TabsTrigger>
                <TabsTrigger value="cumulative">Cumulative</TabsTrigger>
              </TabsList>
              {points.length === 0 ? (
                <div className="flex h-[260px] items-center justify-center text-sm text-muted-foreground">
                  No revenue data for this window.
                </div>
              ) : (
                <>
                  <TabsContent value="area">
                    <RevenueAreaChart points={points} />
                  </TabsContent>
                  <TabsContent value="bar">
                    <RevenueBarChart points={points} />
                  </TabsContent>
                  <TabsContent value="cumulative">
                    <CumulativeRevenueLine points={points} />
                  </TabsContent>
                </>
              )}
            </Tabs>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Occupancy</CardTitle>
            <CardDescription>Occupied vs available rooms.</CardDescription>
          </CardHeader>
          <CardContent className="flex items-center justify-center">
            <div className="flex w-full flex-col items-center gap-3">
              <OccupancyPie occupancyRate={summary?.occupancyRate} />
              <div className="flex items-center gap-4 text-sm">
                <span className="flex items-center gap-1.5">
                  <span className="h-2.5 w-2.5 rounded-full bg-primary" />
                  Occupied
                </span>
                <span className="flex items-center gap-1.5">
                  <span className="h-2.5 w-2.5 rounded-full bg-muted" />
                  Available
                </span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <BedDouble className="h-3.5 w-3.5" />
        {number(summary?.availableRooms)} rooms available now.
      </div>
    </div>
  );
}
