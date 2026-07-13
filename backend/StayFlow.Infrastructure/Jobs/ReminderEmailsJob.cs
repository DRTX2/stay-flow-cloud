using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Notifications;
using StayFlow.Domain.Reservations;
using StayFlow.Infrastructure.Observability;
using StayFlow.Persistence;

namespace StayFlow.Infrastructure.Jobs;

/// <summary>
/// Sends a check-in reminder to every guest arriving tomorrow, across all tenants.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public sealed class ReminderEmailsJob(
    StayFlowDbContext dbContext,
    IDateTimeProvider clock,
    INotificationService notifications,
    StayFlowMetrics metrics,
    ILogger<ReminderEmailsJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var measurement = metrics.MeasureJob("reminder_emails");
        var tomorrow = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddDays(1);

        var arrivals = await dbContext.Reservations
            .IgnoreQueryFilters()
            .Where(reservation => !reservation.IsDeleted
                && reservation.Status == ReservationStatus.Confirmed
                && reservation.Period.CheckIn == tomorrow)
            .Join(
                dbContext.Guests.IgnoreQueryFilters(),
                reservation => reservation.GuestId,
                guest => guest.Id,
                (reservation, guest) => new { reservation.ConfirmationCode, guest.FirstName, guest.Email })
            .ToListAsync(cancellationToken);

        foreach (var arrival in arrivals)
        {
            await notifications.SendAsync(
                new NotificationMessage(
                    NotificationChannel.Email,
                    arrival.Email,
                    "Your stay starts tomorrow",
                    $"Hi {arrival.FirstName}, this is a reminder for your reservation {arrival.ConfirmationCode}. We look forward to welcoming you."),
                cancellationToken);
        }

        logger.LogInformation("Check-in reminders: sent {Count} for {Date}", arrivals.Count, tomorrow);
        measurement.Succeed();
    }
}
