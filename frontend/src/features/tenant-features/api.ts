import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { http } from "@/services/http";
import { getList } from "@/services/list";
import type { TenantFeature } from "@/types/api";

const key = ["tenant-features"] as const;

export function useTenantFeatures() {
  return useQuery({
    queryKey: key,
    queryFn: () => getList<TenantFeature>("/api/v1/tenantfeatures"),
  });
}

export function useSetTenantFeature() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (vars: { key: string; enabled: boolean }) => {
      await http.put(`/api/v1/tenantfeatures/${vars.key}`, {
        enabled: vars.enabled,
      });
    },
    // Optimistic toggle: flip locally, roll back on error.
    onMutate: async (vars) => {
      await qc.cancelQueries({ queryKey: key });
      const prev = qc.getQueryData<TenantFeature[]>(key);
      qc.setQueryData<TenantFeature[]>(key, (old) =>
        (old ?? []).map((f) =>
          f.key === vars.key ? { ...f, enabled: vars.enabled } : f,
        ),
      );
      return { prev };
    },
    onError: (_e, _vars, ctx) => {
      if (ctx?.prev) qc.setQueryData(key, ctx.prev);
    },
    onSettled: () => qc.invalidateQueries({ queryKey: key }),
  });
}
