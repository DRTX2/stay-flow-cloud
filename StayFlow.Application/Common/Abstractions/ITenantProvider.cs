namespace StayFlow.Application.Common.Abstractions;

/// <summary>
/// Resolves the tenant for the current execution context (request). Drives the global
/// query filter and tenant stamping in the persistence layer.
/// </summary>
public interface ITenantProvider
{
    Guid? TenantId { get; }

    bool HasTenant => TenantId is { } id && id != Guid.Empty;
}
