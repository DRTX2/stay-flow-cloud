using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Contracts;
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
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

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

        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(BatchSize)
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
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.Error = ex.Message;
                logger.LogError(ex, "Failed to relay outbox message {MessageId}", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
