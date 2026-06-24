// API DTOs. Kept permissive (optional fields) so the UI tolerates contract drift; the backend's
// StayFlow.ContractTests pin the authoritative shape.

export interface Paged<T> {
  items: T[];
  total?: number;
  page?: number;
  pageSize?: number;
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
  guestName?: string;
  roomId?: string;
  roomNumber?: string;
  status?: ReservationStatus | string;
  checkIn?: string;
  checkOut?: string;
  numberOfGuests?: number;
  total?: number;
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
  status?: string;
  floor?: number;
}

export interface RoomType {
  id: string;
  name?: string;
  baseRate?: number;
  maxOccupancy?: number;
  description?: string;
}

export interface Guest {
  id: string;
  fullName?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  documentId?: string;
}

export interface Invoice {
  id: string;
  number?: string;
  reservationId?: string;
  status?: string;
  issuedOn?: string;
  total?: number;
  tax?: number;
}

export interface ServiceItem {
  id: string;
  name?: string;
  price?: number;
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
