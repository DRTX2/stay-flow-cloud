using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StayFlow.Application.Common.Auditing;

namespace StayFlow.Infrastructure.Auditing;

public static class AuditServiceCollectionExtensions
{
    /// <summary>
    /// Registers the audit store. Uses MongoDB when a connection string is supplied; otherwise a
    /// no-op store so domain-event publishing keeps working without Mongo present.
    /// </summary>
    public static IServiceCollection AddAudit(
        this IServiceCollection services,
        string? mongoConnectionString,
        string databaseName = "stayflow")
    {
        if (!string.IsNullOrWhiteSpace(mongoConnectionString))
        {
            var client = new MongoClient(mongoConnectionString);
            services.AddSingleton(client.GetDatabase(databaseName));
            services.AddSingleton<IAuditStore, MongoAuditStore>();
        }
        else
        {
            services.AddSingleton<IAuditStore, NullAuditStore>();
        }

        return services;
    }
}

/// <summary>No-op audit store used when MongoDB is not configured.</summary>
internal sealed class NullAuditStore : IAuditStore
{
    public Task AppendAsync(AuditRecord record, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IReadOnlyList<AuditRecord>> GetRecentAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<AuditRecord>>([]);
}
