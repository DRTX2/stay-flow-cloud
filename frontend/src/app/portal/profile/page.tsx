import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { Card, CardContent } from "@/components/ui/card";
import { getJson } from "@/server/api";
import { ProfileForm } from "@/features/portal/ProfileForm";
import type { Guest } from "@/types/api";

export const metadata: Metadata = { title: "My Profile" };

export default async function PortalProfilePage() {
  let guest: Guest | null = null;
  try {
    guest = await getJson<Guest>("/api/v1/portal/profile");
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
            We could not find a guest profile securely linked to your account. Contact the
            hotel to verify that your booking uses the same email address.
          </CardContent>
        </Card>
      )}

      {guest && <ProfileForm guest={guest} />}
    </div>
  );
}
