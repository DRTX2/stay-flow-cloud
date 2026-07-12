import {
  BedDouble,
  CalendarCheck,
  CheckCircle2,
  ClipboardList,
  DollarSign,
  Gauge,
  LogIn,
  LogOut,
  Inbox,
  ShoppingBasket,
  ArrowRight,
  TrendingUp,
  Wrench,
} from "lucide-react";
import Link from "next/link";
import { StatCard } from "@/components/shared/StatCard";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
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
import { getJson } from "@/server/api";
import { runSampleStayAction } from "@/app/dashboard/actions";
import type {
  DashboardSummary,
  FrontDeskReservationItem,
  FrontDeskRoomIssue,
  FrontDeskToday,
  RevenuePoint,
  RevenueReport,
  SetupChecklist,
} from "@/types/api";
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
  let frontDesk: FrontDeskToday | null = null;
  let setup: SetupChecklist | null = null;
  let points: RevenuePoint[] = [];
  let failed = false;

  try {
    const revenueReportPromise = getJson<RevenueReport>("/api/v1/analytics/revenue");
    [summary, frontDesk, setup] = await Promise.all([
      getJson<DashboardSummary>("/api/v1/analytics/dashboard"),
      getJson<FrontDeskToday>("/api/v1/analytics/front-desk/today"),
      getJson<SetupChecklist>("/api/v1/analytics/setup-checklist"),
    ]);
    points = (await revenueReportPromise).daily ?? [];
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

      <SetupChecklistCard setup={setup} />

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        <StatCard
          label="Revenue (30d)"
          value={money(summary?.bookedRevenueLast30Days ?? summary?.revenue)}
          icon={DollarSign}
        />
        <StatCard
          label="Occupancy"
          value={percent(summary?.occupancyRate)}
          icon={Gauge}
        />
        <StatCard
          label="Arrivals"
          value={number(summary?.arrivalsToday ?? frontDesk?.arrivals)}
          icon={LogIn}
        />
        <StatCard
          label="Departures"
          value={number(summary?.departuresToday ?? frontDesk?.departures)}
          icon={LogOut}
        />
        <StatCard
          label="In house"
          value={number(summary?.inHouse ?? frontDesk?.inHouse)}
          icon={BedDouble}
        />
        <StatCard
          label="Ops backlog"
          value={number(
            (frontDesk?.pendingHousekeepingTasks ?? 0) +
              (frontDesk?.openMaintenanceWorkOrders ?? 0) +
              (frontDesk?.pendingBookingEnquiries ?? 0) +
              (frontDesk?.openOrders ?? 0),
          )}
          icon={ClipboardList}
        />
      </div>

      <DailyOperationsCard frontDesk={frontDesk} />
      <FrontDeskBoard frontDesk={frontDesk} />

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

function SetupChecklistCard({ setup }: { setup: SetupChecklist | null }) {
  if (!setup) return null;

  return (
    <Card>
      <CardHeader className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
        <div>
          <CardTitle>Guided property setup</CardTitle>
          <CardDescription>
            {setup.completedSteps ?? 0} of {setup.totalSteps ?? 0} setup steps complete.
          </CardDescription>
        </div>
        <form action={runSampleStayAction}>
          <Button type="submit" variant="outline">
            Run sample stay
          </Button>
        </form>
      </CardHeader>
      <CardContent>
        <div className="mb-4 h-2 overflow-hidden rounded-full bg-muted">
          <div
            className="h-full rounded-full bg-primary"
            style={{ width: `${setup.percentComplete ?? 0}%` }}
          />
        </div>
        <div className="grid gap-2 md:grid-cols-5">
          {(setup.steps ?? []).map((step) => (
            <Link
              key={step.key}
              href={step.nextHref ?? "#"}
              className="rounded-lg border p-3 transition hover:bg-muted/50"
            >
              <div className="flex items-center gap-2 text-sm font-medium">
                <CheckCircle2
                  className={
                    step.completed
                      ? "h-4 w-4 text-emerald-600"
                      : "h-4 w-4 text-muted-foreground"
                  }
                />
                {step.label ?? step.key}
              </div>
              <p className="mt-1 text-xs text-muted-foreground">
                {step.count ?? 0} configured
              </p>
            </Link>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

function FrontDeskBoard({ frontDesk }: { frontDesk: FrontDeskToday | null }) {
  return (
    <div className="grid gap-4 xl:grid-cols-3">
      <ReservationListCard
        title="Arrivals today"
        description="Guests expected to check in."
        empty="No arrivals scheduled."
        items={frontDesk?.arrivalList ?? []}
      />
      <ReservationListCard
        title="Departures today"
        description="Rooms expected to turn over."
        empty="No departures scheduled."
        items={frontDesk?.departureList ?? []}
      />
      <RoomIssuesCard issues={frontDesk?.roomIssues ?? []} />
    </div>
  );
}

function DailyOperationsCard({ frontDesk }: { frontDesk: FrontDeskToday | null }) {
  const queues = [
    {
      label: "Arrivals today",
      value: frontDesk?.arrivals ?? 0,
      href: "/dashboard/reservations",
      icon: LogIn,
    },
    {
      label: "Departures today",
      value: frontDesk?.departures ?? 0,
      href: "/dashboard/reservations",
      icon: LogOut,
    },
    {
      label: "Booking enquiries",
      value: frontDesk?.pendingBookingEnquiries ?? 0,
      href: "/dashboard/booking-enquiries",
      icon: Inbox,
    },
    {
      label: "Open room orders",
      value: frontDesk?.openOrders ?? 0,
      href: "/dashboard/orders",
      icon: ShoppingBasket,
    },
    {
      label: "Housekeeping tasks",
      value: frontDesk?.pendingHousekeepingTasks ?? 0,
      href: "/dashboard/housekeeping",
      icon: ClipboardList,
    },
    {
      label: "Maintenance work orders",
      value: frontDesk?.openMaintenanceWorkOrders ?? 0,
      href: "/dashboard/maintenance",
      icon: Wrench,
    },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle>Today&apos;s operations</CardTitle>
        <CardDescription>
          Prioritized queues requiring attention across the property.
        </CardDescription>
      </CardHeader>
      <CardContent className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
        {queues.map((queue) => {
          const Icon = queue.icon;
          return (
            <Link
              key={queue.label}
              href={queue.href}
              className="group flex min-h-20 items-center gap-3 rounded-lg border p-4 transition-colors hover:bg-muted/50"
            >
              <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                <Icon className="h-5 w-5" />
              </span>
              <span className="min-w-0 flex-1">
                <span className="block text-2xl font-semibold tabular-nums">
                  {number(queue.value)}
                </span>
                <span className="block text-sm text-muted-foreground">{queue.label}</span>
              </span>
              <ArrowRight className="h-4 w-4 text-muted-foreground transition-transform group-hover:translate-x-0.5" />
            </Link>
          );
        })}
      </CardContent>
    </Card>
  );
}

function ReservationListCard({
  title,
  description,
  empty,
  items,
}: {
  title: string;
  description: string;
  empty: string;
  items: FrontDeskReservationItem[];
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        {items.length === 0 ? (
          <p className="text-sm text-muted-foreground">{empty}</p>
        ) : (
          items.slice(0, 5).map((item) => (
            <div key={item.reservationId} className="rounded-lg border p-3">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="font-medium">{item.guestName ?? "Guest"}</p>
                  <p className="text-xs text-muted-foreground">
                    Room {item.roomNumber ?? "TBD"} · {item.guests ?? 1} guest(s)
                  </p>
                </div>
                <Badge variant="outline">{item.status ?? "Pending"}</Badge>
              </div>
              <p className="mt-2 text-xs text-muted-foreground">
                {item.confirmationCode ?? item.reservationId}
              </p>
            </div>
          ))
        )}
      </CardContent>
    </Card>
  );
}

function RoomIssuesCard({ issues }: { issues: FrontDeskRoomIssue[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Rooms needing attention</CardTitle>
        <CardDescription>Dirty, maintenance or out-of-service rooms.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        {issues.length === 0 ? (
          <p className="text-sm text-muted-foreground">No room issues right now.</p>
        ) : (
          issues.slice(0, 6).map((issue) => (
            <div
              key={issue.roomId}
              className="flex items-center justify-between rounded-lg border p-3"
            >
              <div>
                <p className="font-medium">Room {issue.roomNumber ?? "TBD"}</p>
                <p className="text-xs text-muted-foreground">
                  {issue.roomStatus} · {issue.cleaningStatus}
                </p>
              </div>
              <div className="flex items-center gap-2 text-xs text-muted-foreground">
                <span className="flex items-center gap-1">
                  <ClipboardList className="h-3.5 w-3.5" />
                  {issue.openHousekeepingTasks ?? 0}
                </span>
                <span className="flex items-center gap-1">
                  <Wrench className="h-3.5 w-3.5" />
                  {issue.openMaintenanceWorkOrders ?? 0}
                </span>
              </div>
            </div>
          ))
        )}
      </CardContent>
    </Card>
  );
}
