import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { TenantFeature } from "@/types/api";
import { TenantFeaturesGrid } from "@/features/tenant-features/TenantFeaturesGrid";

export const metadata: Metadata = { title: "Tenant Features" };

export default async function TenantFeaturesPage() {
  const features = await getList<TenantFeature>("/api/v1/tenantfeatures");
  return (
    <div className="space-y-6">
      <PageHeader
        title="Tenant Features"
        description="Per-tenant feature flags. Premium features are gated by plan."
      />
      <TenantFeaturesGrid features={features} />
    </div>
  );
}
