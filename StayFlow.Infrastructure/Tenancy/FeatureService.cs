using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Tenants;

namespace StayFlow.Infrastructure.Tenancy;

/// <summary>
/// Resolves effective features/limits for the current tenant: plan defaults from
/// <see cref="PlanCatalog"/> with per-tenant <see cref="TenantFeatureOverride"/>s layered on top.
/// </summary>
public sealed class FeatureService(IApplicationDbContext dbContext, ITenantProvider tenantProvider) : IFeatureService
{
    public async Task<SubscriptionPlan> GetPlanAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = tenantProvider.TenantId ?? Guid.Empty;
        var plan = await dbContext.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => t.Plan)
            .FirstOrDefaultAsync(cancellationToken);

        return plan == 0 ? SubscriptionPlan.Basic : plan;
    }

    public async Task<PlanLimits> GetLimitsAsync(CancellationToken cancellationToken = default)
        => PlanCatalog.For(await GetPlanAsync(cancellationToken));

    public async Task<bool> IsEnabledAsync(Feature feature, CancellationToken cancellationToken = default)
    {
        var @override = await dbContext.TenantFeatureOverrides
            .AsNoTracking()
            .Where(o => o.Feature == feature)
            .Select(o => (bool?)o.IsEnabled)
            .FirstOrDefaultAsync(cancellationToken);

        return @override ?? (await GetLimitsAsync(cancellationToken)).Includes(feature);
    }

    public async Task<IReadOnlyDictionary<Feature, bool>> GetEffectiveFlagsAsync(CancellationToken cancellationToken = default)
    {
        var limits = await GetLimitsAsync(cancellationToken);
        var overrides = await dbContext.TenantFeatureOverrides
            .AsNoTracking()
            .ToDictionaryAsync(o => o.Feature, o => o.IsEnabled, cancellationToken);

        return Enum.GetValues<Feature>()
            .ToDictionary(
                feature => feature,
                feature => overrides.TryGetValue(feature, out var enabled) ? enabled : limits.Includes(feature));
    }
}
