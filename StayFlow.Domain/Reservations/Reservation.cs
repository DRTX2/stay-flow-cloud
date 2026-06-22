using StayFlow.Domain.Common;
using StayFlow.Domain.Reservations.Events;

namespace StayFlow.Domain.Reservations;

/// <summary>
/// Reservation aggregate root. Owns its lifecycle state machine
/// (Pending → Confirmed → CheckedIn → CheckedOut, with Cancelled/NoShow branches) and
/// raises domain events on each transition. Overlap/availability checks require other
/// reservations and therefore live in the application layer, not here.
/// </summary>
public sealed class Reservation : TenantEntity
{
    private Reservation()
    {
    }

    private Reservation(Guid roomId, Guid guestId, DateRange period, int numberOfGuests, decimal totalPrice, string confirmationCode)
    {
        RoomId = roomId;
        GuestId = guestId;
        Period = period;
        NumberOfGuests = numberOfGuests;
        TotalPrice = totalPrice;
        ConfirmationCode = confirmationCode;
        Status = ReservationStatus.Pending;
    }

    public Guid RoomId { get; private set; }

    public Guid GuestId { get; private set; }

    public DateRange Period { get; private set; } = null!;

    public int NumberOfGuests { get; private set; }

    /// <summary>Total stay price computed by the dynamic pricing engine at booking time.</summary>
    public decimal TotalPrice { get; private set; }

    public string ConfirmationCode { get; private set; } = string.Empty;

    public ReservationStatus Status { get; private set; }

    public string? CancellationReason { get; private set; }

    public static Reservation Create(Guid roomId, Guid guestId, DateRange period, int numberOfGuests, decimal totalPrice)
    {
        if (roomId == Guid.Empty)
        {
            throw new DomainException("Reservation must reference a room.");
        }

        if (guestId == Guid.Empty)
        {
            throw new DomainException("Reservation must reference a guest.");
        }

        if (numberOfGuests < 1)
        {
            throw new DomainException("A reservation must have at least one guest.");
        }

        if (totalPrice < 0)
        {
            throw new DomainException("Total price cannot be negative.");
        }

        var reservation = new Reservation(roomId, guestId, period, numberOfGuests, totalPrice, GenerateConfirmationCode());
        reservation.RaiseDomainEvent(new ReservationCreatedEvent(reservation.Id, reservation.TenantId, roomId, guestId, totalPrice));
        return reservation;
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new DomainException($"Only pending reservations can be confirmed (current: {Status}).");
        }

        Status = ReservationStatus.Confirmed;
        RaiseDomainEvent(new ReservationConfirmedEvent(Id, TenantId));
    }

    public void CheckIn()
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new DomainException($"Only confirmed reservations can be checked in (current: {Status}).");
        }

        Status = ReservationStatus.CheckedIn;
        RaiseDomainEvent(new ReservationCheckedInEvent(Id, TenantId));
    }

    public void CheckOut()
    {
        if (Status != ReservationStatus.CheckedIn)
        {
            throw new DomainException($"Only checked-in reservations can be checked out (current: {Status}).");
        }

        Status = ReservationStatus.CheckedOut;
        RaiseDomainEvent(new ReservationCheckedOutEvent(Id, TenantId));
    }

    public void Cancel(string? reason = null)
    {
        if (Status is ReservationStatus.CheckedIn or ReservationStatus.CheckedOut)
        {
            throw new DomainException($"A {Status} reservation cannot be cancelled.");
        }

        if (Status == ReservationStatus.Cancelled)
        {
            return;
        }

        Status = ReservationStatus.Cancelled;
        CancellationReason = reason;
        RaiseDomainEvent(new ReservationCancelledEvent(Id, TenantId, reason));
    }

    public void MarkNoShow()
    {
        if (Status != ReservationStatus.Confirmed)
        {
            throw new DomainException($"Only confirmed reservations can be marked as no-show (current: {Status}).");
        }

        Status = ReservationStatus.NoShow;
    }

    private static string GenerateConfirmationCode() =>
        $"SF-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
