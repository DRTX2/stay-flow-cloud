import type { Metadata } from "next";
import { FeedbackForm } from "@/features/feedback/FeedbackForm";

export const metadata: Metadata = { title: "Stay feedback", robots: { index: false } };

export default function FeedbackPage() {
  return (
    <main id="main-content" tabIndex={-1} className="mx-auto max-w-xl px-4 py-12 sm:px-6">
      <div className="mb-6">
        <h1 className="text-3xl font-bold tracking-tight">Share your experience</h1>
        <p className="mt-2 text-muted-foreground">
          Your private feedback helps the hotel improve future stays.
        </p>
      </div>
      <FeedbackForm />
    </main>
  );
}
