import { Metadata } from "next";
import { Utensils } from "lucide-react";

export const metadata: Metadata = {
  title: "Orders & F&B | StayFlow",
};

export default function OrdersPage() {
  return (
    <div className="flex-1 space-y-4 p-8 pt-6">
      <div className="flex items-center justify-between space-y-2">
        <h2 className="text-3xl font-bold tracking-tight">Orders (F&B)</h2>
        <div className="flex items-center space-x-2">
          {/* Place Order button could go here */}
        </div>
      </div>

      <div className="flex flex-col items-center justify-center rounded-md border border-dashed p-8 text-center animate-in fade-in-50">
        <Utensils className="mb-4 h-10 w-10 text-muted-foreground" />
        <h3 className="text-lg font-medium">Room Service & Orders</h3>
        <p className="mt-1 max-w-sm text-sm text-muted-foreground">
          Manage food, beverage, and amenity orders charged directly to guest
          reservations. Uses <strong>/api/v1/orders</strong>.
        </p>
      </div>
    </div>
  );
}
