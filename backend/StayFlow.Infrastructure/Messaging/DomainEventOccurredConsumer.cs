using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFlow.Contracts;
using StayFlow.Infrastructure.Observability;
using StayFlow.Persistence;
using StayFlow.Persistence.Notifications;

namespace StayFlow.Infrastructure.Messaging;

/// <summary>
/// Baseline in-process consumer for relayed integration events. It logs receipt, proving the
/// outbox → bus → consumer path end-to-end. Out-of-process services (e.g. the Notification service)
/// bind their own queues to the same message and react independently — under RabbitMQ each consumer
/// gets its own durable queue, so this and the microservices all receive every published event.
/// </summary>
public sealed class DomainEventOccurredConsumer(
    StayFlowDbContext dbContext,
    ILogger<DomainEventOccurredConsumer> logger,
    StayFlowMetrics metrics)
    : IConsumer<DomainEventOccurred>
{
    private static readonly Dictionary<string, NotificationTemplate> Templates =
        new Dictionary<string, NotificationTemplate>(StringComparer.Ordinal)
        {
            ["ReservationCreatedEvent"] = new("Reservation created", "A new reservation is ready for review.", "reservation", "/dashboard/reservations"),
            ["ReservationConfirmedEvent"] = new("Reservation confirmed", "A reservation has been confirmed.", "reservation", "/dashboard/reservations"),
            ["ReservationCheckedInEvent"] = new("Guest checked in", "A reservation has completed check-in.", "reservation", "/dashboard/reservations"),
            ["ReservationCheckedOutEvent"] = new("Guest checked out", "A reservation has completed check-out.", "reservation", "/dashboard/reservations"),
            ["ReservationCancelledEvent"] = new("Reservation cancelled", "A reservation has been cancelled.", "reservation", "/dashboard/reservations"),
            ["OrderPlacedEvent"] = new("Order placed", "A new service order needs attention.", "order", "/dashboard/orders"),
            ["OrderDeliveredEvent"] = new("Order delivered", "A service order has been delivered.", "order", "/dashboard/orders"),
            ["InvoiceGeneratedEvent"] = new("Invoice generated", "A new invoice has been generated.", "billing", "/dashboard/invoices"),
            ["InvoicePaidEvent"] = new("Invoice paid", "An invoice payment has been recorded.", "billing", "/dashboard/invoices"),
            ["WorkOrderCreatedEvent"] = new("Work order created", "A new maintenance work order needs attention.", "maintenance", "/dashboard/maintenance"),
            ["WorkOrderResolvedEvent"] = new("Work order resolved", "A maintenance work order has been resolved.", "maintenance", "/dashboard/maintenance"),
        };

    public async Task Consume(ConsumeContext<DomainEventOccurred> context)
    {
        var message = context.Message;
        logger.LogInformation(
            "Integration event {EventType} (tenant {TenantId}, user {UserId}) relayed via bus",
            message.EventType, message.TenantId, message.UserId);
        metrics.RecordBusinessEvent("integration_event_consumed");

        if (message.TenantId is not { } tenantId || tenantId == Guid.Empty
            || !Templates.TryGetValue(EventName(message.EventType), out var template))
        {
            logger.LogDebug("No in-app notification configured for {EventType} (event {EventId})", message.EventType, message.Id);
            return;
        }

        var customerUserIds =
            from userRole in dbContext.UserRoles
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where role.NormalizedName == "CUSTOMER"
            select userRole.UserId;

        var recipientIds = await dbContext.Users
            .Where(user => user.TenantId == tenantId && user.IsActive && !customerUserIds.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync(context.CancellationToken);
        var existingRecipientIds = await dbContext.InAppNotifications
            .IgnoreQueryFilters()
            .Where(notification => notification.TenantId == tenantId
                && notification.SourceEventId == message.Id
                && recipientIds.Contains(notification.UserId))
            .Select(notification => notification.UserId)
            .ToListAsync(context.CancellationToken);
        var existing = existingRecipientIds.ToHashSet();
        var notifications = recipientIds
            .Where(userId => !existing.Contains(userId))
            .Select(userId => new InAppNotification
            {
                Id = Guid.CreateVersion7(),
                TenantId = tenantId,
                UserId = userId,
                Title = template.Title,
                Body = template.Body,
                Type = template.Type,
                Link = template.Link,
                CreatedAtUtc = message.OccurredOnUtc,
                SourceEventId = message.Id,
            })
            .ToList();

        if (notifications.Count == 0)
        {
            return;
        }

        dbContext.InAppNotifications.AddRange(notifications);
        await dbContext.SaveChangesAsync(context.CancellationToken);
        for (var i = 0; i < notifications.Count; i++)
        {
            metrics.RecordNotification("in_app", true);
        }
        metrics.RecordBusinessEvent("in_app_notifications_created");
        logger.LogInformation("Created {Count} in-app notifications for event {EventId}", notifications.Count, message.Id);
    }

    private static string EventName(string eventType) => eventType[(eventType.LastIndexOf('.') + 1)..];

    internal static bool IsNotifiableEvent(string eventType) => Templates.ContainsKey(EventName(eventType));

    private sealed record NotificationTemplate(string Title, string Body, string Type, string Link);
}
