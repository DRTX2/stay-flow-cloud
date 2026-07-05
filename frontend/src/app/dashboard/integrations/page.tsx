import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { IntegrationsView } from "@/features/integrations/IntegrationsView";

export const metadata: Metadata = { title: "Integrations" };

export default function IntegrationsPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Integrations"
        description="Connected services and available third-party integrations."
      />
      <IntegrationsView />
    </div>
  );
}
