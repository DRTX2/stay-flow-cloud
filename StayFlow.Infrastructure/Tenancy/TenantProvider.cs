using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Infrastructure.Tenancy;

/// <summary>
/// Resolves the active tenant for the current request from the authenticated principal's
/// <c>tenant_id</c> claim (projected by <see cref="ICurrentUser"/>). This is what drives the
/// global query filter and tenant stamping in persistence, so every authenticated request is
/// transparently scoped to its tenant.
/// </summary>
public sealed class TenantProvider(ICurrentUser currentUser) : ITenantProvider
{
    public Guid? TenantId => currentUser.TenantId;
}
