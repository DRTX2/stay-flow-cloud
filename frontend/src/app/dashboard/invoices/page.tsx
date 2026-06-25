import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { Invoice } from "@/types/api";
import { InvoicesTable } from "@/features/invoices/InvoicesTable";

export const metadata: Metadata = { title: "Invoices" };

export default async function InvoicesPage() {
  const invoices = await getList<Invoice>("/api/v1/invoices");
  return (
    <div className="space-y-6">
      <PageHeader title="Invoices" description="Billing documents and totals." />
      <InvoicesTable data={invoices} />
    </div>
  );
}
