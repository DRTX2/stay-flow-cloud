using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Reservations;
using StayFlow.Persistence;

namespace StayFlow.Infrastructure.Jobs;

/// <summary>
/// Sends check-in reminders to guests arriving tomorrow. Currently logs the would-be recipients;
/// the dispatch hook for a notification service (email/SMS/push) is marked below.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public sealed class ReminderEmailsJob(
    StayFlowDbContext dbContext,
    IDateTimeProvider clock,
    ILogger<ReminderEmailsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var tomorrow = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddDays(1);

        var arriving = await dbContext.Reservations
            .IgnoreQueryFilters()
            .CountAsync(
                reservation => !reservation.IsDeleted
                    && reservation.Status == ReservationStatus.Confirmed
                    && reservation.Period.CheckIn == tomorrow,
                cancellationToken);

        logger.LogInformation(
            "Check-in reminders: {Count} guest(s) arriving {Date} would be notified",
            arriving, tomorrow);

        // TODO: enqueue one notification per reservation via an INotificationService.
    }
}
