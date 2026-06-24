using FluentAssertions;
using StayFlow.Domain.Common;
using StayFlow.Domain.Reservations;

namespace StayFlow.UnitTests.Domain;

public sealed class DateRangeTests
{
    [Fact]
    public void Create_CountsNightsAsHalfOpenInterval()
    {
        var range = DateRange.Create(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 4));

        range.Nights.Should().Be(3);
    }

    [Fact]
    public void Create_CheckOutNotAfterCheckIn_Throws()
    {
        var act = () => DateRange.Create(new DateOnly(2026, 5, 4), new DateOnly(2026, 5, 4));

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("2026-05-03", "2026-05-06", true)]  // overlaps tail
    [InlineData("2026-05-04", "2026-05-08", false)] // starts on our checkout => no shared night
    [InlineData("2026-04-28", "2026-05-02", true)]  // overlaps head
    [InlineData("2026-06-01", "2026-06-03", false)] // fully after
    public void OverlapsWith_DetectsSharedNights(string otherIn, string otherOut, bool expected)
    {
        var baseRange = DateRange.Create(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 4));
        var other = DateRange.Create(DateOnly.Parse(otherIn), DateOnly.Parse(otherOut));

        baseRange.OverlapsWith(other).Should().Be(expected);
    }

    [Fact]
    public void Equality_IsByValue()
    {
        var a = DateRange.Create(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 4));
        var b = DateRange.Create(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 4));

        a.Should().Be(b);
    }
}
