using FluentAssertions;
using StayFlow.Application.Pricing;

namespace StayFlow.UnitTests.Pricing;

public sealed class DynamicPricingServiceTests
{
    private readonly DynamicPricingService _sut = new();

    // 2026-01-01 is a Thursday, so Jan 5 = Mon, Jan 6 = Tue (mid-week, off-season).
    private static readonly DateOnly MidweekIn = new(2026, 1, 5);
    private static readonly DateOnly MidweekOut = new(2026, 1, 7); // 2 nights: Mon, Tue

    [Fact]
    public void Quote_MidweekOffSeasonLowOccupancy_AppliesNoAdjustments()
    {
        var quote = _sut.Quote(new PricingRequest(100m, MidweekIn, MidweekOut, OccupancyRate: 0.5, NumberOfGuests: 2));

        quote.Nights.Should().Be(2);
        quote.TotalPrice.Should().Be(200m);
        quote.AverageNightlyRate.Should().Be(100m);
        quote.Adjustments.Should().BeEmpty();
    }

    [Fact]
    public void Quote_WeekendNight_AppliesWeekendMultiplier()
    {
        // Jan 2 2026 is a Friday => weekend night.
        var quote = _sut.Quote(new PricingRequest(100m, new DateOnly(2026, 1, 2), new DateOnly(2026, 1, 3), 0.0, 2));

        quote.TotalPrice.Should().Be(125m);
        quote.Adjustments.Should().ContainSingle(a => a.Reason.StartsWith("Weekend"));
    }

    [Fact]
    public void Quote_HighSeasonNight_AppliesSeasonMultiplier()
    {
        // Jul 1 2026 is a Wednesday => high season, not weekend.
        var quote = _sut.Quote(new PricingRequest(100m, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 2), 0.0, 2));

        quote.TotalPrice.Should().Be(115m);
        quote.Adjustments.Should().ContainSingle(a => a.Reason.StartsWith("High-season"));
    }

    [Theory]
    [InlineData(0.5, 200)]   // below 0.6 => no demand multiplier
    [InlineData(0.6, 220)]   // 1.10x
    [InlineData(0.85, 240)]  // 1.20x
    public void Quote_Occupancy_AppliesDemandMultiplier(double occupancy, decimal expectedTotal)
    {
        var quote = _sut.Quote(new PricingRequest(100m, MidweekIn, MidweekOut, occupancy, 2));

        quote.TotalPrice.Should().Be(expectedTotal);
    }

    [Fact]
    public void Quote_ExtraGuests_AppliesSurcharge()
    {
        // 2 nights * 100 = 200 subtotal, 4 guests => 2 extra => x1.10 => 220.
        var quote = _sut.Quote(new PricingRequest(100m, MidweekIn, MidweekOut, 0.0, NumberOfGuests: 4));

        quote.TotalPrice.Should().Be(220m);
        quote.Adjustments.Should().Contain(a => a.Reason.StartsWith("Extra guests"));
    }

    [Fact]
    public void Quote_IsDeterministic()
    {
        var request = new PricingRequest(149.99m, MidweekIn, MidweekOut, 0.7, 3);

        _sut.Quote(request).TotalPrice.Should().Be(_sut.Quote(request).TotalPrice);
    }

    [Fact]
    public void Quote_CheckOutNotAfterCheckIn_Throws()
    {
        var act = () => _sut.Quote(new PricingRequest(100m, MidweekIn, MidweekIn, 0.0, 1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Quote_NegativeBaseRate_Throws()
    {
        var act = () => _sut.Quote(new PricingRequest(-1m, MidweekIn, MidweekOut, 0.0, 1));

        act.Should().Throw<ArgumentException>();
    }
}
