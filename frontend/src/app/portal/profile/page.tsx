import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { Card, CardContent } from "@/components/ui/card";
import { getList } from "@/server/api";
import { requireUser } from "@/server/auth/current-user";
import { ProfileForm } from "@/features/portal/ProfileForm";
import type { Guest } from "@/types/api";

export const metadata: Metadata = { title: "My Profile" };

export default async function PortalProfilePage() {
  const user = await requireUser();

  // Attempt to find the guest record matching the logged-in user's email.
  let guest: Guest | null = null;
  try {
    const guests = await getList<Guest>(
      `/api/v1/guests?pageSize=5&search=${encodeURIComponent(user.email ?? "")}`,
    );
    guest =
      guests.find((g) => g.email?.toLowerCase() === user.email?.toLowerCase()) ??
      guests[0] ??
      null;
  } catch {
    // Swallow — the form will render with empty defaults.
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="My Profile"
        description="View and update your personal details."
      />

      {!guest && (
        <Card>
          <CardContent className="p-6 text-sm text-muted-foreground">
            We could not find a guest profile linked to your account. Fill in the form
            below to create one.
          </CardContent>
        </Card>
      )}

      <ProfileForm guest={guest} />
    </div>
  );
}
