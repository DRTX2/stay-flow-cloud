import { http } from "./http";

/** The API returns either a bare array or a paged envelope ({ items: [...] }); normalize. */
export function unwrapList<T>(data: unknown): T[] {
  if (Array.isArray(data)) return data as T[];
  if (data && typeof data === "object" && "items" in data) {
    const items = (data as { items: unknown }).items;
    if (Array.isArray(items)) return items as T[];
  }
  return [];
}

export async function getList<T>(url: string): Promise<T[]> {
  const { data } = await http.get(url);
  return unwrapList<T>(data);
}
