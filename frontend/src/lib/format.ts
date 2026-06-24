export function money(value?: number | null, currency = "USD"): string {
  if (value == null) return "—";
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency,
    maximumFractionDigits: 0,
  }).format(value);
}

export function money2(value?: number | null, currency = "USD"): string {
  if (value == null) return "—";
  return new Intl.NumberFormat(undefined, { style: "currency", currency }).format(value);
}

export function formatDate(value?: string | null): string {
  if (!value) return "—";
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? value : d.toLocaleDateString();
}

export function percent(value?: number | null): string {
  if (value == null) return "—";
  const n = value <= 1 ? value * 100 : value;
  return `${n.toFixed(1)}%`;
}

export function number(value?: number | null): string {
  if (value == null) return "—";
  return new Intl.NumberFormat().format(value);
}

export function initials(name?: string): string {
  if (!name) return "U";
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase())
    .join("");
}
