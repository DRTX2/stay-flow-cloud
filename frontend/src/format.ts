export function money(value?: number): string {
  if (value == null) return "—";
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency: "USD",
  }).format(value);
}

export function date(value?: string): string {
  if (!value) return "—";
  const d = new Date(value);
  return Number.isNaN(d.getTime()) ? value : d.toLocaleDateString();
}

export function percent(value?: number): string {
  if (value == null) return "—";
  // Accept either 0..1 or 0..100.
  const n = value <= 1 ? value * 100 : value;
  return `${n.toFixed(1)}%`;
}
