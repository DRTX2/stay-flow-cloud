import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getPaged } from "@/server/api";
import { parsePageParams, type SearchParams } from "@/lib/pagination";
import type { Guest } from "@/types/api";
import { GuestsTable } from "@/features/guests/GuestsTable";
import { GuestFormDialog } from "@/features/guests/GuestFormDialog";

export const metadata: Metadata = { title: "Guests" };

export default async function GuestsPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  const { page, pageSize, search } = parsePageParams(await searchParams);
  const result = await getPaged<Guest>("/api/v1/guests", { page, pageSize, search });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Guests"
        description="Guest profiles for this tenant."
        actions={<GuestFormDialog />}
      />
      <GuestsTable
        data={result.items}
        pagination={{
          page: result.page,
          pageSize: result.pageSize,
          totalCount: result.totalCount,
          totalPages: result.totalPages,
          search: { value: search ?? "", placeholder: "Search guests…" },
        }}
      />
    </div>
  );
}
