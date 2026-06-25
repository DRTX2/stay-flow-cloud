import { useMemo } from "react";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Line,
  LineChart,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import type { RevenuePoint } from "@/types/api";
import { formatDate, money } from "@/lib/format";

const AXIS = "hsl(var(--muted-foreground))";
const GRID = "hsl(var(--border))";

interface Series {
  label: string;
  amount: number;
}

function toSeries(points: RevenuePoint[]): Series[] {
  return points.map((p) => ({
    label: p.date ? formatDate(p.date) : (p.period ?? ""),
    amount: p.amount ?? p.revenue ?? 0,
  }));
}

const tooltipStyle = {
  backgroundColor: "hsl(var(--popover))",
  border: "1px solid hsl(var(--border))",
  borderRadius: 8,
  color: "hsl(var(--popover-foreground))",
  fontSize: 12,
};

export function RevenueAreaChart({ points }: { points: RevenuePoint[] }) {
  const data = useMemo(() => toSeries(points), [points]);
  return (
    <ResponsiveContainer width="100%" height={280}>
      <AreaChart data={data} margin={{ left: 4, right: 8, top: 8 }}>
        <defs>
          <linearGradient id="rev" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor="hsl(var(--primary))" stopOpacity={0.4} />
            <stop offset="95%" stopColor="hsl(var(--primary))" stopOpacity={0} />
          </linearGradient>
        </defs>
        <CartesianGrid strokeDasharray="3 3" stroke={GRID} vertical={false} />
        <XAxis
          dataKey="label"
          stroke={AXIS}
          fontSize={12}
          tickLine={false}
          axisLine={false}
        />
        <YAxis
          stroke={AXIS}
          fontSize={12}
          tickLine={false}
          axisLine={false}
          tickFormatter={(v) => money(Number(v))}
          width={64}
        />
        <Tooltip contentStyle={tooltipStyle} formatter={(v) => money(Number(v))} />
        <Area
          type="monotone"
          dataKey="amount"
          stroke="hsl(var(--primary))"
          strokeWidth={2}
          fill="url(#rev)"
        />
      </AreaChart>
    </ResponsiveContainer>
  );
}

export function RevenueBarChart({ points }: { points: RevenuePoint[] }) {
  const data = useMemo(() => toSeries(points).slice(-12), [points]);
  return (
    <ResponsiveContainer width="100%" height={260}>
      <BarChart data={data} margin={{ left: 4, right: 8, top: 8 }}>
        <CartesianGrid strokeDasharray="3 3" stroke={GRID} vertical={false} />
        <XAxis
          dataKey="label"
          stroke={AXIS}
          fontSize={12}
          tickLine={false}
          axisLine={false}
        />
        <YAxis
          stroke={AXIS}
          fontSize={12}
          tickLine={false}
          axisLine={false}
          tickFormatter={(v) => money(Number(v))}
          width={64}
        />
        <Tooltip
          cursor={{ fill: "hsl(var(--muted))", opacity: 0.4 }}
          contentStyle={tooltipStyle}
          formatter={(v) => money(Number(v))}
        />
        <Bar dataKey="amount" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  );
}

export function CumulativeRevenueLine({ points }: { points: RevenuePoint[] }) {
  const data = useMemo(() => {
    const series = toSeries(points);
    return series.map((s, i) => ({
      label: s.label,
      total: series.slice(0, i + 1).reduce((sum, point) => sum + point.amount, 0),
    }));
  }, [points]);
  return (
    <ResponsiveContainer width="100%" height={260}>
      <LineChart data={data} margin={{ left: 4, right: 8, top: 8 }}>
        <CartesianGrid strokeDasharray="3 3" stroke={GRID} vertical={false} />
        <XAxis
          dataKey="label"
          stroke={AXIS}
          fontSize={12}
          tickLine={false}
          axisLine={false}
        />
        <YAxis
          stroke={AXIS}
          fontSize={12}
          tickLine={false}
          axisLine={false}
          tickFormatter={(v) => money(Number(v))}
          width={64}
        />
        <Tooltip contentStyle={tooltipStyle} formatter={(v) => money(Number(v))} />
        <Line
          type="monotone"
          dataKey="total"
          stroke="hsl(var(--primary))"
          strokeWidth={2}
          dot={false}
        />
      </LineChart>
    </ResponsiveContainer>
  );
}

const PIE_COLORS = ["hsl(var(--primary))", "hsl(var(--muted))"];

export function OccupancyPie({ occupancyRate }: { occupancyRate?: number }) {
  const rate =
    occupancyRate == null ? 0 : occupancyRate <= 1 ? occupancyRate : occupancyRate / 100;
  const data = [
    { name: "Occupied", value: Math.round(rate * 100) },
    { name: "Available", value: Math.round((1 - rate) * 100) },
  ];
  return (
    <ResponsiveContainer width="100%" height={260}>
      <PieChart>
        <Pie
          data={data}
          dataKey="value"
          nameKey="name"
          innerRadius={60}
          outerRadius={90}
          paddingAngle={2}
        >
          {data.map((_, i) => (
            <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />
          ))}
        </Pie>
        <Tooltip contentStyle={tooltipStyle} formatter={(v) => `${v}%`} />
      </PieChart>
    </ResponsiveContainer>
  );
}
