using MassTransit;
using Microsoft.Extensions.Logging;

namespace StayFlow.Infrastructure.Messaging;

/// <summary>
/// Baseline consumer for relayed integration events. It logs receipt, proving the outbox → bus →
/// consumer path end-to-end; real integrations (email/SMS notifications, ERP/CRM sync) would add
/// their own consumers for the same message. With the in-memory transport this runs in-process;
/// pointing MassTransit at RabbitMQ/SQS later makes it a true out-of-process consumer with no
/// changes here.
/// </summary>
public sealed class DomainEventOccurredConsumer(ILogger<DomainEventOccurredConsumer> logger)
    : IConsumer<DomainEventOccurred>
{
    public Task Consume(ConsumeContext<DomainEventOccurred> context)
    {
        var message = context.Message;
        logger.LogInformation(
            "Integration event {EventType} (tenant {TenantId}, user {UserId}) relayed via bus",
            message.EventType, message.TenantId, message.UserId);

        return Task.CompletedTask;
    }
}
