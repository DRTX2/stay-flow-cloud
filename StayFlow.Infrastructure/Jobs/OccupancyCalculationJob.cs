using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Reservations;
using StayFlow.Persistence;

namespace StayFlow.Infrastructure.Jobs;

/// <summary>
/// Computes per-tenant occupancy for the current date. Runs without a request, so it bypasses the
/// tenant query filter and iterates every tenant explicitly.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public sealed class OccupancyCalculationJob(
    StayFlowDbContext dbContext,
    IDateTimeProvider clock,
    ILogger<OccupancyCalculationJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        var tenants = await dbContext.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(tenant => new { tenant.Id, tenant.Name })
            .ToListAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            var totalRooms = await dbContext.Rooms
                .IgnoreQueryFilters()
                .CountAsync(room => room.TenantId == tenant.Id && !room.IsDeleted, cancellationToken);

            if (totalRooms == 0)
            {
                continue;
            }

            var occupied = await dbContext.Reservations
                .IgnoreQueryFilters()
                .CountAsync(
                    reservation => reservation.TenantId == tenant.Id
                        && !reservation.IsDeleted
                        && (reservation.Status == ReservationStatus.Confirmed || reservation.Status == ReservationStatus.CheckedIn)
                        && reservation.Period.CheckIn <= today
                        && today < reservation.Period.CheckOut,
                    cancellationToken);

            var occupancyPercent = Math.Round(100.0 * occupied / totalRooms, 1);
            logger.LogInformation(
                "Occupancy for {Tenant} on {Date}: {Occupied}/{Total} rooms ({Percent}%)",
                tenant.Name, today, occupied, totalRooms, occupancyPercent);
        }
    }
}
