namespace StayFlow.Persistence.Outbox;

/// <summary>
/// A domain event captured for reliable, asynchronous relay to the integration bus. Persisted in the
/// same transaction as the originating aggregate change (transactional outbox pattern) by
/// <c>ConvertDomainEventsToOutboxInterceptor</c>, then drained and published by the outbox processor.
/// Rows are append-only except for the fields recording the relay outcome.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    /// <summary>Tenant captured at write time, since the relay runs without a request context.</summary>
    public Guid? TenantId { get; set; }

    /// <summary>Acting user captured at write time, for the same reason as <see cref="TenantId"/>.</summary>
    public Guid? UserId { get; set; }

    /// <summary>CLR type name of the originating domain event.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>JSON-serialized domain event payload.</summary>
    public string Content { get; set; } = string.Empty;

    public DateTimeOffset OccurredOnUtc { get; set; }

    /// <summary>Set once the message has been successfully relayed; null while pending.</summary>
    public DateTimeOffset? ProcessedOnUtc { get; set; }

    /// <summary>Number of relay attempts, incremented on each failure.</summary>
    public int Attempts { get; set; }

    /// <summary>Last relay error, if any.</summary>
    public string? Error { get; set; }
}
