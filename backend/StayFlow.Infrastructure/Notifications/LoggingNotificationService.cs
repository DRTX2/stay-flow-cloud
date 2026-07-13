using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Notifications;
using StayFlow.Infrastructure.Observability;

namespace StayFlow.Infrastructure.Notifications;

/// <summary>
/// Default notification transport: structured logging. Keeps the system runnable with no external
/// provider configured. Swap for an SES/SMTP sender (email), SNS or Twilio (SMS) and FCM/APNs
/// (push) implementations — typically selected per <see cref="NotificationChannel"/> — without
/// touching callers, which depend only on <see cref="INotificationService"/>.
/// </summary>
public sealed class LoggingNotificationService(ILogger<LoggingNotificationService> logger, StayFlowMetrics metrics) : INotificationService
{
    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[{Channel}] notification queued: {Subject}",
            message.Channel, message.Subject);
        metrics.RecordNotification(message.Channel.ToString(), succeeded: true);

        return Task.CompletedTask;
    }
}
