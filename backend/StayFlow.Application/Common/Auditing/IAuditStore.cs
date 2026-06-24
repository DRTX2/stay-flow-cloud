namespace StayFlow.Application.Common.Auditing;

/// <summary>An immutable record of something that happened, persisted to the activity/event store.</summary>
public sealed record AuditRecord(
    Guid Id,
    Guid? TenantId,
    Guid? UserId,
    string EventType,
    DateTimeOffset OccurredOnUtc,
    string Payload);

/// <summary>
/// Append-only store for audit events and activity streams. Backed by MongoDB when configured;
/// a no-op implementation is used otherwise so the rest of the system is unaffected.
/// </summary>
public interface IAuditStore
{
    Task AppendAsync(AuditRecord record, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditRecord>> GetRecentAsync(Guid? tenantId, int take, CancellationToken cancellationToken = default);
}
