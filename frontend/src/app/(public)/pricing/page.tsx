import type { Metadata } from "next";
import Link from "next/link";
import { Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { cn } from "@/lib/utils";

export const metadata: Metadata = {
  title: "Pricing",
  description:
    "Simple, transparent pricing for StayFlow Cloud — from independent properties to hotel groups.",
};

export const dynamic = "force-static";

const PLANS = [
  {
    name: "Basic",
    price: "$0",
    cadence: "/ month",
    blurb: "For a single property getting started.",
    features: [
      "1 property",
      "Reservations & front desk",
      "Up to 50 rooms",
      "Email support",
    ],
    cta: "Start free",
    featured: false,
  },
  {
    name: "Pro",
    price: "$149",
    cadence: "/ month",
    blurb: "For growing properties that need analytics and billing.",
    features: [
      "Up to 5 properties",
      "Billing & invoicing",
      "Executive analytics",
      "Document storage",
      "Priority support",
    ],
    cta: "Choose Pro",
    featured: true,
  },
  {
    name: "Enterprise",
    price: "Custom",
    cadence: "",
    blurb: "For hotel groups with advanced needs.",
    features: [
      "Unlimited properties",
      "SSO & advanced RBAC",
      "Audit & compliance",
      "Custom integrations",
      "Dedicated success manager",
    ],
    cta: "Contact sales",
    featured: false,
  },
];

export default function PricingPage() {
  return (
    <main
      id="main-content"
      tabIndex={-1}
      className="mx-auto max-w-7xl px-4 py-20 sm:px-6"
    >
      <div className="mx-auto max-w-2xl text-center">
        <h1 className="text-4xl font-bold tracking-tight">
          Pricing that scales with you
        </h1>
        <p className="mt-3 text-lg text-muted-foreground">
          Start free, upgrade as you grow. No hidden fees.
        </p>
      </div>

      <div className="mt-12 grid gap-6 lg:grid-cols-3">
        {PLANS.map((plan) => (
          <Card
            key={plan.name}
            className={cn(
              plan.featured && "border-primary shadow-md ring-1 ring-primary",
            )}
          >
            <CardHeader>
              {plan.featured && (
                <span className="mb-2 w-fit rounded-full bg-primary px-2.5 py-0.5 text-xs font-medium text-primary-foreground">
                  Most popular
                </span>
              )}
              <h2 className="text-lg font-semibold">{plan.name}</h2>
              <div className="mt-2 flex items-baseline gap-1">
                <span className="text-3xl font-bold tracking-tight">{plan.price}</span>
                <span className="text-sm text-muted-foreground">{plan.cadence}</span>
              </div>
              <p className="mt-2 text-sm text-muted-foreground">{plan.blurb}</p>
            </CardHeader>
            <CardContent className="space-y-4">
              <ul className="space-y-2 text-sm">
                {plan.features.map((feature) => (
                  <li key={feature} className="flex items-center gap-2">
                    <Check className="h-4 w-4 text-success" />
                    {feature}
                  </li>
                ))}
              </ul>
              <Button
                asChild
                className="w-full"
                variant={plan.featured ? "default" : "outline"}
              >
                <Link href="/login">{plan.cta}</Link>
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </main>
  );
}
