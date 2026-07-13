"use client";

import { useState, useTransition } from "react";
import { CheckCircle2, Loader2, Star } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { submitFeedbackAction } from "@/app/(public)/feedback/actions";
import { cn } from "@/lib/utils";
import type { Locale } from "@/i18n/config";

export function FeedbackForm({ locale }: { locale: Locale }) {
  const [rating, setRating] = useState(0);
  const [comment, setComment] = useState("");
  const [error, setError] = useState("");
  const [submitted, setSubmitted] = useState(false);
  const [pending, startTransition] = useTransition();

  function submit() {
    setError("");
    const token = window.location.hash.slice(1);
    if (!token)
      return setError(
        locale === "es"
          ? "Este enlace está incompleto o no es válido."
          : "This feedback link is incomplete or invalid.",
      );
    if (rating < 1)
      return setError(
        locale === "es"
          ? "Selecciona una puntuación antes de enviar."
          : "Select a rating before submitting.",
      );
    startTransition(async () => {
      const result = await submitFeedbackAction({ token, rating, comment });
      if (result.ok) setSubmitted(true);
      else
        setError(
          result.error ??
            (locale === "es"
              ? "No se pudo enviar la opinión."
              : "Could not submit feedback."),
        );
    });
  }

  if (submitted) {
    return (
      <Card>
        <CardContent
          role="status"
          className="flex flex-col items-center gap-3 p-10 text-center"
        >
          <CheckCircle2 className="h-12 w-12 text-success" />
          <h2 className="text-xl font-semibold">
            {locale === "es" ? "Gracias por tu opinión" : "Thank you for your feedback"}
          </h2>
          <p className="text-sm text-muted-foreground">
            {locale === "es"
              ? "Tu respuesta se compartió de forma privada con el equipo del hotel."
              : "Your response has been shared privately with the hotel team."}
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>
          {locale === "es" ? "¿Cómo fue tu estancia?" : "How was your stay?"}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        <fieldset className="space-y-2">
          <legend className="text-sm font-medium">
            {locale === "es" ? "Puntuación general" : "Overall rating"}
          </legend>
          <div className="flex gap-1" aria-label="Rating from 1 to 5">
            {[1, 2, 3, 4, 5].map((value) => (
              <button
                key={value}
                type="button"
                onClick={() => setRating(value)}
                className="rounded-md p-2 focus-visible:ring-2"
                aria-label={`${value} star${value > 1 ? "s" : ""}`}
                aria-pressed={rating === value}
              >
                <Star
                  className={cn(
                    "h-7 w-7",
                    value <= rating
                      ? "fill-warning text-warning"
                      : "text-muted-foreground",
                  )}
                />
              </button>
            ))}
          </div>
        </fieldset>
        <div className="space-y-2">
          <Label htmlFor="feedback-comment">
            {locale === "es" ? "Comentarios (opcional)" : "Comments (optional)"}
          </Label>
          <Textarea
            id="feedback-comment"
            value={comment}
            onChange={(event) => setComment(event.target.value)}
            maxLength={2000}
            rows={6}
            placeholder={
              locale === "es"
                ? "Cuéntale al hotel qué salió bien y qué podría mejorar."
                : "Tell the hotel what went well and what could be improved."
            }
            disabled={pending}
          />
          <p className="text-right text-xs text-muted-foreground">
            {comment.length}/2000
          </p>
        </div>
        {error && (
          <p
            role="alert"
            className="rounded-md bg-destructive/10 p-3 text-sm text-destructive"
          >
            {error}
          </p>
        )}
        <Button
          className="w-full"
          onClick={submit}
          disabled={pending || rating < 1}
          aria-busy={pending}
        >
          {pending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {pending
            ? locale === "es"
              ? "Enviando..."
              : "Submitting..."
            : locale === "es"
              ? "Enviar opinión privada"
              : "Submit private feedback"}
        </Button>
        <p className="text-center text-xs text-muted-foreground">
          {locale === "es"
            ? "Esta invitación de un solo uso vence 30 días después de emitirse."
            : "This one-time invitation expires 30 days after it is issued."}
        </p>
      </CardContent>
    </Card>
  );
}
