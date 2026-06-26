"use client";

import { useState, type ReactNode } from "react";
import {
  type ColumnDef,
  type ColumnFiltersState,
  type SortingState,
  type VisibilityState,
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { Download, Search } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { downloadFile } from "@/lib/csv";
import { DataTablePagination } from "./DataTablePagination";
import { DataTableViewOptions } from "./DataTableViewOptions";
import { ServerPagination } from "./ServerPagination";
import { ServerSearchInput } from "./ServerSearchInput";

/** When provided, the table delegates paging (and optionally search) to the server via the URL. */
export interface ServerTableConfig {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  search?: { value: string; placeholder?: string };
}

interface DataTableProps<TData, TValue> {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
  isLoading?: boolean;
  searchPlaceholder?: string;
  /** Enables a CSV export of the currently filtered rows. */
  exportFileName?: string;
  toolbar?: ReactNode;
  emptyState?: ReactNode;
  pageSize?: number;
  /** Switches the table to server-side pagination/search driven by URL params. */
  serverPagination?: ServerTableConfig;
}

export function DataTable<TData, TValue>({
  columns,
  data,
  isLoading,
  searchPlaceholder = "Search…",
  exportFileName,
  toolbar,
  emptyState,
  pageSize = 10,
  serverPagination,
}: DataTableProps<TData, TValue>) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>({});
  const [globalFilter, setGlobalFilter] = useState("");

  const server = serverPagination;

  // TanStack Table returns non-memoizable functions; the React Compiler auto-skips this component,
  // so silence the advisory rule here.
  // eslint-disable-next-line react-hooks/incompatible-library
  const table = useReactTable({
    data,
    columns,
    state: { sorting, columnFilters, columnVisibility, globalFilter },
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: setColumnVisibility,
    onGlobalFilterChange: setGlobalFilter,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getSortedRowModel: getSortedRowModel(),
    // In server mode the data is already a single page; skip the client paginator.
    ...(server
      ? {
          manualPagination: true,
          manualFiltering: true,
          pageCount: Math.max(server.totalPages, 1),
        }
      : { getPaginationRowModel: getPaginationRowModel() }),
    initialState: { pagination: { pageSize: server ? server.pageSize : pageSize } },
  });

  function exportCsv() {
    const cols = table.getVisibleLeafColumns().filter((c) => c.accessorFn !== undefined);
    const header = cols.map((c) => c.id);
    const escape = (v: unknown) => {
      const s = v == null ? "" : String(v);
      return /[",\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
    };
    const lines = table
      .getFilteredRowModel()
      .rows.map((row) => cols.map((c) => escape(row.getValue(c.id))).join(","));
    const csv = [header.join(","), ...lines].join("\n");
    downloadFile(exportFileName ?? "export.csv", csv);
  }

  const colCount = table.getAllColumns().length;

  return (
    <div className="space-y-3">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
        {server ? (
          server.search ? (
            <ServerSearchInput
              initialValue={server.search.value}
              placeholder={server.search.placeholder ?? searchPlaceholder}
            />
          ) : (
            <div className="w-full sm:max-w-xs" />
          )
        ) : (
          <div className="relative w-full sm:max-w-xs">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              value={globalFilter}
              onChange={(e) => setGlobalFilter(e.target.value)}
              placeholder={searchPlaceholder}
              aria-label="Search table"
              className="pl-8"
            />
          </div>
        )}
        <div className="flex items-center gap-2 sm:ml-auto">
          {toolbar}
          {exportFileName && (
            <Button
              variant="outline"
              size="sm"
              className="h-8"
              onClick={exportCsv}
              disabled={isLoading || table.getFilteredRowModel().rows.length === 0}
            >
              <Download className="mr-2 h-4 w-4" />
              CSV
            </Button>
          )}
          <DataTableViewOptions table={table} />
        </div>
      </div>

      <div className="rounded-xl border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id} className="hover:bg-transparent">
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(header.column.columnDef.header, header.getContext())}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <TableRow key={i}>
                  {Array.from({ length: colCount }).map((__, j) => (
                    <TableCell key={j}>
                      <Skeleton className="h-5 w-full" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : table.getRowModel().rows.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id} data-state={row.getIsSelected() && "selected"}>
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow className="hover:bg-transparent">
                <TableCell colSpan={colCount} className="h-48 p-0">
                  {emptyState ?? (
                    <div className="flex h-48 items-center justify-center text-sm text-muted-foreground">
                      No results.
                    </div>
                  )}
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      {server ? (
        <ServerPagination
          page={server.page}
          pageSize={server.pageSize}
          totalCount={server.totalCount}
          totalPages={server.totalPages}
        />
      ) : (
        <DataTablePagination table={table} />
      )}
    </div>
  );
}
