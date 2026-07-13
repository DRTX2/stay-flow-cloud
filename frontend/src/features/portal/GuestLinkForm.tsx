"use client";

import { useTransition } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { linkGuestAction } from "@/app/portal/link/actions";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export function GuestLinkForm() {
  const [pending, startTransition] = useTransition();
  const router = useRouter();

  return (
    <form
      className="space-y-4"
      onSubmit={(event) => {
        event.preventDefault();
        const token = String(new FormData(event.currentTarget).get("token") ?? "").trim();
        startTransition(async () => {
          const result = await linkGuestAction(token);
          if (!result.ok) {
            toast.error(result.error);
            return;
          }
          toast.success("Your guest profile is connected.");
          router.push("/portal");
          router.refresh();
        });
      }}
    >
      <div className="space-y-2">
        <Label htmlFor="token">One-time invitation</Label>
        <Input id="token" name="token" autoComplete="off" required maxLength={256} />
      </div>
      <Button type="submit" disabled={pending}>
        {pending ? "Connecting..." : "Connect stay"}
      </Button>
    </form>
  );
}
