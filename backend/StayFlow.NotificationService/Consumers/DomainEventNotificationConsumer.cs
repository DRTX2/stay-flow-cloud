using MassTransit;
using StayFlow.Contracts;

namespace StayFlow.NotificationService.Consumers;

/// <summary>
/// Out-of-process consumer for the <see cref="DomainEventOccurred"/> integration event. It runs in
/// its own service and binds its own durable RabbitMQ queue, so it receives every event the monolith
/// publishes independently of the in-process consumer. It maps interesting event types onto guest- and
/// staff-facing notifications.
/// </summary>
/// <remarks>
/// Delivery is at-least-once, so the same event can arrive more than once; a production dispatcher
/// would dedupe on <see cref="DomainEventOccurred.Id"/> before actually sending. Here the channels are
/// stubbed with structured logging to keep the service self-contained for the portfolio.
/// </remarks>
public sealed class DomainEventNotificationConsumer(ILogger<DomainEventNotificationConsumer> logger)
    : IConsumer<DomainEventOccurred>
{
    public Task Consume(ConsumeContext<DomainEventOccurred> context)
    {
        var message = context.Message;

        var channel = ResolveChannel(message.EventType);
        if (channel is null)
        {
            logger.LogDebug(
                "No notification configured for {EventType} (event {EventId}); skipping",
                message.EventType, message.Id);
            return Task.CompletedTask;
        }

        // Stub for the real dispatch (email/SMS/push). Idempotency key is the event id.
        logger.LogInformation(
            "Dispatching {Channel} notification for {EventType} (event {EventId}, tenant {TenantId}, user {UserId})",
            channel, message.EventType, message.Id, message.TenantId, message.UserId);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Picks a delivery channel from the event type. Matching on a suffix keeps it decoupled from the
    /// publisher's exact namespace while still reacting to the meaningful business events.
    /// </summary>
    private static string? ResolveChannel(string eventType) => eventType switch
    {
        var type when type.EndsWith("ReservationCreated", StringComparison.OrdinalIgnoreCase) => "Email",
        var type when type.EndsWith("ReservationCancelled", StringComparison.OrdinalIgnoreCase) => "Email",
        var type when type.EndsWith("ReservationCanceled", StringComparison.OrdinalIgnoreCase) => "Email",
        var type when type.EndsWith("InvoiceGenerated", StringComparison.OrdinalIgnoreCase) => "Email",
        var type when type.EndsWith("InvoicePaid", StringComparison.OrdinalIgnoreCase) => "Email",
        var type when type.EndsWith("GuestRegistered", StringComparison.OrdinalIgnoreCase) => "Email",
        _ => null,
    };
}
