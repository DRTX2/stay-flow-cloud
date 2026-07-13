using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Infrastructure.Observability;
using StayFlow.Persistence;

namespace StayFlow.Infrastructure.Jobs;

/// <summary>
/// Prunes outbox rows that have already been relayed and aged out, keeping the table from growing
/// unbounded. Only processed messages past the retention window are removed.
/// </summary>
[AutomaticRetry(Attempts = 3)]
public sealed class OutboxCleanupJob(
    StayFlowDbContext dbContext,
    IDateTimeProvider clock,
    StayFlowMetrics metrics,
    ILogger<OutboxCleanupJob> logger)
{
    private static readonly TimeSpan _retention = TimeSpan.FromDays(7);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var measurement = metrics.MeasureJob("outbox_cleanup");
        var cutoff = clock.UtcNow - _retention;

        var removed = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc != null && message.ProcessedOnUtc < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogInformation(
            "Outbox cleanup: removed {Count} processed message(s) older than {Cutoff:u}",
            removed, cutoff);
        measurement.Succeed();
    }
}
