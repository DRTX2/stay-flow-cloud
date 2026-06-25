import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { DocumentItem } from "@/types/api";
import { DocumentsTable } from "@/features/documents/DocumentsTable";

export const metadata: Metadata = { title: "Documents" };

export default async function DocumentsPage() {
  const documents = await getList<DocumentItem>("/api/v1/documents");
  return (
    <div className="space-y-6">
      <PageHeader
        title="Documents"
        description="Tenant-scoped files in S3 (contracts, invoices, IDs)."
      />
      <DocumentsTable data={documents} />
    </div>
  );
}
