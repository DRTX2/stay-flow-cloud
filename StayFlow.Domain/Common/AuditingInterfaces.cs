namespace StayFlow.Domain.Common;

/// <summary>Audit metadata stamped by the persistence interceptor.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAtUtc { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAtUtc { get; set; }
    string? UpdatedBy { get; set; }
}

/// <summary>Soft-delete marker; rows are filtered out by a global query filter.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAtUtc { get; set; }
    string? DeletedBy { get; set; }
}

/// <summary>Entity that belongs to a single tenant; isolated via a global query filter.</summary>
public interface ITenantScoped
{
    Guid TenantId { get; }
}
