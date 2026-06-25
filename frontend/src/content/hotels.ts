import type { PublicHotelDetail } from "@/types/api";

/**
 * Curated public hotel catalog. In a real product this would come from a CMS or a public catalog
 * service; modelling it as an async source lets the public pages demonstrate SSG + ISR
 * (generateStaticParams + revalidate) exactly as they would against a remote backend.
 */
const HOTELS: PublicHotelDetail[] = [
  {
    slug: "aurora-grand-barcelona",
    name: "Aurora Grand Barcelona",
    city: "Barcelona",
    country: "Spain",
    rating: 4.8,
    fromRate: 180,
    description:
      "A Mediterranean landmark steps from the beach, blending Catalan modernism with " +
      "calm, contemporary rooms and a rooftop infinity pool.",
    heroImageUrl:
      "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1600&q=80",
    amenities: ["Rooftop pool", "Spa & wellness", "Sea view", "Free Wi-Fi", "Restaurant"],
    roomTypes: [
      {
        id: "rt-bcn-standard",
        name: "Standard Double",
        description: "Bright 24m² room with a queen bed and city view.",
        baseRate: 180,
        maxOccupancy: 2,
        imageUrl:
          "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?auto=format&fit=crop&w=1200&q=80",
      },
      {
        id: "rt-bcn-deluxe",
        name: "Deluxe Sea View",
        description: "Spacious 34m² room with balcony and Mediterranean views.",
        baseRate: 260,
        maxOccupancy: 3,
        imageUrl:
          "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?auto=format&fit=crop&w=1200&q=80",
      },
      {
        id: "rt-bcn-suite",
        name: "Executive Suite",
        description: "Separate lounge, premium bath amenities, and lounge access.",
        baseRate: 420,
        maxOccupancy: 4,
        imageUrl:
          "https://images.unsplash.com/photo-1591088398332-8a7791972843?auto=format&fit=crop&w=1200&q=80",
      },
    ],
  },
  {
    slug: "nordic-fjord-lodge",
    name: "Nordic Fjord Lodge",
    city: "Bergen",
    country: "Norway",
    rating: 4.7,
    fromRate: 210,
    description:
      "A design-forward lodge on the water's edge, with floor-to-ceiling windows framing " +
      "the fjord and a wood-fired sauna at the dock.",
    heroImageUrl:
      "https://images.unsplash.com/photo-1601918774946-25832a4be0d6?auto=format&fit=crop&w=1600&q=80",
    amenities: [
      "Fjord view",
      "Sauna",
      "Breakfast included",
      "EV charging",
      "Pet friendly",
    ],
    roomTypes: [
      {
        id: "rt-brg-cabin",
        name: "Waterfront Cabin",
        description: "Cosy cabin with a private deck over the water.",
        baseRate: 210,
        maxOccupancy: 2,
        imageUrl:
          "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=1200&q=80",
      },
      {
        id: "rt-brg-family",
        name: "Family Loft",
        description: "Two-level loft sleeping up to four with a kitchenette.",
        baseRate: 340,
        maxOccupancy: 4,
        imageUrl:
          "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=80",
      },
    ],
  },
  {
    slug: "saffron-palace-marrakech",
    name: "Saffron Palace Marrakech",
    city: "Marrakech",
    country: "Morocco",
    rating: 4.9,
    fromRate: 150,
    description:
      "A restored riad in the heart of the medina with a courtyard garden, plunge pool, " +
      "and rooftop dining under the stars.",
    heroImageUrl:
      "https://images.unsplash.com/photo-1539020140153-e479b8c22e70?auto=format&fit=crop&w=1600&q=80",
    amenities: [
      "Courtyard pool",
      "Rooftop terrace",
      "Hammam",
      "Airport transfer",
      "Concierge",
    ],
    roomTypes: [
      {
        id: "rt-rak-classic",
        name: "Classic Riad Room",
        description: "Hand-crafted interiors opening onto the courtyard.",
        baseRate: 150,
        maxOccupancy: 2,
        imageUrl:
          "https://images.unsplash.com/photo-1596436889106-be35e843f974?auto=format&fit=crop&w=1200&q=80",
      },
      {
        id: "rt-rak-suite",
        name: "Garden Suite",
        description: "Private terrace, soaking tub, and garden views.",
        baseRate: 300,
        maxOccupancy: 3,
        imageUrl:
          "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?auto=format&fit=crop&w=1200&q=80",
      },
    ],
  },
  {
    slug: "harbor-view-singapore",
    name: "Harbor View Singapore",
    city: "Singapore",
    country: "Singapore",
    rating: 4.6,
    fromRate: 240,
    description:
      "A sleek business hotel on the marina with a sky bar, 24/7 fitness, and effortless " +
      "access to the financial district.",
    heroImageUrl:
      "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1600&q=80",
    amenities: ["Sky bar", "Infinity pool", "24/7 gym", "Business lounge", "Free Wi-Fi"],
    roomTypes: [
      {
        id: "rt-sin-city",
        name: "City Room",
        description: "Modern room with skyline views and a work desk.",
        baseRate: 240,
        maxOccupancy: 2,
        imageUrl:
          "https://images.unsplash.com/photo-1611892440504-42a792e24d32?auto=format&fit=crop&w=1200&q=80",
      },
      {
        id: "rt-sin-marina",
        name: "Marina Suite",
        description: "Corner suite overlooking the marina with lounge access.",
        baseRate: 460,
        maxOccupancy: 3,
        imageUrl:
          "https://images.unsplash.com/photo-1618773928121-c32242e63f39?auto=format&fit=crop&w=1200&q=80",
      },
    ],
  },
];

/** Simulates an async catalog fetch (CMS/public service). */
export async function getHotels(): Promise<PublicHotelDetail[]> {
  return HOTELS;
}

export async function getHotelBySlug(slug: string): Promise<PublicHotelDetail | null> {
  return HOTELS.find((h) => h.slug === slug) ?? null;
}

export async function getHotelSlugs(): Promise<string[]> {
  return HOTELS.map((h) => h.slug);
}
