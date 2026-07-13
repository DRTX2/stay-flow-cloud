import type { Metadata } from "next";
import { FeedbackForm } from "@/features/feedback/FeedbackForm";
import { getLocale } from "@/i18n/server";

export const metadata: Metadata = { title: "Stay feedback", robots: { index: false } };

export default async function FeedbackPage() {
  const locale = await getLocale();
  return (
    <main id="main-content" tabIndex={-1} className="mx-auto max-w-xl px-4 py-12 sm:px-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold tracking-tight">
          {locale === "es" ? "Comparte tu experiencia" : "Share your experience"}
        </h1>
        <p className="mt-2 text-muted-foreground">
          {locale === "es"
            ? "Tu opinión privada ayuda al hotel a mejorar futuras estancias."
            : "Your private feedback helps the hotel improve future stays."}
        </p>
      </div>
      <FeedbackForm locale={locale} />
    </main>
  );
}
