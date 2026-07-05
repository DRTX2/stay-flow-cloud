import {
  CheckCircle2,
  Cloud,
  Database,
  KeyRound,
  type LucideIcon,
  MessageSquare,
  MonitorSmartphone,
  Shield,
  XCircle,
} from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

interface IntegrationItem {
  name: string;
  description: string;
  icon: LucideIcon;
  status: "active" | "available" | "disabled";
  category: string;
}

const integrations: IntegrationItem[] = [
  // Authentication providers
  {
    name: "Google OAuth",
    description: "Allow users to sign in with their Google accounts via OpenID Connect.",
    icon: Shield,
    status: "available",
    category: "Authentication",
  },
  {
    name: "Microsoft OAuth",
    description: "Enterprise sign-in through Microsoft / Azure AD accounts.",
    icon: Shield,
    status: "available",
    category: "Authentication",
  },
  {
    name: "GitHub OAuth",
    description: "Developer-friendly sign-in via GitHub accounts.",
    icon: Shield,
    status: "available",
    category: "Authentication",
  },
  // Storage & data
  {
    name: "AWS S3",
    description:
      "Document and file storage with tenant-scoped buckets and CloudFront CDN.",
    icon: Cloud,
    status: "active",
    category: "Storage",
  },
  {
    name: "PostgreSQL",
    description: "Primary relational database for all operational data (EF Core).",
    icon: Database,
    status: "active",
    category: "Data",
  },
  {
    name: "MongoDB",
    description: "Audit log and event store for immutable domain event trails.",
    icon: Database,
    status: "active",
    category: "Data",
  },
  {
    name: "Redis",
    description: "Distributed cache, rate limiting, and session storage.",
    icon: Database,
    status: "active",
    category: "Data",
  },
  // Messaging
  {
    name: "MassTransit",
    description: "Message bus for domain events with transactional outbox pattern.",
    icon: MessageSquare,
    status: "active",
    category: "Messaging",
  },
  {
    name: "RabbitMQ",
    description: "Message broker transport for inter-service communication.",
    icon: MessageSquare,
    status: "available",
    category: "Messaging",
  },
  // Monitoring
  {
    name: "OpenTelemetry",
    description: "Metrics, traces, and instrumentation for ASP.NET Core and EF Core.",
    icon: MonitorSmartphone,
    status: "active",
    category: "Observability",
  },
  {
    name: "Prometheus + Grafana",
    description:
      "Metrics scraping and pre-built dashboards for API performance monitoring.",
    icon: MonitorSmartphone,
    status: "active",
    category: "Observability",
  },
  // API
  {
    name: "REST API (OAuth2)",
    description: "Full tenant-scoped API with OpenIddict OAuth2/OIDC server and PKCE.",
    icon: KeyRound,
    status: "active",
    category: "API",
  },
];

function statusColor(status: IntegrationItem["status"]) {
  switch (status) {
    case "active":
      return "success" as const;
    case "available":
      return "secondary" as const;
    case "disabled":
      return "destructive" as const;
  }
}

function StatusIcon({ status }: { status: IntegrationItem["status"] }) {
  if (status === "active") return <CheckCircle2 className="h-4 w-4 text-emerald-500" />;
  if (status === "disabled") return <XCircle className="h-4 w-4 text-destructive" />;
  return <CheckCircle2 className="h-4 w-4 text-muted-foreground" />;
}

export function IntegrationsView() {
  const categories = Array.from(new Set(integrations.map((i) => i.category)));

  return (
    <div className="space-y-8">
      {categories.map((category) => (
        <div key={category} className="space-y-4">
          <h2 className="text-lg font-semibold tracking-tight">{category}</h2>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {integrations
              .filter((i) => i.category === category)
              .map((integration) => (
                <Card
                  key={integration.name}
                  className="transition-shadow hover:shadow-md"
                >
                  <CardHeader className="pb-3">
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex items-center gap-3">
                        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-muted">
                          <integration.icon className="h-4 w-4" />
                        </div>
                        <CardTitle className="text-sm">{integration.name}</CardTitle>
                      </div>
                      <div className="flex items-center gap-1.5">
                        <StatusIcon status={integration.status} />
                        <Badge
                          variant={statusColor(integration.status)}
                          className="text-xs"
                        >
                          {integration.status === "active"
                            ? "Active"
                            : integration.status === "available"
                              ? "Available"
                              : "Disabled"}
                        </Badge>
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <CardDescription>{integration.description}</CardDescription>
                  </CardContent>
                </Card>
              ))}
          </div>
        </div>
      ))}

      {/* API documentation link */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm">API documentation</CardTitle>
        </CardHeader>
        <CardContent>
          <CardDescription>
            The full API reference is available at{" "}
            <a
              href="/api/v1"
              target="_blank"
              rel="noopener noreferrer"
              className="font-medium underline hover:text-foreground"
            >
              Swagger / OpenAPI
            </a>{" "}
            when the backend is running. All endpoints are tenant-scoped and require
            OAuth2 bearer tokens.
          </CardDescription>
        </CardContent>
      </Card>
    </div>
  );
}
