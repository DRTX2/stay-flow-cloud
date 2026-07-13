namespace StayFlow.Persistence.Notifications;

/// <summary>A durable notification addressed to one user within one tenant.</summary>
public sealed class InAppNotification
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Link { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ReadAtUtc { get; set; }
    public Guid SourceEventId { get; set; }
}
