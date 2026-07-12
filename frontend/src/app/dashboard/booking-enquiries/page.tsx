import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { BookingEnquiriesView } from "@/features/booking-enquiries/BookingEnquiriesView";
import { getList, getPaged } from "@/server/api";
import type { BookingEnquiry, Room } from "@/types/api";

export const metadata: Metadata = { title: "Booking Enquiries" };

export default async function BookingEnquiriesPage() {
  const [enquiries, rooms] = await Promise.all([
    getPaged<BookingEnquiry>("/api/v1/bookingenquiries", { page: 1, pageSize: 100 }),
    getList<Room>("/api/v1/rooms?pageSize=200"),
  ]);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Booking enquiries"
        description="Review public requests, assign an available room and create the operational reservation."
      />
      <BookingEnquiriesView enquiries={enquiries.items} rooms={rooms} />
    </div>
  );
}
