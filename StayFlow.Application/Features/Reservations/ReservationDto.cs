using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Reservations;

public sealed record ReservationDto(
    Guid Id,
    Guid RoomId,
    Guid GuestId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int NumberOfGuests,
    decimal TotalPrice,
    string ConfirmationCode,
    ReservationStatus Status)
{
    public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

    public static ReservationDto FromEntity(Reservation reservation) => new(
        reservation.Id,
        reservation.RoomId,
        reservation.GuestId,
        reservation.Period.CheckIn,
        reservation.Period.CheckOut,
        reservation.NumberOfGuests,
        reservation.TotalPrice,
        reservation.ConfirmationCode,
        reservation.Status);
}
