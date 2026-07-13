import { PageHeader } from "@/components/shared/PageHeader";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { GuestLinkForm } from "@/features/portal/GuestLinkForm";

export default function GuestLinkPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Connect your stay"
        description="Securely link this account to your hotel guest profile."
      />
      <Card className="max-w-xl">
        <CardHeader>
          <CardTitle>Guest invitation</CardTitle>
          <CardDescription>
            The code expires after 48 hours and can only be used once.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <GuestLinkForm />
        </CardContent>
      </Card>
    </div>
  );
}
