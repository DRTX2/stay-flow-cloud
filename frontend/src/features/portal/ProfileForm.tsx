"use client";

import { useTransition } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { Guest } from "@/types/api";
import { updateProfileAction } from "@/app/portal/profile/actions";

interface ProfileFormProps {
  guest: Guest | null;
}

export function ProfileForm({ guest }: ProfileFormProps) {
  const [isPending, startTransition] = useTransition();
  const router = useRouter();

  function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);

    startTransition(async () => {
      const result = await updateProfileAction({
        firstName: fd.get("firstName") as string,
        lastName: fd.get("lastName") as string,
        email: fd.get("email") as string,
        phone: (fd.get("phone") as string) || undefined,
        documentNumber: (fd.get("documentNumber") as string) || undefined,
      });
      if (result.ok) {
        toast.success("Profile updated successfully.");
        router.refresh();
      } else {
        toast.error(result.error ?? "Could not update profile.");
      }
    });
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Personal details</CardTitle>
        <CardDescription>
          Update your personal information. Changes are saved to your guest profile.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="grid gap-4 sm:grid-cols-2">
          <div className="space-y-2">
            <Label htmlFor="firstName">First name</Label>
            <Input
              id="firstName"
              name="firstName"
              defaultValue={guest?.firstName ?? ""}
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="lastName">Last name</Label>
            <Input
              id="lastName"
              name="lastName"
              defaultValue={guest?.lastName ?? ""}
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              name="email"
              type="email"
              defaultValue={guest?.email ?? ""}
              required
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="phone">Phone</Label>
            <Input id="phone" name="phone" type="tel" defaultValue={guest?.phone ?? ""} />
          </div>
          <div className="space-y-2 sm:col-span-2">
            <Label htmlFor="documentNumber">Document / ID number</Label>
            <Input
              id="documentNumber"
              name="documentNumber"
              defaultValue={guest?.documentNumber ?? ""}
              placeholder="Passport or national ID"
            />
          </div>
          <div className="sm:col-span-2">
            <Button type="submit" disabled={isPending}>
              {isPending ? "Saving…" : "Save changes"}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
