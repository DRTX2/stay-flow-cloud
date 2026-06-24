import { useRevenue } from "../api/hooks";
import { money, date } from "../format";
import type { RevenuePoint } from "../api/types";

function toCsv(rows: RevenuePoint[]): string {
  const header = "period,amount";
  const lines = rows.map((r) => {
    const period = r.date ?? r.period ?? "";
    const amount = r.amount ?? r.revenue ?? 0;
    return `${period},${amount}`;
  });
  return [header, ...lines].join("\n");
}

function download(name: string, content: string, type: string) {
  const blob = new Blob([content], { type });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = name;
  a.click();
  URL.revokeObjectURL(url);
}

export function Reports() {
  const { data, isLoading, isError } = useRevenue();
  const rows = data ?? [];

  return (
    <section>
      <div className="row-between">
        <h1>Revenue report</h1>
        <button
          className="primary"
          disabled={rows.length === 0}
          onClick={() => download("revenue.csv", toCsv(rows), "text/csv")}
        >
          Export CSV
        </button>
      </div>

      {isLoading && <div className="state">Loading…</div>}
      {isError && (
        <div className="state error">
          Could not load the revenue report. Requires{" "}
          <code>analytics:view</code>.
        </div>
      )}
      {data && rows.length === 0 && (
        <div className="state">No revenue in the selected window.</div>
      )}
      {rows.length > 0 && (
        <table className="grid">
          <thead>
            <tr>
              <th>Period</th>
              <th>Amount</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r, i) => (
              <tr key={i}>
                <td>{r.date ? date(r.date) : (r.period ?? "—")}</td>
                <td>{money(r.amount ?? r.revenue)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
