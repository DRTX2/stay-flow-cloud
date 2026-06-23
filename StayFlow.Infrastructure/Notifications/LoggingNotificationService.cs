using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Notifications;

namespace StayFlow.Infrastructure.Notifications;

/// <summary>
/// Default notification transport: structured logging. Keeps the system runnable with no external
/// provider configured. Swap for an SES/SMTP sender (email), SNS or Twilio (SMS) and FCM/APNs
/// (push) implementations — typically selected per <see cref="NotificationChannel"/> — without
/// touching callers, which depend only on <see cref="INotificationService"/>.
/// </summary>
public sealed class LoggingNotificationService(ILogger<LoggingNotificationService> logger) : INotificationService
{
    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[{Channel}] notification to {Recipient}: {Subject}",
            message.Channel, message.Recipient, message.Subject);

        return Task.CompletedTask;
    }
}
