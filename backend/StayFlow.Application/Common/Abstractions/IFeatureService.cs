using StayFlow.Domain.Tenants;

namespace StayFlow.Application.Common.Abstractions;

/// <summary>
/// Resolves the effective feature/limit configuration for the current tenant: plan defaults
/// combined with any per-tenant overrides.
/// </summary>
public interface IFeatureService
{
    Task<SubscriptionPlan> GetPlanAsync(CancellationToken cancellationToken = default);

    Task<PlanLimits> GetLimitsAsync(CancellationToken cancellationToken = default);

    Task<bool> IsEnabledAsync(Feature feature, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Feature, bool>> GetEffectiveFlagsAsync(CancellationToken cancellationToken = default);
}
