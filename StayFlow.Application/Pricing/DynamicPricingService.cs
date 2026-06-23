namespace StayFlow.Application.Pricing;

/// <summary>
/// Demand-based pricing engine. Per night it applies weekend and seasonal multipliers, then
/// over the whole stay applies an occupancy (demand) multiplier and an extra-guest surcharge.
/// Deterministic: identical inputs always yield the same quote, so it is unit-testable.
/// </summary>
public sealed class DynamicPricingService : IPricingService
{
    private const decimal WeekendMultiplier = 1.25m;
    private const decimal HighSeasonMultiplier = 1.15m;
    private const int IncludedGuests = 2;
    private const decimal ExtraGuestSurchargePerGuest = 0.05m;

    public PriceQuote Quote(PricingRequest request)
    {
        if (request.CheckOut <= request.CheckIn)
        {
            throw new ArgumentException("Check-out must be after check-in.", nameof(request));
        }

        if (request.BaseNightlyRate < 0)
        {
            throw new ArgumentException("Base nightly rate cannot be negative.", nameof(request));
        }

        var adjustments = new List<PriceAdjustment>();
        var weekendNights = 0;
        var highSeasonNights = 0;
        decimal subtotal = 0m;

        for (var day = request.CheckIn; day < request.CheckOut; day = day.AddDays(1))
        {
            var nightly = request.BaseNightlyRate;

            if (IsWeekend(day))
            {
                nightly *= WeekendMultiplier;
                weekendNights++;
            }

            if (IsHighSeason(day))
            {
                nightly *= HighSeasonMultiplier;
                highSeasonNights++;
            }

            subtotal += nightly;
        }

        if (weekendNights > 0)
        {
            adjustments.Add(new PriceAdjustment($"Weekend nights ({weekendNights})", WeekendMultiplier));
        }

        if (highSeasonNights > 0)
        {
            adjustments.Add(new PriceAdjustment($"High-season nights ({highSeasonNights})", HighSeasonMultiplier));
        }

        var occupancyMultiplier = ResolveOccupancyMultiplier(request.OccupancyRate);
        if (occupancyMultiplier != 1m)
        {
            adjustments.Add(new PriceAdjustment($"Demand (occupancy {request.OccupancyRate:P0})", occupancyMultiplier));
            subtotal *= occupancyMultiplier;
        }

        if (request.NumberOfGuests > IncludedGuests)
        {
            var extraGuests = request.NumberOfGuests - IncludedGuests;
            var surchargeMultiplier = 1m + (ExtraGuestSurchargePerGuest * extraGuests);
            adjustments.Add(new PriceAdjustment($"Extra guests ({extraGuests})", surchargeMultiplier));
            subtotal *= surchargeMultiplier;
        }

        var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
        var total = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero);
        var average = Math.Round(total / nights, 2, MidpointRounding.AwayFromZero);

        return new PriceQuote(total, average, nights, adjustments);
    }

    private static bool IsWeekend(DateOnly day) => day.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;

    private static bool IsHighSeason(DateOnly day) => day.Month is 6 or 7 or 8 or 12;

    private static decimal ResolveOccupancyMultiplier(double occupancyRate) => occupancyRate switch
    {
        >= 0.8 => 1.20m,
        >= 0.6 => 1.10m,
        _ => 1.0m,
    };
}
