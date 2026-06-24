namespace StayFlow.Domain.Common;

/// <summary>Entity carrying audit metadata (created/updated by whom and when).</summary>
public abstract class AuditableEntity : Entity, IAuditable
{
    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id) : base(id)
    {
    }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
}
