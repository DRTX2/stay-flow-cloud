"use client";

import { useState, useTransition } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { CheckCircle2, Loader2 } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { money } from "@/lib/format";
import { createBookingAction } from "@/app/(public)/book/actions";

export interface BookingHotel {
  slug: string;
  name: string;
  roomTypes: { id: string; name: string; baseRate: number }[];
}

const schema = z
  .object({
    hotelSlug: z.string().min(1, "Select a hotel"),
    roomTypeId: z.string().min(1, "Select a room"),
    checkIn: z.string().min(1, "Required"),
    checkOut: z.string().min(1, "Required"),
    guests: z.coerce.number().int().min(1, "At least 1").max(20),
    fullName: z.string().min(2, "Enter your name"),
    email: z.string().email("Enter a valid email"),
    phone: z.string().optional(),
  })
  .refine((v) => v.checkOut > v.checkIn, {
    message: "Check-out must be after check-in",
    path: ["checkOut"],
  });

type FormValues = z.infer<typeof schema>;

export function BookingForm({
  hotels,
  initialHotel,
  initialRoomType,
}: {
  hotels: BookingHotel[];
  initialHotel?: string;
  initialRoomType?: string;
}) {
  const [pending, startTransition] = useTransition();
  const [reference, setReference] = useState<string | null>(null);

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      hotelSlug: initialHotel ?? "",
      roomTypeId: initialRoomType ?? "",
      checkIn: "",
      checkOut: "",
      guests: 2,
      fullName: "",
      email: "",
      phone: "",
    },
  });

  const selectedHotel = hotels.find((h) => h.slug === form.watch("hotelSlug"));
  const roomTypes = selectedHotel?.roomTypes ?? [];

  function onSubmit(values: FormValues) {
    startTransition(async () => {
      const result = await createBookingAction(values);
      if (result.ok) {
        setReference(result.reference ?? "received");
        toast.success("Booking request sent");
      } else {
        toast.error(result.error ?? "Could not submit your booking");
      }
    });
  }

  if (reference) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center gap-3 p-10 text-center">
          <CheckCircle2 className="h-12 w-12 text-success" />
          <h2 className="text-xl font-semibold">Request received</h2>
          <p className="max-w-sm text-sm text-muted-foreground">
            Thanks! Your booking enquiry has been sent. Our team will confirm availability
            shortly. Your reference is:
          </p>
          <p className="rounded-md border bg-muted px-3 py-1.5 font-mono text-sm">
            {reference}
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardContent className="p-6">
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <FormField
              control={form.control}
              name="hotelSlug"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Hotel</FormLabel>
                  <Select
                    value={field.value}
                    onValueChange={(v) => {
                      field.onChange(v);
                      form.setValue("roomTypeId", "");
                    }}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select a hotel" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {hotels.map((h) => (
                        <SelectItem key={h.slug} value={h.slug}>
                          {h.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="roomTypeId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Room type</FormLabel>
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={!selectedHotel}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select a room" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {roomTypes.map((rt) => (
                        <SelectItem key={rt.id} value={rt.id}>
                          {rt.name} · {money(rt.baseRate)} / night
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="checkIn"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Check-in</FormLabel>
                    <FormControl>
                      <Input type="date" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="checkOut"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Check-out</FormLabel>
                    <FormControl>
                      <Input type="date" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="guests"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Guests</FormLabel>
                  <FormControl>
                    <Input type="number" min={1} max={20} {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="fullName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Full name</FormLabel>
                  <FormControl>
                    <Input placeholder="Jane Doe" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email</FormLabel>
                    <FormControl>
                      <Input type="email" placeholder="jane@example.com" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="phone"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Phone (optional)</FormLabel>
                    <FormControl>
                      <Input type="tel" placeholder="+1 555 000 0000" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <Button type="submit" className="w-full" disabled={pending}>
              {pending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Request booking
            </Button>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
