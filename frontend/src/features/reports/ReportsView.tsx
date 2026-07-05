"use client";

import Link from "next/link";
import { useState } from "react";
import {
  CalendarCheck,
  Download,
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
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { money, number, percent } from "@/lib/format";
import type { DashboardSummary, RevenuePoint } from "@/types/api";
import {
  RevenueAreaChart,
  RevenueBarChart,
  CumulativeRevenueLine,
  OccupancyPie,
} from "@/features/dashboard/charts";

interface ReportsViewProps {
  summary: DashboardSummary | null;
  points: RevenuePoint[];
  failed: boolean;
}

const today = new Date().toISOString().slice(0, 10);

export function ReportsView({ summary, points, failed }: ReportsViewProps) {
  const [days, setDays] = useState(30);
  const [auditDate, setAuditDate] = useState(today);
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");

  const occupancyQuery = new URLSearchParams();
  if (from) occupancyQuery.set("from", from);
  if (to) occupancyQuery.set("to", to);

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>Exportable reports</CardTitle>
          <CardDescription>
            CSV files are generated server-side with the current user permissions.
          </CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 lg:grid-cols-4">
          <div className="space-y-1.5">
            <Label htmlFor="reportDays">Revenue period</Label>
            <Input
              id="reportDays"
              type="number"
              min={7}
              max={365}
              value={days}
              onChange={(e) => setDays(Number(e.target.value) || 30)}
            />
            <Button asChild variant="outline" size="sm" className="w-full gap-2">
              <Link href={`/dashboard/reports/export/revenue?days=${days}`}>
                <Download className="h-4 w-4" /> Revenue CSV
              </Link>
            </Button>
          </div>
          <div className="space-y-1.5">
            <Label>Occupancy range</Label>
            <div className="grid grid-cols-2 gap-2">
              <Input
                type="date"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
                aria-label="From date"
              />
              <Input
                type="date"
                value={to}
                onChange={(e) => setTo(e.target.value)}
                aria-label="To date"
              />
            </div>
            <Button asChild variant="outline" size="sm" className="w-full gap-2">
              <Link
                href={`/dashboard/reports/export/occupancy${occupancyQuery.toString() ? `?${occupancyQuery}` : ""}`}
              >
                <Download className="h-4 w-4" /> Occupancy CSV
              </Link>
            </Button>
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="auditDate">Operations date</Label>
            <Input
              id="auditDate"
              type="date"
              value={auditDate}
              onChange={(e) => setAuditDate(e.target.value)}
            />
            <Button asChild variant="outline" size="sm" className="w-full gap-2">
              <Link
                href={`/dashboard/reports/export/arrivals-departures?date=${auditDate}`}
              >
                <Download className="h-4 w-4" /> Arrivals/departures
              </Link>
            </Button>
          </div>
          <div className="space-y-1.5">
            <Label>Night audit</Label>
            <p className="min-h-9 text-xs text-muted-foreground">
              Snapshot of occupancy, revenue, dirty rooms and open ops work.
            </p>
            <Button asChild variant="outline" size="sm" className="w-full gap-2">
              <Link href={`/dashboard/reports/export/night-audit?date=${auditDate}`}>
                <Download className="h-4 w-4" /> Night audit CSV
              </Link>
            </Button>
          </div>
        </CardContent>
      </Card>

      {failed && (
        <Card>
          <CardContent className="p-6 text-sm text-destructive">
            Could not load analytics. This view requires the{" "}
            <code className="rounded bg-muted px-1">analytics:view</code> permission.
          </CardContent>
        </Card>
      )}

      {/* KPI cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6">
        <StatCard label="Revenue" value={money(summary?.revenue)} icon={DollarSign} />
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

      {/* Revenue charts */}
      <Card>
        <CardHeader>
          <CardTitle>Revenue analysis</CardTitle>
          <CardDescription>
            Trend, period breakdown, and cumulative revenue.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="area">
            <TabsList>
              <TabsTrigger value="area">Trend</TabsTrigger>
              <TabsTrigger value="bar">By period</TabsTrigger>
              <TabsTrigger value="cumulative">Cumulative</TabsTrigger>
            </TabsList>
            {points.length === 0 ? (
              <div className="flex h-[280px] items-center justify-center text-sm text-muted-foreground">
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

      {/* Occupancy */}
      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Occupancy rate</CardTitle>
            <CardDescription>Current occupied vs available rooms.</CardDescription>
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

        <Card>
          <CardHeader>
            <CardTitle>Key metrics summary</CardTitle>
            <CardDescription>At-a-glance operational numbers.</CardDescription>
          </CardHeader>
          <CardContent>
            <dl className="space-y-3 text-sm">
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Arrivals today</dt>
                <dd className="font-medium">{number(summary?.arrivalsToday)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Departures today</dt>
                <dd className="font-medium">{number(summary?.departuresToday)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Available rooms</dt>
                <dd className="font-medium">{number(summary?.availableRooms)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">Average Daily Rate</dt>
                <dd className="font-medium">{money(summary?.adr)}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-muted-foreground">RevPAR</dt>
                <dd className="font-medium">{money(summary?.revPar)}</dd>
              </div>
            </dl>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
