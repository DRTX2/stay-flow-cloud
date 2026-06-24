import type { ReactNode } from "react";
import type { UseQueryResult } from "@tanstack/react-query";

interface Column<T> {
  header: string;
  render: (row: T) => ReactNode;
}

interface Props<T> {
  query: UseQueryResult<T[]>;
  columns: Column<T>[];
  rowKey: (row: T) => string;
  empty?: string;
}

// Renders a list query with loading/error/empty states handled in one place.
export function DataTable<T>({ query, columns, rowKey, empty }: Props<T>) {
  if (query.isLoading) return <div className="state">Loading…</div>;
  if (query.isError)
    return (
      <div className="state error">
        Failed to load. Is the API running and are you signed in with the right
        permissions?
      </div>
    );
  const rows = query.data ?? [];
  if (rows.length === 0)
    return <div className="state">{empty ?? "No records yet."}</div>;

  return (
    <table className="grid">
      <thead>
        <tr>
          {columns.map((c) => (
            <th key={c.header}>{c.header}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {rows.map((row) => (
          <tr key={rowKey(row)}>
            {columns.map((c) => (
              <td key={c.header}>{c.render(row)}</td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
}
