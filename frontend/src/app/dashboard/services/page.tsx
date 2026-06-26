import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { ServiceItem } from "@/types/api";
import { ServicesTable } from "@/features/services/ServicesTable";
import { CreateServiceDialog } from "@/features/services/CreateServiceDialog";

export const metadata: Metadata = { title: "Services" };

export default async function ServicesPage() {
  const services = await getList<ServiceItem>("/api/v1/services");
  return (
    <div className="space-y-6">
      <PageHeader
        title="Services"
        description="Sellable extras attached to stays."
        actions={<CreateServiceDialog />}
      />
      <ServicesTable data={services} />
    </div>
  );
}
