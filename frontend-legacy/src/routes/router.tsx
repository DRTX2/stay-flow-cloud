import { lazy } from "react";
import { createBrowserRouter } from "react-router-dom";
import { AppShell } from "@/components/layout/AppShell";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { RouteError } from "@/components/shared/RouteError";
import { LoginPage } from "@/pages/LoginPage";
import { CallbackPage } from "@/pages/CallbackPage";
import { NotFoundPage } from "@/pages/NotFoundPage";

// Route-level code splitting: each screen is its own chunk, loaded on demand
// behind the AppShell's <Suspense> fallback.
const DashboardPage = lazy(() =>
  import("@/features/dashboard/DashboardPage").then((m) => ({
    default: m.DashboardPage,
  })),
);
const ReservationsPage = lazy(() =>
  import("@/features/reservations/ReservationsPage").then((m) => ({
    default: m.ReservationsPage,
  })),
);
const RoomsPage = lazy(() =>
  import("@/features/rooms/RoomsPage").then((m) => ({ default: m.RoomsPage })),
);
const RoomTypesPage = lazy(() =>
  import("@/features/room-types/RoomTypesPage").then((m) => ({
    default: m.RoomTypesPage,
  })),
);
const GuestsPage = lazy(() =>
  import("@/features/guests/GuestsPage").then((m) => ({ default: m.GuestsPage })),
);
const ServicesPage = lazy(() =>
  import("@/features/services/ServicesPage").then((m) => ({
    default: m.ServicesPage,
  })),
);
const InvoicesPage = lazy(() =>
  import("@/features/invoices/InvoicesPage").then((m) => ({
    default: m.InvoicesPage,
  })),
);
const DocumentsPage = lazy(() =>
  import("@/features/documents/DocumentsPage").then((m) => ({
    default: m.DocumentsPage,
  })),
);
const TenantFeaturesPage = lazy(() =>
  import("@/features/tenant-features/TenantFeaturesPage").then((m) => ({
    default: m.TenantFeaturesPage,
  })),
);
const AuditPage = lazy(() =>
  import("@/features/audit/AuditPage").then((m) => ({ default: m.AuditPage })),
);

export const router = createBrowserRouter([
  { path: "/login", element: <LoginPage /> },
  { path: "/callback", element: <CallbackPage /> },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <AppShell />
      </ProtectedRoute>
    ),
    errorElement: <RouteError />,
    children: [
      { index: true, element: <DashboardPage /> },
      { path: "reservations", element: <ReservationsPage /> },
      { path: "rooms", element: <RoomsPage /> },
      { path: "room-types", element: <RoomTypesPage /> },
      { path: "guests", element: <GuestsPage /> },
      { path: "services", element: <ServicesPage /> },
      { path: "invoices", element: <InvoicesPage /> },
      { path: "documents", element: <DocumentsPage /> },
      { path: "tenant-features", element: <TenantFeaturesPage /> },
      { path: "audit", element: <AuditPage /> },
    ],
  },
  { path: "*", element: <NotFoundPage /> },
]);
