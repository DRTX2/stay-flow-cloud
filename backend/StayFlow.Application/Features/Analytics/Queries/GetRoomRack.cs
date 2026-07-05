using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Analytics.Queries;

public sealed record GetRoomRackQuery(DateOnly? From = null, DateOnly? To = null) : IRequest<RoomRackDto>;

public sealed record RoomRackDto(DateOnly From, DateOnly To, IReadOnlyList<RoomRackRoomDto> Rooms);

public sealed record RoomRackRoomDto(
    Guid RoomId,
    string RoomNumber,
    string RoomTypeName,
    string RoomStatus,
    string CleaningStatus,
    IReadOnlyList<RoomRackReservationDto> Reservations);

public sealed record RoomRackReservationDto(
    Guid ReservationId,
    string ConfirmationCode,
    Guid GuestId,
    string GuestName,
    DateOnly CheckIn,
    DateOnly CheckOut,
    string Status);

public sealed class GetRoomRackHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GetRoomRackQuery, RoomRackDto>
{
    public async Task<RoomRackDto> Handle(GetRoomRackQuery request, CancellationToken cancellationToken)
    {
        var from = request.From ?? DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var to = request.To ?? from.AddDays(14);
        if (to < from)
        {
            (from, to) = (to, from);
        }

        var rooms = await dbContext.Rooms
            .AsNoTracking()
            .Join(dbContext.RoomTypes,
                room => room.RoomTypeId,
                roomType => roomType.Id,
                (room, roomType) => new
                {
                    room.Id,
                    room.Number,
                    RoomTypeName = roomType.Name,
                    room.Status,
                    room.CleaningStatus,
                })
            .OrderBy(room => room.Number)
            .ToListAsync(cancellationToken);

        var roomIds = rooms.Select(room => room.Id).ToArray();
        var reservations = await dbContext.Reservations
            .AsNoTracking()
            .Where(reservation => roomIds.Contains(reservation.RoomId)
                && reservation.Status != ReservationStatus.Cancelled
                && reservation.Status != ReservationStatus.NoShow
                && reservation.Period.CheckIn < to
                && reservation.Period.CheckOut > from)
            .Join(dbContext.Guests,
                reservation => reservation.GuestId,
                guest => guest.Id,
                (reservation, guest) => new
                {
                    reservation.Id,
                    reservation.RoomId,
                    reservation.GuestId,
                    reservation.ConfirmationCode,
                    reservation.Period.CheckIn,
                    reservation.Period.CheckOut,
                    reservation.Status,
                    GuestName = guest.FirstName + " " + guest.LastName,
                })
            .OrderBy(reservation => reservation.CheckIn)
            .ToListAsync(cancellationToken);

        var reservationsByRoom = reservations
            .GroupBy(reservation => reservation.RoomId)
            .ToDictionary(group => group.Key, group => group
                .Select(reservation => new RoomRackReservationDto(
                    reservation.Id,
                    reservation.ConfirmationCode,
                    reservation.GuestId,
                    reservation.GuestName,
                    reservation.CheckIn,
                    reservation.CheckOut,
                    reservation.Status.ToString()))
                .ToList());

        return new RoomRackDto(from, to, rooms
            .Select(room => new RoomRackRoomDto(
                room.Id,
                room.Number,
                room.RoomTypeName,
                room.Status.ToString(),
                room.CleaningStatus.ToString(),
                reservationsByRoom.GetValueOrDefault(room.Id, [])))
            .ToList());
    }
}
