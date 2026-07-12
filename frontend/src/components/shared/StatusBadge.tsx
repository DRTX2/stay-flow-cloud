import { Badge, type BadgeProps } from "@/components/ui/badge";

const MAP: Record<string, BadgeProps["variant"]> = {
  confirmed: "default",
  checkedin: "default",
  checkedout: "secondary",
  pending: "warning",
  draft: "warning",
  issued: "default",
  paid: "success",
  cancelled: "destructive",
  rejected: "destructive",
  converted: "success",
  active: "success",
  inactive: "secondary",
  available: "success",
  occupied: "warning",
  outofservice: "destructive",
};

export function StatusBadge({ status }: { status?: string }) {
  if (!status) return <span className="text-muted-foreground">—</span>;
  const variant = MAP[status.toLowerCase().replace(/[\s_-]/g, "")] ?? "outline";
  return <Badge variant={variant}>{status}</Badge>;
}
