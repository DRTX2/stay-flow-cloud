using MassTransit;
using Microsoft.Extensions.Logging;
using StayFlow.Contracts;

namespace StayFlow.Infrastructure.Messaging;

/// <summary>
/// Baseline in-process consumer for relayed integration events. It logs receipt, proving the
/// outbox → bus → consumer path end-to-end. Out-of-process services (e.g. the Notification service)
/// bind their own queues to the same message and react independently — under RabbitMQ each consumer
/// gets its own durable queue, so this and the microservices all receive every published event.
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
