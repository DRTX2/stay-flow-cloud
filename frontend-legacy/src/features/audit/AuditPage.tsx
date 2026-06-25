import { useMemo } from "react";
import { ScrollText } from "lucide-react";
import type { ColumnDef } from "@tanstack/react-table";
import type { AuditEntry } from "@/types/api";
import { PageHeader } from "@/components/shared/PageHeader";
import { EmptyState } from "@/components/shared/EmptyState";
import { DataTable } from "@/components/shared/data-table/DataTable";
import { DataTableColumnHeader } from "@/components/shared/data-table/DataTableColumnHeader";
import { Badge } from "@/components/ui/badge";
import { useAudit } from "./api";

export function AuditPage() {
  const { data, isLoading } = useAudit();

  const columns = useMemo<ColumnDef<AuditEntry>[]>(
    () => [
      {
        accessorKey: "timestamp",
        header: ({ column }) => <DataTableColumnHeader column={column} title="When" />,
        cell: ({ row }) =>
          row.original.timestamp
            ? new Date(row.original.timestamp).toLocaleString()
            : "—",
      },
      {
        accessorKey: "event",
        header: ({ column }) => <DataTableColumnHeader column={column} title="Event" />,
        cell: ({ row }) => (
          <Badge variant="outline" className="font-mono text-xs">
            {row.original.event ?? "—"}
          </Badge>
        ),
      },
      {
        accessorKey: "entityType",
        header: "Entity",
        cell: ({ row }) => row.original.entityType ?? "—",
      },
      {
        accessorKey: "userId",
        header: "User",
        cell: ({ row }) => (
          <span className="font-mono text-xs text-muted-foreground">
            {row.original.userId ?? "—"}
          </span>
        ),
      },
    ],
    [],
  );

  return (
    <div className="space-y-6">
      <PageHeader
        title="Audit Log"
        description="Immutable trail of domain events (stored in MongoDB)."
      />
      <DataTable
        columns={columns}
        data={data ?? []}
        isLoading={isLoading}
        searchPlaceholder="Search events…"
        exportFileName="audit.csv"
        pageSize={20}
        emptyState={
          <EmptyState
            icon={ScrollText}
            title="No audit entries"
            description="Actions across the platform are recorded here."
          />
        }
      />
    </div>
  );
}
