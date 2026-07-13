"use client";

import { useState, useTransition } from "react";
import { useForm, useWatch } from "react-hook-form";
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
import {
  checkAvailabilityAction,
  createBookingAction,
} from "@/app/(public)/book/actions";
import type { Locale } from "@/i18n/config";
import type { PublicAvailability } from "@/types/api";

export interface BookingHotel {
  slug: string;
  name: string;
  roomTypes: { id: string; name: string; baseRate: number; maxOccupancy?: number }[];
}

const createSchema = (locale: Locale) =>
  z
    .object({
      hotelSlug: z
        .string()
        .min(1, locale === "es" ? "Selecciona un hotel" : "Select a hotel"),
      roomTypeId: z
        .string()
        .min(1, locale === "es" ? "Selecciona una habitación" : "Select a room"),
      checkIn: z.string().min(1, locale === "es" ? "Obligatorio" : "Required"),
      checkOut: z.string().min(1, locale === "es" ? "Obligatorio" : "Required"),
      guests: z.coerce
        .number()
        .int()
        .min(1, locale === "es" ? "Mínimo 1" : "At least 1")
        .max(20),
      fullName: z
        .string()
        .min(2, locale === "es" ? "Escribe tu nombre" : "Enter your name"),
      email: z
        .string()
        .email(locale === "es" ? "Escribe un correo válido" : "Enter a valid email"),
      phone: z.string().optional(),
    })
    .refine((v) => v.checkOut > v.checkIn, {
      message:
        locale === "es"
          ? "La salida debe ser posterior a la llegada"
          : "Check-out must be after check-in",
      path: ["checkOut"],
    });

type FormValues = z.infer<ReturnType<typeof createSchema>>;

export function BookingForm({
  hotels,
  initialHotel,
  initialRoomType,
  locale,
}: {
  hotels: BookingHotel[];
  initialHotel?: string;
  initialRoomType?: string;
  locale: Locale;
}) {
  const [pending, startTransition] = useTransition();
  const [checking, startAvailabilityTransition] = useTransition();
  const [reference, setReference] = useState<string | null>(null);
  const [availability, setAvailability] = useState<PublicAvailability | null>(null);
  const initialHotelRecord = hotels.find((hotel) => hotel.slug === initialHotel);
  const validInitialRoomType = initialHotelRecord?.roomTypes.some(
    (roomType) => roomType.id === initialRoomType,
  );

  const form = useForm<FormValues>({
    resolver: zodResolver(createSchema(locale)),
    defaultValues: {
      hotelSlug: initialHotelRecord?.slug ?? "",
      roomTypeId: validInitialRoomType ? (initialRoomType ?? "") : "",
      checkIn: "",
      checkOut: "",
      guests: 2,
      fullName: "",
      email: "",
      phone: "",
    },
  });

  const selectedHotelSlug = useWatch({ control: form.control, name: "hotelSlug" });
  const selectedHotel = hotels.find((h) => h.slug === selectedHotelSlug);
  const roomTypes = selectedHotel?.roomTypes ?? [];
  const selectedRoomTypeId = useWatch({ control: form.control, name: "roomTypeId" });
  const selectedRoomType = roomTypes.find(
    (roomType) => roomType.id === selectedRoomTypeId,
  );
  const minimumDate = new Date().toISOString().slice(0, 10);

  function onSubmit(values: FormValues) {
    startTransition(async () => {
      const checked = await checkAvailabilityAction(values);
      if (
        !checked.ok ||
        !checked.availability ||
        checked.availability.availableRoomCount < 1
      ) {
        setAvailability(checked.availability ?? null);
        toast.error(
          locale === "es"
            ? "No hay habitaciones disponibles para esas fechas."
            : "No rooms are available for those dates.",
        );
        return;
      }
      const result = await createBookingAction(values);
      if (result.ok) {
        setReference(result.reference ?? "received");
        toast.success(
          locale === "es" ? "Solicitud de reserva enviada" : "Booking request sent",
        );
      } else {
        toast.error(
          result.error ??
            (locale === "es"
              ? "No se pudo enviar tu reserva"
              : "Could not submit your booking"),
        );
      }
    });
  }

  async function checkAvailability() {
    const values = form.getValues();
    const valid = await form.trigger([
      "hotelSlug",
      "roomTypeId",
      "checkIn",
      "checkOut",
      "guests",
    ]);
    if (
      !valid ||
      !values.hotelSlug ||
      !values.roomTypeId ||
      !values.checkIn ||
      !values.checkOut
    )
      return;
    startAvailabilityTransition(async () => {
      const result = await checkAvailabilityAction(values);
      if (!result.ok) toast.error(result.error);
      setAvailability(result.availability ?? null);
    });
  }

  if (reference) {
    return (
      <Card>
        <CardContent
          role="status"
          aria-live="polite"
          className="flex flex-col items-center gap-3 p-6 text-center sm:p-10"
        >
          <CheckCircle2 className="h-12 w-12 text-success" />
          <h2 className="text-xl font-semibold">
            {locale === "es" ? "Solicitud recibida" : "Request received"}
          </h2>
          <p className="max-w-sm text-sm text-muted-foreground">
            {locale === "es"
              ? "Gracias. Enviamos tu solicitud y nuestro equipo confirmará pronto la disponibilidad. Tu referencia es:"
              : "Thanks! Your booking enquiry has been sent. Our team will confirm availability shortly. Your reference is:"}
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
                        <SelectValue
                          placeholder={
                            locale === "es" ? "Selecciona un hotel" : "Select a hotel"
                          }
                        />
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

            <div className="rounded-lg border bg-muted/30 p-4" aria-live="polite">
              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <div className="text-sm">
                  {availability ? (
                    availability.availableRoomCount > 0 ? (
                      <>
                        <p className="font-medium text-success">
                          {availability.availableRoomCount}{" "}
                          {locale === "es"
                            ? "habitaciones disponibles"
                            : "rooms available"}
                        </p>
                        <p className="text-muted-foreground">
                          {availability.nights} {locale === "es" ? "noches" : "nights"} ·{" "}
                          {money(availability.estimatedTotal, availability.currency)}{" "}
                          {locale === "es" ? "estimado" : "estimated"}
                        </p>
                      </>
                    ) : (
                      <p className="font-medium text-destructive">
                        {locale === "es"
                          ? "Sin disponibilidad para estas fechas"
                          : "No availability for these dates"}
                      </p>
                    )
                  ) : (
                    <p className="text-muted-foreground">
                      {locale === "es"
                        ? "Comprueba disponibilidad y precio antes de enviar."
                        : "Check availability and price before submitting."}
                    </p>
                  )}
                </div>
                <Button
                  type="button"
                  variant="outline"
                  onClick={checkAvailability}
                  disabled={checking}
                >
                  {checking && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                  {locale === "es" ? "Comprobar" : "Check availability"}
                </Button>
              </div>
            </div>

            <FormField
              control={form.control}
              name="roomTypeId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>
                    {locale === "es" ? "Tipo de habitación" : "Room type"}
                  </FormLabel>
                  <Select
                    value={field.value}
                    onValueChange={field.onChange}
                    disabled={!selectedHotel}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue
                          placeholder={
                            locale === "es"
                              ? "Selecciona una habitación"
                              : "Select a room"
                          }
                        />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {roomTypes.map((rt) => (
                        <SelectItem key={rt.id} value={rt.id}>
                          {rt.name} · {money(rt.baseRate)} /{" "}
                          {locale === "es" ? "noche" : "night"}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField
                control={form.control}
                name="checkIn"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>{locale === "es" ? "Llegada" : "Check-in"}</FormLabel>
                    <FormControl>
                      <Input type="date" min={minimumDate} {...field} />
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
                    <FormLabel>{locale === "es" ? "Salida" : "Check-out"}</FormLabel>
                    <FormControl>
                      <Input type="date" min={minimumDate} {...field} />
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
                  <FormLabel>{locale === "es" ? "Huéspedes" : "Guests"}</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      min={1}
                      max={selectedRoomType?.maxOccupancy ?? 20}
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                  {selectedRoomType?.maxOccupancy && (
                    <p className="text-xs text-muted-foreground">
                      {locale === "es" ? "Ocupación máxima" : "Maximum occupancy"}:{" "}
                      {selectedRoomType.maxOccupancy}
                    </p>
                  )}
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="fullName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>
                    {locale === "es" ? "Nombre completo" : "Full name"}
                  </FormLabel>
                  <FormControl>
                    <Input placeholder="Jane Doe" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="grid gap-4 sm:grid-cols-2">
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
                    <FormLabel>
                      {locale === "es" ? "Teléfono (opcional)" : "Phone (optional)"}
                    </FormLabel>
                    <FormControl>
                      <Input type="tel" placeholder="+1 555 000 0000" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <Button
              type="submit"
              className="w-full"
              disabled={pending}
              aria-busy={pending}
            >
              {pending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {pending
                ? locale === "es"
                  ? "Enviando solicitud..."
                  : "Sending request..."
                : locale === "es"
                  ? "Solicitar reserva"
                  : "Request booking"}
            </Button>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
