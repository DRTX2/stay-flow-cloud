using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Contracts;
using StayFlow.Infrastructure.Observability;
using StayFlow.Persistence;

namespace StayFlow.Infrastructure.Messaging;

/// <summary>
/// Drains the transactional outbox: polls for unprocessed <c>OutboxMessage</c> rows, publishes each
/// as a <see cref="DomainEventOccurred"/> integration event to the bus, and records the outcome.
/// Runs on its own DI scope with no request context, which is why the originating tenant/user are
/// carried on the row rather than read from ambient providers. Failures are isolated per message and
/// retried on the next pass (at-least-once delivery; consumers must be idempotent).
/// </summary>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IBus bus,
    IDateTimeProvider clock,
    StayFlowMetrics metrics,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);
    private const int _batchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_pollInterval);

        try
        {
            // Run an immediate first pass, then on each tick.
            do
            {
                await ProcessPendingAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Host is shutting down.
        }
    }

    private async Task ProcessPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StayFlowDbContext>();

        var pending = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .Select(message => (DateTimeOffset?)message.OccurredOnUtc)
            .MinAsync(cancellationToken);
        var pendingCount = await dbContext.OutboxMessages
            .LongCountAsync(message => message.ProcessedOnUtc == null, cancellationToken);
        metrics.ObserveOutbox(pendingCount, pending is null ? TimeSpan.Zero : clock.UtcNow - pending.Value);

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(_batchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                await bus.Publish(
                    new DomainEventOccurred(
                        message.Id,
                        message.TenantId,
                        message.UserId,
                        message.Type,
                        message.Content,
                        message.OccurredOnUtc),
                    cancellationToken);

                message.ProcessedOnUtc = clock.UtcNow;
                message.Error = null;
                metrics.RecordOutboxPublish(succeeded: true);
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.Error = ex.Message;
                logger.LogError(ex, "Failed to relay outbox message {MessageId}", message.Id);
                metrics.RecordOutboxPublish(succeeded: false);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
