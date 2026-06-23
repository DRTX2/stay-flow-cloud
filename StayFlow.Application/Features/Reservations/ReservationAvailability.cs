using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Reservations;

/// <summary>
/// Shared availability/occupancy queries used by reservation creation and price quoting.
/// "Active" reservations are those that still hold the room (pending, confirmed, checked-in).
/// </summary>
internal static class ReservationAvailability
{
    private static readonly ReservationStatus[] ActiveStatuses =
    [
        ReservationStatus.Pending,
        ReservationStatus.Confirmed,
        ReservationStatus.CheckedIn,
    ];

    public static async Task<bool> HasOverlapAsync(
        IApplicationDbContext dbContext,
        Guid roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken)
    {
        return await dbContext.Reservations.AnyAsync(
            r => r.RoomId == roomId
                 && ActiveStatuses.Contains(r.Status)
                 && r.Period.CheckIn < checkOut
                 && checkIn < r.Period.CheckOut,
            cancellationToken);
    }

    public static async Task<double> OccupancyAsync(
        IApplicationDbContext dbContext,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken)
    {
        var totalRooms = await dbContext.Rooms.CountAsync(cancellationToken);
        if (totalRooms == 0)
        {
            return 0d;
        }

        var occupiedRooms = await dbContext.Reservations
            .Where(r => ActiveStatuses.Contains(r.Status)
                        && r.Period.CheckIn < checkOut
                        && checkIn < r.Period.CheckOut)
            .Select(r => r.RoomId)
            .Distinct()
            .CountAsync(cancellationToken);

        return (double)occupiedRooms / totalRooms;
    }
}
