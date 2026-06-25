import type { Metadata } from "next";
import { getHotels } from "@/content/hotels";
import { BookingForm, type BookingHotel } from "@/features/booking/BookingForm";

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
  const hotels = await getHotels();

  const formHotels: BookingHotel[] = hotels.map((h) => ({
    slug: h.slug,
    name: h.name ?? h.slug,
    roomTypes: (h.roomTypes ?? []).map((rt) => ({
      id: rt.id,
      name: rt.name ?? rt.id,
      baseRate: rt.baseRate ?? 0,
    })),
  }));

  return (
    <main className="mx-auto max-w-xl px-4 py-12 sm:px-6">
      <h1 className="text-3xl font-bold tracking-tight">Book your stay</h1>
      <p className="mt-1 text-muted-foreground">
        Send a booking enquiry and our team will confirm availability.
      </p>
      <div className="mt-6">
        <BookingForm
          hotels={formHotels}
          initialHotel={hotel}
          initialRoomType={roomType}
        />
      </div>
    </main>
  );
}
