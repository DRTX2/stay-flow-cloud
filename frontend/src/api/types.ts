// API DTOs. Kept intentionally permissive (optional fields) so the UI tolerates contract drift;
// the backend's StayFlow.ContractTests pin the authoritative shape.

export interface DashboardSummary {
  totalReservations?: number;
  occupancyRate?: number;
  revenue?: number;
  availableRooms?: number;
  arrivalsToday?: number;
  departuresToday?: number;
}

export interface RevenuePoint {
  date?: string;
  period?: string;
  amount?: number;
  revenue?: number;
}

export interface Reservation {
  id: string;
  guestName?: string;
  roomNumber?: string;
  status?: string;
  checkIn?: string;
  checkOut?: string;
  total?: number;
}

export interface Room {
  id: string;
  number?: string;
  roomTypeName?: string;
  status?: string;
  floor?: number;
}

export interface Guest {
  id: string;
  fullName?: string;
  email?: string;
  phone?: string;
  documentId?: string;
}

export interface ServiceItem {
  id: string;
  name?: string;
  price?: number;
  description?: string;
}
