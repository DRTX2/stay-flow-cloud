"use client";

import { useOptimistic, useTransition } from "react";
import Link from "next/link";
import { Lock, ToggleRight } from "lucide-react";
import { toast } from "sonner";
import type { TenantFeature } from "@/types/api";
import { EmptyState } from "@/components/shared/EmptyState";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { setTenantFeatureAction } from "@/app/dashboard/tenant-features/actions";

export function TenantFeaturesGrid({ features }: { features: TenantFeature[] }) {
  const [optimistic, applyOptimistic] = useOptimistic(
    features,
    (state, update: { key: string; enabled: boolean }) =>
      state.map((f) => (f.key === update.key ? { ...f, enabled: update.enabled } : f)),
  );
  const [, startTransition] = useTransition();

  function toggle(key: string, enabled: boolean) {
    startTransition(async () => {
      applyOptimistic({ key, enabled });
      const result = await setTenantFeatureAction(key, enabled);
      if (result.ok) toast.success("Feature updated");
      else toast.error(result.error ?? "Could not update feature");
    });
  }

  if (optimistic.length === 0) {
    return (
      <EmptyState
        icon={ToggleRight}
        title="No feature flags"
        description="Feature flags configured for this tenant will appear here."
      />
    );
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {optimistic.map((f) => {
        const id = `feature-${f.key}`;
        const locked = f.includedInPlan === false;
        return (
          <Card key={f.key}>
            <CardContent className="flex items-start justify-between gap-3 p-5">
              <div className="space-y-1">
                <div className="flex flex-wrap items-center gap-2">
                  <Label htmlFor={id} className="text-sm font-semibold">
                    {f.name ?? f.key}
                  </Label>
                  {locked && (
                    <Badge variant="secondary" className="gap-1">
                      <Lock className="h-3 w-3" /> {f.requiredPlan ?? "Upgrade"}
                    </Badge>
                  )}
                </div>
                <p className="font-mono text-xs text-muted-foreground">{f.key}</p>
                {locked && (
                  <Button asChild variant="link" size="sm" className="h-auto p-0 text-xs">
                    <Link href="/pricing">Upgrade to unlock</Link>
                  </Button>
                )}
              </div>
              <Checkbox
                id={id}
                checked={!!f.enabled}
                disabled={locked}
                aria-label={`Toggle ${f.name ?? f.key}`}
                onCheckedChange={(v) => f.key && toggle(f.key, v === true)}
              />
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
