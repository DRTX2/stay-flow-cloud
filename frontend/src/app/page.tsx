import Link from "next/link";

export default function HomePage() {
  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col items-center justify-center gap-6 px-6 text-center">
      <p className="rounded-full border px-3 py-1 text-xs font-medium text-muted-foreground">
        StayFlow Cloud · Next.js {process.env.NEXT_RUNTIME ? "" : ""}App Router
      </p>
      <h1 className="text-balance text-4xl font-bold tracking-tight sm:text-5xl">
        Hospitality management, reimagined for the cloud.
      </h1>
      <p className="text-balance text-lg text-muted-foreground">
        Reservations, front desk, billing and analytics — multi-tenant, secure, fast.
      </p>
      <div className="flex gap-3">
        <Link
          href="/hotels"
          className="rounded-md bg-primary px-5 py-2.5 text-sm font-medium text-primary-foreground"
        >
          Browse hotels
        </Link>
        <Link
          href="/dashboard"
          className="rounded-md border px-5 py-2.5 text-sm font-medium"
        >
          Go to dashboard
        </Link>
      </div>
    </main>
  );
}
