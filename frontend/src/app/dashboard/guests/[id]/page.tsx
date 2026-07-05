import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { StatusBadge } from "@/components/shared/StatusBadge";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { getJson } from "@/server/api";
import { money2 } from "@/lib/format";
import type { GuestProfile } from "@/types/api";

export const metadata: Metadata = { title: "Guest Profile | StayFlow" };

function guestName(profile: GuestProfile): string {
  const name =
    profile.guest.fullName ??
    `${profile.guest.firstName ?? ""} ${profile.guest.lastName ?? ""}`.trim();
  return name || profile.guest.email || "Guest";
}

export default async function GuestProfilePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const profile = await getJson<GuestProfile>(`/api/v1/guests/${id}/profile`);

  return (
    <div className="space-y-6">
      <PageHeader
        title={guestName(profile)}
        description="Guest Profile 360: contact, stay history, and billing value."
      />

      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader>
            <CardTitle>Contact</CardTitle>
            <CardDescription>{profile.guest.email ?? "No email"}</CardDescription>
          </CardHeader>
          <CardContent className="space-y-1 text-sm text-muted-foreground">
            <p>Phone: {profile.guest.phone ?? "Not captured"}</p>
            <p>Document: {profile.guest.documentNumber ?? "Not captured"}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Total stays</CardTitle>
          </CardHeader>
          <CardContent className="text-3xl font-bold">
            {profile.totalStays ?? 0}
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Lifetime value</CardTitle>
          </CardHeader>
          <CardContent className="text-3xl font-bold">
            {money2(profile.lifetimeValue)}
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Last stay</CardTitle>
          </CardHeader>
          <CardContent className="text-3xl font-bold">
            {profile.lastStay ?? "—"}
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Reservation history</CardTitle>
            <CardDescription>
              All stays for this guest in the current tenant.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {(profile.reservations ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No reservation history.</p>
            ) : (
              profile.reservations?.map((reservation) => (
                <div key={reservation.id} className="rounded-lg border p-3">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium">
                        Room {reservation.roomNumber ?? "TBD"}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {reservation.checkIn} to {reservation.checkOut} ·{" "}
                        {reservation.confirmationCode}
                      </p>
                    </div>
                    <StatusBadge status={reservation.status} />
                  </div>
                  <p className="mt-2 text-sm font-medium">
                    {money2(reservation.totalPrice)}
                  </p>
                </div>
              ))
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Invoices</CardTitle>
            <CardDescription>Billing summary across the guest history.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {(profile.invoices ?? []).length === 0 ? (
              <p className="text-sm text-muted-foreground">No invoices yet.</p>
            ) : (
              profile.invoices?.map((invoice) => (
                <div
                  key={invoice.id}
                  className="flex items-center justify-between rounded-lg border p-3"
                >
                  <div>
                    <p className="font-medium">{invoice.number ?? invoice.id}</p>
                    <p className="text-xs text-muted-foreground">
                      Paid: {invoice.paidAtUtc ?? "Not paid"}
                    </p>
                  </div>
                  <div className="text-right">
                    <Badge variant="outline">{invoice.status ?? "Draft"}</Badge>
                    <p className="mt-1 text-sm font-medium">{money2(invoice.total)}</p>
                  </div>
                </div>
              ))
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
