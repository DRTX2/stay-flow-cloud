using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Reservations;
using StayFlow.Persistence;

namespace StayFlow.Infrastructure.Jobs;

/// <summary>
/// The classic hotel "night audit": closes out the operating day with an arrivals/departures/in-house
/// snapshot. The hook for marking unfulfilled arrivals as no-shows and rolling up daily revenue lives
/// here once those domain operations exist.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public sealed class NightAuditJob(
    StayFlowDbContext dbContext,
    IDateTimeProvider clock,
    ILogger<NightAuditJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        var reservations = dbContext.Reservations.IgnoreQueryFilters().Where(r => !r.IsDeleted);

        var arrivals = await reservations.CountAsync(r => r.Period.CheckIn == today, cancellationToken);
        var departures = await reservations.CountAsync(r => r.Period.CheckOut == today, cancellationToken);
        var inHouse = await reservations.CountAsync(r => r.Status == ReservationStatus.CheckedIn, cancellationToken);

        logger.LogInformation(
            "Night audit {Date}: {Arrivals} arrivals, {Departures} departures, {InHouse} in-house",
            today, arrivals, departures, inHouse);

        // TODO: flag overdue arrivals (Confirmed with CheckIn < today) as no-shows and persist a
        // daily revenue roll-up once the corresponding domain operations are available.
    }
}
