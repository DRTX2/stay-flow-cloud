using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Analytics.Queries;

/// <summary>Operational KPIs for the current tenant's dashboard. Tenant-scoped via the query filter.</summary>
public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public sealed record DashboardSummaryDto(
    DateOnly Date,
    int TotalRooms,
    int OccupiedRooms,
    double OccupancyRate,
    int ArrivalsToday,
    int DeparturesToday,
    int InHouse,
    IReadOnlyDictionary<string, int> ReservationsByStatus,
    IReadOnlyDictionary<string, int> InvoicesByStatus,
    decimal BookedRevenueLast30Days);

public sealed class GetDashboardSummaryHandler(IApplicationDbContext dbContext, IDateTimeProvider clock)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private static readonly ReservationStatus[] ActiveStatuses =
    [
        ReservationStatus.Pending,
        ReservationStatus.Confirmed,
        ReservationStatus.CheckedIn,
    ];

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var revenueCutoff = today.AddDays(-30);

        var totalRooms = await dbContext.Rooms.CountAsync(cancellationToken);

        var occupiedRooms = await dbContext.Reservations
            .Where(r => ActiveStatuses.Contains(r.Status) && r.Period.CheckIn <= today && today < r.Period.CheckOut)
            .Select(r => r.RoomId)
            .Distinct()
            .CountAsync(cancellationToken);

        var arrivalsToday = await dbContext.Reservations
            .CountAsync(r => r.Period.CheckIn == today && r.Status != ReservationStatus.Cancelled, cancellationToken);

        var departuresToday = await dbContext.Reservations
            .CountAsync(r => r.Period.CheckOut == today && r.Status != ReservationStatus.Cancelled, cancellationToken);

        var inHouse = await dbContext.Reservations
            .CountAsync(r => r.Status == ReservationStatus.CheckedIn, cancellationToken);

        var reservationsByStatus = await dbContext.Reservations
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var invoicesByStatus = await dbContext.Invoices
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var bookedRevenue = await dbContext.Reservations
            .Where(r => r.Status == ReservationStatus.CheckedOut
                && r.Period.CheckOut >= revenueCutoff
                && r.Period.CheckOut <= today)
            .SumAsync(r => (decimal?)r.TotalPrice, cancellationToken) ?? 0m;

        return new DashboardSummaryDto(
            today,
            totalRooms,
            occupiedRooms,
            totalRooms == 0 ? 0d : Math.Round((double)occupiedRooms / totalRooms, 4),
            arrivalsToday,
            departuresToday,
            inHouse,
            reservationsByStatus.ToDictionary(x => x.Status.ToString(), x => x.Count),
            invoicesByStatus.ToDictionary(x => x.Status.ToString(), x => x.Count),
            bookedRevenue);
    }
}
