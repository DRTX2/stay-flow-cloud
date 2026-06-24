import { ToggleRight } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { EmptyState } from "@/components/shared/EmptyState";
import { Card, CardContent } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { useSetTenantFeature, useTenantFeatures } from "./api";

export function TenantFeaturesPage() {
  const { data, isLoading } = useTenantFeatures();
  const setFeature = useSetTenantFeature();
  const features = data ?? [];

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tenant Features"
        description="Per-tenant feature flags. Premium features are gated by plan."
      />

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-24 w-full rounded-xl" />
          ))}
        </div>
      ) : features.length === 0 ? (
        <EmptyState
          icon={ToggleRight}
          title="No feature flags"
          description="Feature flags configured for this tenant will appear here."
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {features.map((f) => {
            const id = `feature-${f.key}`;
            return (
              <Card key={f.key}>
                <CardContent className="flex items-start justify-between gap-3 p-5">
                  <div className="space-y-1">
                    <Label htmlFor={id} className="text-sm font-semibold">
                      {f.name ?? f.key}
                    </Label>
                    <p className="font-mono text-xs text-muted-foreground">{f.key}</p>
                  </div>
                  <Checkbox
                    id={id}
                    checked={!!f.enabled}
                    aria-label={`Toggle ${f.name ?? f.key}`}
                    onCheckedChange={(v) =>
                      f.key && setFeature.mutate({ key: f.key, enabled: v === true })
                    }
                  />
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}
