import type { Metadata } from "next";
import { PageHeader } from "@/components/shared/PageHeader";
import { OrdersView } from "@/features/orders/OrdersView";
import { getList, getPaged } from "@/server/api";
import type { Order, Reservation, ServiceItem } from "@/types/api";

export const metadata: Metadata = { title: "Orders & F&B" };

export default async function OrdersPage() {
  const [orders, reservations, services] = await Promise.all([
    getPaged<Order>("/api/v1/orders", { page: 1, pageSize: 100 }),
    getList<Reservation>("/api/v1/reservations?status=CheckedIn&pageSize=100"),
    getList<ServiceItem>("/api/v1/services?pageSize=200"),
  ]);
  return (
    <div className="space-y-6">
      <PageHeader
        title="Orders & F&B"
        description="Place and fulfil food, beverage and amenity requests charged to active stays."
      />
      <OrdersView orders={orders.items} reservations={reservations} services={services} />
    </div>
  );
}
