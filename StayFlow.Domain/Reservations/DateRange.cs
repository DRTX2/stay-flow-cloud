using StayFlow.Domain.Common;

namespace StayFlow.Domain.Reservations;

/// <summary>
/// The stay period of a reservation as a half-open interval [CheckIn, CheckOut).
/// The number of billable nights is the difference in days.
/// </summary>
public sealed class DateRange : ValueObject
{
    private DateRange(DateOnly checkIn, DateOnly checkOut)
    {
        CheckIn = checkIn;
        CheckOut = checkOut;
    }

    public DateOnly CheckIn { get; }

    public DateOnly CheckOut { get; }

    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

    public static DateRange Create(DateOnly checkIn, DateOnly checkOut)
    {
        if (checkOut <= checkIn)
        {
            throw new DomainException("Check-out must be after check-in.");
        }

        return new DateRange(checkIn, checkOut);
    }

    /// <summary>True when two stay periods share at least one night.</summary>
    public bool OverlapsWith(DateRange other) => CheckIn < other.CheckOut && other.CheckIn < CheckOut;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CheckIn;
        yield return CheckOut;
    }
}
