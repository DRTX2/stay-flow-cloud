import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { AuditEntry } from "@/types/api";
import { AuditTable } from "@/features/audit/AuditTable";

export const metadata: Metadata = { title: "Audit Log" };

export default async function AuditPage() {
  const entries = await getList<AuditEntry>("/api/v1/audit");
  return (
    <div className="space-y-6">
      <PageHeader
        title="Audit Log"
        description="Immutable trail of domain events (stored in MongoDB)."
      />
      <AuditTable data={entries} />
    </div>
  );
}
