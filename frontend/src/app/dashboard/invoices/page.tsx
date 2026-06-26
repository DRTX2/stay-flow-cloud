import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getPaged } from "@/server/api";
import { parsePageParams, type SearchParams } from "@/lib/pagination";
import type { Invoice } from "@/types/api";
import { InvoicesTable } from "@/features/invoices/InvoicesTable";

export const metadata: Metadata = { title: "Invoices" };

export default async function InvoicesPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  const { page, pageSize } = parsePageParams(await searchParams);
  const result = await getPaged<Invoice>("/api/v1/invoices", { page, pageSize });

  return (
    <div className="space-y-6">
      <PageHeader title="Invoices" description="Billing documents and totals." />
      <InvoicesTable
        data={result.items}
        pagination={{
          page: result.page,
          pageSize: result.pageSize,
          totalCount: result.totalCount,
          totalPages: result.totalPages,
        }}
      />
    </div>
  );
}
