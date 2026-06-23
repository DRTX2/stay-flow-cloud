using MediatR;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Tenants;

namespace StayFlow.Application.Features.Tenants.Queries;

public sealed record PlanLimitsDto(int MaxRooms, int MaxUsers, int MaxServiceItems);

public sealed record TenantFeaturesDto(
    SubscriptionPlan Plan,
    PlanLimitsDto Limits,
    IReadOnlyDictionary<string, bool> Features);

/// <summary>Returns the current tenant's plan, limits and effective feature flags.</summary>
public sealed record GetTenantFeaturesQuery : IRequest<TenantFeaturesDto>;

public sealed class GetTenantFeaturesHandler(IFeatureService features)
    : IRequestHandler<GetTenantFeaturesQuery, TenantFeaturesDto>
{
    public async Task<TenantFeaturesDto> Handle(GetTenantFeaturesQuery request, CancellationToken cancellationToken)
    {
        var plan = await features.GetPlanAsync(cancellationToken);
        var limits = await features.GetLimitsAsync(cancellationToken);
        var flags = await features.GetEffectiveFlagsAsync(cancellationToken);

        return new TenantFeaturesDto(
            plan,
            new PlanLimitsDto(limits.MaxRooms, limits.MaxUsers, limits.MaxServiceItems),
            flags.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value));
    }
}
