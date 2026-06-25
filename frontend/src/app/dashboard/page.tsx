import { requireUser } from "@/server/auth/current-user";

// Placeholder — replaced by the full executive dashboard in the RSC migration phase.
export default async function DashboardPage() {
  const user = await requireUser();
  return (
    <main className="p-8">
      <h1 className="text-2xl font-semibold">Dashboard</h1>
      <p className="mt-2 text-muted-foreground">
        Signed in as {user.name ?? user.email ?? user.id}
      </p>
    </main>
  );
}
