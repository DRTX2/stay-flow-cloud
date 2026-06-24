import {
  BedDouble,
  CalendarCheck,
  DollarSign,
  Gauge,
  TrendingUp,
  Users,
} from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
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
import { useDashboard, useRevenue } from "./api";
import {
  CumulativeRevenueLine,
  OccupancyPie,
  RevenueAreaChart,
  RevenueBarChart,
} from "./charts";

export function DashboardPage() {
  const { data, isLoading, isError } = useDashboard();
  const revenue = useRevenue();
  const points = revenue.data ?? [];

  return (
    <div className="space-y-6">
      <PageHeader
        title="Dashboard"
        description="Executive overview of revenue, occupancy and operations."
      />

      {isError && (
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
          value={money(data?.revenue)}
          icon={DollarSign}
          loading={isLoading}
        />
        <StatCard
          label="Occupancy"
          value={percent(data?.occupancyRate)}
          icon={Gauge}
          loading={isLoading}
        />
        <StatCard
          label="ADR"
          value={money(data?.adr)}
          icon={TrendingUp}
          loading={isLoading}
        />
        <StatCard
          label="RevPAR"
          value={money(data?.revPar)}
          icon={TrendingUp}
          loading={isLoading}
        />
        <StatCard
          label="Reservations"
          value={number(data?.totalReservations)}
          icon={CalendarCheck}
          loading={isLoading}
        />
        <StatCard
          label="Guests"
          value={number(data?.totalGuests)}
          icon={Users}
          loading={isLoading}
        />
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
              {revenue.isLoading ? (
                <Skeleton className="mt-4 h-[260px] w-full" />
              ) : points.length === 0 ? (
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
            {isLoading ? (
              <Skeleton className="h-[260px] w-full" />
            ) : (
              <div className="flex w-full flex-col items-center gap-3">
                <OccupancyPie occupancyRate={data?.occupancyRate} />
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
            )}
          </CardContent>
        </Card>
      </div>

      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <BedDouble className="h-3.5 w-3.5" />
        {number(data?.availableRooms)} rooms available now.
      </div>
    </div>
  );
}
