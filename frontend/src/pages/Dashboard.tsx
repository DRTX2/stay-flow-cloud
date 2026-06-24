import { useDashboard } from "../api/hooks";
import { money, percent } from "../format";

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="card stat">
      <span className="stat-value">{value}</span>
      <span className="stat-label">{label}</span>
    </div>
  );
}

export function Dashboard() {
  const { data, isLoading, isError } = useDashboard();

  return (
    <section>
      <h1>Dashboard</h1>
      {isLoading && <div className="state">Loading…</div>}
      {isError && (
        <div className="state error">
          Could not load analytics. Requires the <code>analytics:view</code>{" "}
          permission.
        </div>
      )}
      {data && (
        <div className="stats">
          <Stat
            label="Reservations"
            value={String(data.totalReservations ?? "—")}
          />
          <Stat label="Occupancy" value={percent(data.occupancyRate)} />
          <Stat label="Revenue (30d)" value={money(data.revenue)} />
          <Stat
            label="Available rooms"
            value={String(data.availableRooms ?? "—")}
          />
          <Stat
            label="Arrivals today"
            value={String(data.arrivalsToday ?? "—")}
          />
          <Stat
            label="Departures today"
            value={String(data.departuresToday ?? "—")}
          />
        </div>
      )}
    </section>
  );
}
