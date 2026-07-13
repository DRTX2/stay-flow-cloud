"use client";

import { useState } from "react";
import Link from "next/link";
import { Bell, CheckCheck } from "lucide-react";
import {
  QueryClient,
  QueryClientProvider,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import type { NotificationList } from "@/types/api";

const queryKey = ["notifications"] as const;

async function getNotifications(): Promise<NotificationList> {
  const response = await fetch("/api/notifications", { cache: "no-store" });
  if (!response.ok) throw new Error("Could not load notifications");
  return (await response.json()) as NotificationList;
}

async function post(path: string): Promise<void> {
  const response = await fetch(path, { method: "POST" });
  if (!response.ok) throw new Error("Could not update notifications");
}

function BellContent() {
  const client = useQueryClient();
  const notifications = useQuery({
    queryKey,
    queryFn: getNotifications,
    refetchInterval: 45_000,
  });
  const markRead = useMutation({
    mutationFn: (id: string) => post(`/api/notifications/${encodeURIComponent(id)}/read`),
    onSuccess: () => client.invalidateQueries({ queryKey }),
  });
  const markAllRead = useMutation({
    mutationFn: () => post("/api/notifications/read-all"),
    onSuccess: () => client.invalidateQueries({ queryKey }),
  });
  const unreadCount = notifications.data?.unreadCount ?? 0;

  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          className="relative"
          aria-label={`Notifications${unreadCount ? `, ${unreadCount} unread` : ""}`}
        >
          <Bell className="h-4 w-4" />
          {unreadCount > 0 && (
            <span className="absolute right-0.5 top-0.5 min-w-4 rounded-full bg-primary px-1 text-center text-[10px] font-semibold leading-4 text-primary-foreground">
              {unreadCount > 99 ? "99+" : unreadCount}
            </span>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent align="end" className="w-[min(24rem,calc(100vw-1rem))] p-0">
        <div className="flex items-center justify-between border-b px-4 py-3">
          <div>
            <p className="text-sm font-semibold">Notifications</p>
            <p className="text-xs text-muted-foreground">{unreadCount} unread</p>
          </div>
          {unreadCount > 0 && (
            <Button
              variant="ghost"
              size="sm"
              className="h-8 gap-1.5 text-xs"
              disabled={markAllRead.isPending}
              onClick={() => markAllRead.mutate()}
            >
              <CheckCheck className="h-3.5 w-3.5" />
              Mark all read
            </Button>
          )}
        </div>
        <div className="max-h-[min(28rem,70vh)] overflow-y-auto">
          {notifications.isPending && (
            <p className="p-6 text-center text-sm text-muted-foreground">
              Loading notifications...
            </p>
          )}
          {notifications.isError && (
            <p className="p-6 text-center text-sm text-destructive">
              Notifications are unavailable.
            </p>
          )}
          {notifications.data?.items.length === 0 && (
            <p className="p-6 text-center text-sm text-muted-foreground">
              You are all caught up.
            </p>
          )}
          {notifications.data?.items.map((notification) => {
            const content = (
              <div className="min-w-0 flex-1">
                <div className="flex items-start justify-between gap-3">
                  <p className="text-sm font-medium leading-5">{notification.title}</p>
                  <time
                    className="shrink-0 text-[11px] text-muted-foreground"
                    dateTime={notification.createdAtUtc}
                  >
                    {new Date(notification.createdAtUtc).toLocaleDateString(undefined, {
                      month: "short",
                      day: "numeric",
                    })}
                  </time>
                </div>
                <p className="mt-0.5 text-xs leading-5 text-muted-foreground">
                  {notification.body}
                </p>
              </div>
            );
            const className = cn(
              "flex border-b px-4 py-3 text-left transition-colors hover:bg-muted/60",
              !notification.readAtUtc && "bg-primary/5",
            );
            return notification.link ? (
              <Link
                key={notification.id}
                href={notification.link}
                className={className}
                onClick={() =>
                  !notification.readAtUtc && markRead.mutate(notification.id)
                }
              >
                {content}
              </Link>
            ) : (
              <button
                key={notification.id}
                type="button"
                className={cn(className, "w-full")}
                onClick={() =>
                  !notification.readAtUtc && markRead.mutate(notification.id)
                }
              >
                {content}
              </button>
            );
          })}
        </div>
      </PopoverContent>
    </Popover>
  );
}

export function NotificationBell() {
  const [queryClient] = useState(() => new QueryClient());
  return (
    <QueryClientProvider client={queryClient}>
      <BellContent />
    </QueryClientProvider>
  );
}
