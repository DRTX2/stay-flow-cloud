namespace StayFlow.Domain.Common;

/// <summary>
/// Auditable entity scoped to a tenant and soft-deletable. <see cref="TenantId"/> is
/// stamped by the persistence interceptor on insert and enforced by a global query filter,
/// so application/domain code never needs to set or filter it manually.
/// </summary>
public abstract class TenantEntity : AuditableEntity, ITenantScoped, ISoftDeletable
{
    protected TenantEntity()
    {
    }

    protected TenantEntity(Guid id) : base(id)
    {
    }

    public Guid TenantId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
