export type SearchParams = Record<string, string | string[] | undefined>;

export interface PageParams {
  page: number;
  pageSize: number;
  search?: string;
}

function first(value: string | string[] | undefined): string | undefined {
  return Array.isArray(value) ? value[0] : value;
}

/** Parse and clamp `page`/`pageSize`/`search` from a route's resolved searchParams. */
export function parsePageParams(sp: SearchParams, defaultPageSize = 20): PageParams {
  const page = Math.max(1, Number(first(sp.page)) || 1);
  const pageSize = Math.min(
    100,
    Math.max(1, Number(first(sp.pageSize)) || defaultPageSize),
  );
  const search = first(sp.search)?.trim() || undefined;
  return { page, pageSize, search };
}
