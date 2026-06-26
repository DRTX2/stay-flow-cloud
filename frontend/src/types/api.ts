// API DTOs. Kept permissive (optional fields) so the UI tolerates contract drift; the backend's
// StayFlow.ContractTests pin the authoritative shape.

export interface Paged<T> {
  items: T[];
  total?: number;
  page?: number;
  pageSize?: number;
}

/** Normalized paged envelope matching the backend `PagedResult<T>`. */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface DashboardSummary {
  totalReservations?: number;
  occupancyRate?: number;
  revenue?: number;
  adr?: number;
  revPar?: number;
  availableRooms?: number;
  totalGuests?: number;
  arrivalsToday?: number;
  departuresToday?: number;
}

export interface RevenuePoint {
  date?: string;
  period?: string;
  amount?: number;
  revenue?: number;
}

export type ReservationStatus =
  | "Pending"
  | "Confirmed"
  | "CheckedIn"
  | "CheckedOut"
  | "Cancelled";

export interface Reservation {
  id: string;
  guestId?: string;
  // Resolved on the server from the guest/room lists (the API list returns only ids).
  guestName?: string;
  roomId?: string;
  roomNumber?: string;
  status?: ReservationStatus | string;
  checkIn?: string;
  checkOut?: string;
  numberOfGuests?: number;
  totalPrice?: number;
  total?: number;
  confirmationCode?: string;
  nights?: number;
}

export interface CreateReservationRequest {
  guestId: string;
  roomId: string;
  checkIn: string;
  checkOut: string;
  numberOfGuests: number;
}

export interface Room {
  id: string;
  number?: string;
  roomTypeId?: string;
  roomTypeName?: string;
  basePrice?: number;
  capacity?: number;
  status?: string;
  floor?: number;
}

export interface CreateRoomRequest {
  number: string;
  roomTypeId: string;
  basePrice: number;
  capacity: number;
  floor: number;
}

export interface RoomType {
  id: string;
  name?: string;
  baseRate?: number;
  maxOccupancy?: number;
  description?: string;
}

export interface CreateRoomTypeRequest {
  name: string;
  baseRate: number;
  maxOccupancy: number;
  description?: string;
}

export interface Guest {
  id: string;
  fullName?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  documentNumber?: string;
}

export interface CreateGuestRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  documentNumber?: string;
}

export interface Invoice {
  id: string;
  number?: string;
  reservationId?: string;
  status?: string;
  currency?: string;
  issuedAtUtc?: string;
  dueAtUtc?: string;
  paidAtUtc?: string;
  subtotal?: number;
  taxTotal?: number;
  total?: number;
}

// Matches the backend ServiceCategory enum (now serialized as its string name).
export const SERVICE_CATEGORIES = [
  "FoodAndBeverage",
  "Spa",
  "Transport",
  "Laundry",
  "Excursion",
  "Other",
] as const;

export type ServiceCategory = (typeof SERVICE_CATEGORIES)[number];

export interface ServiceItem {
  id: string;
  name?: string;
  price?: number;
  category?: ServiceCategory | string;
  isActive?: boolean;
  description?: string;
}

export interface CreateServiceRequest {
  name: string;
  price: number;
  category: ServiceCategory;
  description?: string;
}

export interface AuditEntry {
  id?: string;
  event?: string;
  entityType?: string;
  userId?: string;
  timestamp?: string;
}

export interface TenantFeature {
  key?: string;
  name?: string;
  enabled?: boolean;
}

export interface DocumentItem {
  key: string;
  name?: string;
  size?: number;
  contentType?: string;
  uploadedOn?: string;
}

// --- Public (anonymous) booking surface --------------------------------------

export interface PublicHotel {
  slug: string;
  name?: string;
  city?: string;
  country?: string;
  description?: string;
  heroImageUrl?: string;
  rating?: number;
  fromRate?: number;
  amenities?: string[];
}

export interface PublicRoomType {
  id: string;
  name?: string;
  description?: string;
  baseRate?: number;
  maxOccupancy?: number;
  imageUrl?: string;
}

export interface PublicHotelDetail extends PublicHotel {
  roomTypes?: PublicRoomType[];
}

export interface BookingRequest {
  hotelSlug: string;
  roomTypeId: string;
  checkIn: string;
  checkOut: string;
  guests: number;
  fullName: string;
  email: string;
  phone?: string;
}
