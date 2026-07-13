import type { Metadata } from "next";
import { ShieldCheck, Users } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ActionForm, ActionSubmit } from "@/components/shared/ActionForm";
import { getJson } from "@/server/api";
import type { StaffUsersResponse } from "@/types/api";
import { createStaffUserAction, updateStaffRolesAction } from "./actions";

export const metadata: Metadata = { title: "Staff" };

export default async function StaffPage() {
  const staff = await getJson<StaffUsersResponse>("/api/v1/staff");
  const assignableRoles = staff.assignableRoles ?? [
    "FrontDesk",
    "Housekeeping",
    "Manager",
    "Admin",
  ];
  const users = staff.users ?? [];

  return (
    <div className="space-y-6">
      <PageHeader
        title="Staff & Roles"
        description="Create tenant staff and assign operational roles without touching seed data."
      />

      <Card>
        <CardHeader>
          <CardTitle>Create staff user</CardTitle>
          <CardDescription>
            Use a temporary password and ask the staff member to change it after first
            login.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ActionForm
            action={createStaffUserAction}
            className="grid gap-3 lg:grid-cols-[1fr_1fr_1fr_1fr_auto] lg:items-end"
          >
            <div className="space-y-2">
              <Label htmlFor="staff-name">Full name</Label>
              <Input id="staff-name" name="fullName" autoComplete="name" required />
            </div>
            <div className="space-y-2">
              <Label htmlFor="staff-email">Email</Label>
              <Input
                id="staff-email"
                name="email"
                type="email"
                autoComplete="email"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="staff-password">Temporary password</Label>
              <Input
                id="staff-password"
                name="password"
                type="password"
                autoComplete="new-password"
                minLength={8}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="staff-role">Role</Label>
              <select
                id="staff-role"
                name="roles"
                className="h-9 w-full rounded-md border bg-background px-3 text-sm"
                defaultValue="FrontDesk"
              >
                {assignableRoles.map((role) => (
                  <option key={role} value={role}>
                    {role}
                  </option>
                ))}
              </select>
            </div>
            <ActionSubmit pendingLabel="Creating…">Create</ActionSubmit>
          </ActionForm>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Users className="h-5 w-5" /> Staff directory
          </CardTitle>
          <CardDescription>
            Roles map to permissions in the backend token, not just UI labels.
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          {users.length === 0 ? (
            <div className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground">
              No staff users yet.
            </div>
          ) : (
            users.map((user) => (
              <div
                key={user.id}
                className="grid gap-3 rounded-lg border p-3 lg:grid-cols-[1fr_auto] lg:items-center"
              >
                <div>
                  <div className="flex flex-wrap items-center gap-2">
                    <p className="font-medium">{user.fullName || user.email}</p>
                    {user.isActive ? (
                      <Badge variant="outline">Active</Badge>
                    ) : (
                      <Badge variant="secondary">Inactive</Badge>
                    )}
                  </div>
                  <p className="text-sm text-muted-foreground">{user.email}</p>
                  <div className="mt-2 flex flex-wrap gap-2">
                    {(user.roles ?? []).map((role) => (
                      <Badge key={role} variant="secondary" className="gap-1">
                        <ShieldCheck className="h-3 w-3" /> {role}
                      </Badge>
                    ))}
                  </div>
                </div>
                <ActionForm
                  action={updateStaffRolesAction}
                  className="flex flex-wrap items-center gap-2"
                >
                  <input type="hidden" name="id" value={user.id} />
                  <Label htmlFor={`staff-role-${user.id}`} className="sr-only">
                    Role for {user.fullName || user.email}
                  </Label>
                  <select
                    id={`staff-role-${user.id}`}
                    name="roles"
                    className="h-9 rounded-md border bg-background px-3 text-sm"
                    defaultValue={(user.roles ?? ["FrontDesk"])[0]}
                  >
                    {assignableRoles.map((role) => (
                      <option key={role} value={role}>
                        {role}
                      </option>
                    ))}
                  </select>
                  <ActionSubmit pendingLabel="Updating…" size="sm" variant="outline">
                    Update role
                  </ActionSubmit>
                </ActionForm>
              </div>
            ))
          )}
        </CardContent>
      </Card>
    </div>
  );
}
