import { getJson } from "@/server/api";
import type { PublicHotelDetail } from "@/types/api";

interface CatalogHotelResponse {
  slug: string;
  name: string;
  propertyType: string;
  currency: string;
  roomTypes: Array<{
    id: string;
    name: string;
    description?: string;
    baseRate: number;
    maxOccupancy: number;
  }>;
}

const PROPERTY_IMAGES: Record<string, string> = {
  Hotel:
    "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1600&q=80",
  Resort:
    "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1600&q=80",
  Hostel:
    "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa?auto=format&fit=crop&w=1600&q=80",
};

function mapHotel(hotel: CatalogHotelResponse): PublicHotelDetail {
  return {
    slug: hotel.slug,
    name: hotel.name,
    propertyType: hotel.propertyType,
    currency: hotel.currency,
    description: `${hotel.propertyType} managed with StayFlow Cloud.`,
    heroImageUrl: PROPERTY_IMAGES[hotel.propertyType] ?? PROPERTY_IMAGES.Hotel,
    fromRate:
      hotel.roomTypes.length > 0
        ? Math.min(...hotel.roomTypes.map((roomType) => roomType.baseRate))
        : undefined,
    roomTypes: hotel.roomTypes.map((roomType) => ({
      ...roomType,
      imageUrl: PROPERTY_IMAGES[hotel.propertyType] ?? PROPERTY_IMAGES.Hotel,
    })),
  };
}

/** The operational backend is the single source of truth for public sellable inventory. */
export async function getHotels(): Promise<PublicHotelDetail[]> {
  const hotels = await getJson<CatalogHotelResponse[]>("/api/v1/public/hotels", {
    auth: false,
  });
  return hotels.map(mapHotel);
}

export async function getHotelBySlug(slug: string): Promise<PublicHotelDetail | null> {
  try {
    return mapHotel(
      await getJson<CatalogHotelResponse>(
        `/api/v1/public/hotels/${encodeURIComponent(slug)}`,
        { auth: false },
      ),
    );
  } catch {
    return null;
  }
}

export async function getHotelSlugs(): Promise<string[]> {
  return (await getHotels()).map((hotel) => hotel.slug);
}
