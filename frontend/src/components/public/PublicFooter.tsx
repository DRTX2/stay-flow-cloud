import Link from "next/link";
import { getDictionary } from "@/i18n/server";

export async function PublicFooter() {
  const dictionary = await getDictionary();
  return (
    <footer className="border-t">
      <div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-10 sm:flex-row sm:items-center sm:justify-between sm:px-6">
        <div>
          <p className="text-sm font-semibold">StayFlow Cloud</p>
          <p className="mt-1 text-sm text-muted-foreground">
            {dictionary.common.tagline}
          </p>
        </div>
        <nav className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-muted-foreground">
          <Link href="/hotels" className="hover:text-foreground">
            {dictionary.common.hotels}
          </Link>
          <Link href="/pricing" className="hover:text-foreground">
            {dictionary.common.pricing}
          </Link>
          <Link href="/dashboard" className="hover:text-foreground">
            {dictionary.common.dashboard}
          </Link>
          <Link href="/login" className="hover:text-foreground">
            {dictionary.common.signIn}
          </Link>
        </nav>
        <p className="text-xs text-muted-foreground">
          © {new Date().getFullYear()} StayFlow Cloud
        </p>
      </div>
    </footer>
  );
}
