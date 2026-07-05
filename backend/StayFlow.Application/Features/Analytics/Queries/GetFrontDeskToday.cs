using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Housekeeping;
using StayFlow.Domain.Maintenance;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Analytics.Queries;

public sealed record GetFrontDeskTodayQuery(DateOnly? Date = null) : IRequest<FrontDeskTodayDto>;

public sealed record FrontDeskTodayDto(
    DateOnly Date,
    int Arrivals,
    int Departures,
    int InHouse,
    int DirtyRooms,
    int OutOfServiceRooms,
    int PendingHousekeepingTasks,
    int OpenMaintenanceWorkOrders,
    IReadOnlyList<FrontDeskReservationItemDto> ArrivalList,
    IReadOnlyList<FrontDeskReservationItemDto> DepartureList,
    IReadOnlyList<FrontDeskRoomIssueDto> RoomIssues);

public sealed record FrontDeskReservationItemDto(
    Guid ReservationId,
    string ConfirmationCode,
    Guid GuestId,
    string GuestName,
    Guid RoomId,
    string RoomNumber,
    string Status,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests);

public sealed record FrontDeskRoomIssueDto(
    Guid RoomId,
    string RoomNumber,
    string RoomStatus,
    string CleaningStatus,
    int OpenHousekeepingTasks,
    int OpenMaintenanceWorkOrders);

public sealed class GetFrontDeskTodayHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GetFrontDeskTodayQuery, FrontDeskTodayDto>
{
    public async Task<FrontDeskTodayDto> Handle(GetFrontDeskTodayQuery request, CancellationToken cancellationToken)
    {
        var date = request.Date ?? DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        var arrivals = await GetReservationItems(date, isArrival: true, cancellationToken);
        var departures = await GetReservationItems(date, isArrival: false, cancellationToken);

        var inHouse = await dbContext.Reservations
            .CountAsync(reservation => reservation.Status == ReservationStatus.CheckedIn, cancellationToken);

        var dirtyRooms = await dbContext.Rooms
            .CountAsync(room => room.CleaningStatus != RoomCleaningStatus.Clean
                && room.CleaningStatus != RoomCleaningStatus.Inspected, cancellationToken);

        var outOfServiceRooms = await dbContext.Rooms
            .CountAsync(room => room.Status == RoomStatus.Maintenance || room.Status == RoomStatus.OutOfService, cancellationToken);

        var pendingHousekeeping = await dbContext.HousekeepingTasks
            .CountAsync(task => task.Status != HousekeepingTaskStatus.Completed, cancellationToken);

        var openMaintenance = await dbContext.WorkOrders
            .CountAsync(workOrder => workOrder.Status == WorkOrderStatus.Open || workOrder.Status == WorkOrderStatus.InProgress, cancellationToken);

        var roomIssues = await GetRoomIssues(cancellationToken);

        return new FrontDeskTodayDto(
            date,
            arrivals.Count,
            departures.Count,
            inHouse,
            dirtyRooms,
            outOfServiceRooms,
            pendingHousekeeping,
            openMaintenance,
            arrivals,
            departures,
            roomIssues);
    }

    private async Task<IReadOnlyList<FrontDeskReservationItemDto>> GetReservationItems(
        DateOnly date,
        bool isArrival,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Reservations
            .Where(reservation => reservation.Status != ReservationStatus.Cancelled
                && reservation.Status != ReservationStatus.NoShow);

        query = isArrival
            ? query.Where(reservation => reservation.Period.CheckIn == date)
            : query.Where(reservation => reservation.Period.CheckOut == date);

        return await query
            .Join(dbContext.Guests,
                reservation => reservation.GuestId,
                guest => guest.Id,
                (reservation, guest) => new { reservation, guest })
            .Join(dbContext.Rooms,
                item => item.reservation.RoomId,
                room => room.Id,
                (item, room) => new { item.reservation, item.guest, room })
            .OrderBy(item => item.room.Number)
            .Select(item => new FrontDeskReservationItemDto(
                    item.reservation.Id,
                    item.reservation.ConfirmationCode,
                    item.guest.Id,
                    item.guest.FirstName + " " + item.guest.LastName,
                    item.room.Id,
                    item.room.Number,
                    item.reservation.Status.ToString(),
                    item.reservation.Period.CheckIn,
                    item.reservation.Period.CheckOut,
                    item.reservation.NumberOfGuests))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<FrontDeskRoomIssueDto>> GetRoomIssues(CancellationToken cancellationToken)
    {
        var rooms = await dbContext.Rooms
            .Where(room => room.Status == RoomStatus.Maintenance
                || room.Status == RoomStatus.OutOfService
                || (room.CleaningStatus != RoomCleaningStatus.Clean && room.CleaningStatus != RoomCleaningStatus.Inspected))
            .Select(room => new
            {
                room.Id,
                room.Number,
                room.Status,
                room.CleaningStatus,
            })
            .OrderBy(room => room.Number)
            .ToListAsync(cancellationToken);

        var roomIds = rooms.Select(room => room.Id).ToArray();

        var housekeepingCounts = await dbContext.HousekeepingTasks
            .Where(task => roomIds.Contains(task.RoomId) && task.Status != HousekeepingTaskStatus.Completed)
            .GroupBy(task => task.RoomId)
            .Select(group => new { RoomId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.RoomId, item => item.Count, cancellationToken);

        var maintenanceCounts = await dbContext.WorkOrders
            .Where(workOrder => workOrder.RoomId.HasValue
                && roomIds.Contains(workOrder.RoomId.Value)
                && (workOrder.Status == WorkOrderStatus.Open || workOrder.Status == WorkOrderStatus.InProgress))
            .GroupBy(workOrder => workOrder.RoomId!.Value)
            .Select(group => new { RoomId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.RoomId, item => item.Count, cancellationToken);

        return rooms.Select(room => new FrontDeskRoomIssueDto(
                room.Id,
                room.Number,
                room.Status.ToString(),
                room.CleaningStatus.ToString(),
                housekeepingCounts.GetValueOrDefault(room.Id),
                maintenanceCounts.GetValueOrDefault(room.Id)))
            .ToList();
    }
}
