import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { getList } from "@/server/api";
import type { Guest } from "@/types/api";
import { GuestsTable } from "@/features/guests/GuestsTable";
import { GuestFormDialog } from "@/features/guests/GuestFormDialog";

export const metadata: Metadata = { title: "Guests" };

export default async function GuestsPage() {
  const guests = await getList<Guest>("/api/v1/guests");
  return (
    <div className="space-y-6">
      <PageHeader
        title="Guests"
        description="Guest profiles for this tenant."
        actions={<GuestFormDialog />}
      />
      <GuestsTable data={guests} />
    </div>
  );
}
