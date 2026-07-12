import type { Metadata } from "next";
import { MessageSquareText, Star } from "lucide-react";
import { PageHeader } from "@/components/shared/PageHeader";
import { EmptyState } from "@/components/shared/EmptyState";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { getPaged } from "@/server/api";
import type { StayFeedback } from "@/types/api";

export const metadata: Metadata = { title: "Guest Feedback" };

export default async function FeedbackPage() {
  const feedback = await getPaged<StayFeedback>("/api/v1/feedback", {
    page: 1,
    pageSize: 100,
  });
  return (
    <div className="space-y-6">
      <PageHeader
        title="Guest feedback"
        description="Private, verified-stay responses submitted after checkout."
      />
      {feedback.items.length === 0 ? (
        <EmptyState
          icon={MessageSquareText}
          title="No feedback yet"
          description="Generate a feedback invitation from a checked-out reservation."
        />
      ) : (
        <div className="grid gap-4 xl:grid-cols-2">
          {feedback.items.map((item) => (
            <Card key={item.id}>
              <CardHeader className="flex-row items-start justify-between gap-4">
                <div>
                  <CardTitle className="text-base">{item.guestName}</CardTitle>
                  <p className="mt-1 font-mono text-xs text-muted-foreground">
                    {item.confirmationCode}
                  </p>
                </div>
                <div className="flex items-center gap-1 font-semibold">
                  <Star className="h-4 w-4 fill-warning text-warning" />
                  {item.rating}/5
                </div>
              </CardHeader>
              <CardContent>
                <p className="text-sm leading-relaxed">
                  {item.comment || "No written comment."}
                </p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
