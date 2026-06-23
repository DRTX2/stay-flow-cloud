namespace StayFlow.Application.Common.Notifications;

public enum NotificationChannel
{
    Email,
    Sms,
    Push,
}

/// <summary>A channel-agnostic message to deliver to a single recipient.</summary>
public sealed record NotificationMessage(
    NotificationChannel Channel,
    string Recipient,
    string Subject,
    string Body);

/// <summary>
/// Sends outbound notifications (email/SMS/push). The application layer depends on this
/// abstraction; the infrastructure layer chooses the transport (SMTP, SES, SNS/Twilio, FCM…).
/// </summary>
public interface INotificationService
{
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
