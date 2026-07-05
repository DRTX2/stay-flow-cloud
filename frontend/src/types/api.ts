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
  date?: string;
  totalRooms?: number;
  occupiedRooms?: number;
  totalReservations?: number;
  occupancyRate?: number;
  arrivalsToday?: number;
  departuresToday?: number;
  inHouse?: number;
  reservationsByStatus?: Record<string, number>;
  invoicesByStatus?: Record<string, number>;
  bookedRevenueLast30Days?: number;
  revenue?: number;
  adr?: number;
  revPar?: number;
  availableRooms?: number;
  totalGuests?: number;
}

export interface FrontDeskReservationItem {
  reservationId: string;
  confirmationCode?: string;
  guestId: string;
  guestName?: string;
  roomId: string;
  roomNumber?: string;
  status?: string;
  checkIn?: string;
  checkOut?: string;
  guests?: number;
}

export interface FrontDeskRoomIssue {
  roomId: string;
  roomNumber?: string;
  roomStatus?: string;
  cleaningStatus?: string;
  openHousekeepingTasks?: number;
  openMaintenanceWorkOrders?: number;
}

export interface FrontDeskToday {
  date?: string;
  arrivals?: number;
  departures?: number;
  inHouse?: number;
  dirtyRooms?: number;
  outOfServiceRooms?: number;
  pendingHousekeepingTasks?: number;
  openMaintenanceWorkOrders?: number;
  arrivalList?: FrontDeskReservationItem[];
  departureList?: FrontDeskReservationItem[];
  roomIssues?: FrontDeskRoomIssue[];
}

export interface RevenuePoint {
  date?: string;
  period?: string;
  amount?: number;
  revenue?: number;
  checkouts?: number;
}

export interface RevenueReport {
  from?: string;
  to?: string;
  total?: number;
  daily?: RevenuePoint[];
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
  cleaningStatus?: string;
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
  includedInPlan?: boolean;
  requiredPlan?: string;
}

export interface PlanLimits {
  maxRooms?: number;
  maxUsers?: number;
  maxServiceItems?: number;
}

export interface TenantFeaturesResponse {
  plan?: string;
  limits?: PlanLimits;
  features?: Record<string, boolean>;
  featureDetails?: TenantFeature[];
}

export interface StaffUser {
  id: string;
  fullName?: string;
  email?: string;
  isActive?: boolean;
  roles?: string[];
}

export interface StaffUsersResponse {
  assignableRoles?: string[];
  users?: StaffUser[];
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

// --- Operations Expansion ---------------------------------------------------

export interface HousekeepingTask {
  id: string;
  roomId: string;
  taskType?: string;
  status?: string;
  assignedToId?: string;
  notes?: string;
  createdAtUtc?: string;
  completedAtUtc?: string;
}

export interface WorkOrder {
  id: string;
  roomId?: string;
  description?: string;
  priority?: string;
  status?: string;
  reportedById?: string;
  assignedToId?: string;
  resolutionNotes?: string;
  createdAtUtc?: string;
  resolvedAtUtc?: string;
}

export interface RoomRackReservation {
  reservationId: string;
  confirmationCode?: string;
  guestId: string;
  guestName?: string;
  checkIn?: string;
  checkOut?: string;
  status?: string;
}

export interface RoomRackRoom {
  roomId: string;
  roomNumber?: string;
  roomTypeName?: string;
  roomStatus?: string;
  cleaningStatus?: string;
  reservations?: RoomRackReservation[];
}

export interface RoomRack {
  from?: string;
  to?: string;
  rooms?: RoomRackRoom[];
}

export interface SetupStep {
  key: string;
  label?: string;
  completed?: boolean;
  count?: number;
  nextHref?: string;
}

export interface SetupChecklist {
  completedSteps?: number;
  totalSteps?: number;
  percentComplete?: number;
  steps?: SetupStep[];
}

export interface GuestReservationHistory {
  id: string;
  roomId?: string;
  roomNumber?: string;
  checkIn?: string;
  checkOut?: string;
  status?: string;
  totalPrice?: number;
  confirmationCode?: string;
}

export interface GuestInvoiceSummary {
  id: string;
  reservationId?: string;
  number?: string;
  status?: string;
  total?: number;
  paidAtUtc?: string;
}

export interface GuestProfile {
  guest: Guest;
  totalStays?: number;
  lifetimeValue?: number;
  lastStay?: string;
  reservations?: GuestReservationHistory[];
  invoices?: GuestInvoiceSummary[];
}

export interface SampleStay {
  guestId: string;
  roomId: string;
  reservationId: string;
  serviceItemId: string;
  chargeId: string;
  invoiceId: string;
  confirmationCode?: string;
  invoiceNumber?: string;
  invoiceTotal?: number;
}

export interface OrderLineItem {
  serviceItemId: string;
  serviceName?: string;
  quantity: number;
  unitPrice: number;
  total: number;
}

export interface Order {
  id: string;
  reservationId: string;
  status?: string;
  notes?: string;
  totalAmount: number;
  createdAtUtc?: string;
  deliveredAtUtc?: string;
  items?: OrderLineItem[];
}
