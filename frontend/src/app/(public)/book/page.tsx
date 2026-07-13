import type { Metadata } from "next";
import { BookingForm, type BookingHotel } from "@/features/booking/BookingForm";
import { getJson } from "@/server/api";
import { getLocale } from "@/i18n/server";

export const metadata: Metadata = {
  title: "Book a stay",
  description: "Request a booking at a StayFlow Cloud property.",
  robots: { index: false, follow: true },
};

export default async function BookPage({
  searchParams,
}: {
  searchParams: Promise<{ hotel?: string; roomType?: string }>;
}) {
  const { hotel, roomType } = await searchParams;
  const [hotels, locale] = await Promise.all([
    getJson<BookingHotel[]>("/api/v1/public/hotels", { auth: false }),
    getLocale(),
  ]);

  const formHotels: BookingHotel[] = hotels.map((h) => ({
    slug: h.slug,
    name: h.name ?? h.slug,
    roomTypes: (h.roomTypes ?? []).map((rt) => ({
      id: rt.id,
      name: rt.name ?? rt.id,
      baseRate: rt.baseRate ?? 0,
      maxOccupancy: rt.maxOccupancy,
    })),
  }));

  return (
    <main id="main-content" tabIndex={-1} className="mx-auto max-w-xl px-4 py-12 sm:px-6">
      <h1 className="text-3xl font-bold tracking-tight">
        {locale === "es" ? "Reserva tu estancia" : "Book your stay"}
      </h1>
      <p className="mt-1 text-muted-foreground">
        {locale === "es"
          ? "Envía una solicitud y nuestro equipo confirmará la disponibilidad."
          : "Send a booking enquiry and our team will confirm availability."}
      </p>
      <div className="mt-6">
        <BookingForm
          hotels={formHotels}
          initialHotel={hotel}
          initialRoomType={roomType}
          locale={locale}
        />
      </div>
    </main>
  );
}
