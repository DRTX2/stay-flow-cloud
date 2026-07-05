import type { Metadata } from "next";
import Link from "next/link";
import { PageHeader } from "@/components/shared/PageHeader";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { getJson } from "@/server/api";
import type { TenantFeaturesResponse } from "@/types/api";
import { TenantFeaturesGrid } from "@/features/tenant-features/TenantFeaturesGrid";

export const metadata: Metadata = { title: "Plan & Features" };

export default async function TenantFeaturesPage() {
  const tenantFeatures = await getJson<TenantFeaturesResponse>("/api/v1/tenantfeatures");
  const limits = tenantFeatures.limits;

  return (
    <div className="space-y-6">
      <PageHeader
        title="Plan & Features"
        description="Subscription status, usage limits, and feature gates for this tenant."
      />
      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>{tenantFeatures.plan ?? "Basic"} plan</CardTitle>
            <CardDescription>
              Upgrade to unlock higher limits and commercial PMS features.
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3 sm:grid-cols-3">
            <Limit label="Rooms" value={limits?.maxRooms} />
            <Limit label="Users" value={limits?.maxUsers} />
            <Limit label="Service items" value={limits?.maxServiceItems} />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Need more?</CardTitle>
            <CardDescription>
              Professional unlocks API access and advanced reports. Enterprise adds
              loyalty and multi-currency.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button asChild className="w-full">
              <Link href="/pricing">View upgrade options</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
      <TenantFeaturesGrid features={tenantFeatures.featureDetails ?? []} />
    </div>
  );
}

function Limit({ label, value }: { label: string; value?: number }) {
  return (
    <div className="rounded-lg border p-4">
      <p className="text-sm text-muted-foreground">{label}</p>
      <p className="mt-1 text-2xl font-bold">
        {value === 2147483647 ? "Unlimited" : (value ?? "—")}
      </p>
    </div>
  );
}
