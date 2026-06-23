namespace StayFlow.Application.Pricing;

/// <summary>Computes the price of a stay from the base rate and demand signals.</summary>
public interface IPricingService
{
    PriceQuote Quote(PricingRequest request);
}

/// <param name="BaseNightlyRate">Room base nightly rate in the tenant's currency.</param>
/// <param name="CheckIn">Inclusive arrival date.</param>
/// <param name="CheckOut">Exclusive departure date.</param>
/// <param name="OccupancyRate">Property occupancy over the stay, 0..1, used for demand pricing.</param>
/// <param name="NumberOfGuests">Guests staying; drives the extra-guest surcharge.</param>
public sealed record PricingRequest(
    decimal BaseNightlyRate,
    DateOnly CheckIn,
    DateOnly CheckOut,
    double OccupancyRate,
    int NumberOfGuests);

public sealed record PriceQuote(
    decimal TotalPrice,
    decimal AverageNightlyRate,
    int Nights,
    IReadOnlyList<PriceAdjustment> Adjustments);

public sealed record PriceAdjustment(string Reason, decimal Multiplier);
