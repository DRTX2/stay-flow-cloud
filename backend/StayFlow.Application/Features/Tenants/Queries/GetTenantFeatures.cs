using MediatR;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Tenants;

namespace StayFlow.Application.Features.Tenants.Queries;

public sealed record PlanLimitsDto(int MaxRooms, int MaxUsers, int MaxServiceItems);

public sealed record TenantFeatureDto(
    string Key,
    string Name,
    bool Enabled,
    bool IncludedInPlan,
    SubscriptionPlan RequiredPlan);

public sealed record TenantFeaturesDto(
    SubscriptionPlan Plan,
    PlanLimitsDto Limits,
    IReadOnlyDictionary<string, bool> Features,
    IReadOnlyList<TenantFeatureDto> FeatureDetails);

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
        var featureDetails = flags
            .OrderBy(flag => flag.Key.ToString())
            .Select(flag => new TenantFeatureDto(
                flag.Key.ToString(),
                Humanize(flag.Key.ToString()),
                flag.Value,
                limits.Includes(flag.Key),
                PlanCatalog.RequiredPlanFor(flag.Key)))
            .ToList();

        return new TenantFeaturesDto(
            plan,
            new PlanLimitsDto(limits.MaxRooms, limits.MaxUsers, limits.MaxServiceItems),
            flags.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
            featureDetails);
    }

    private static string Humanize(string value)
        => string.Concat(value.Select((character, index) =>
            index > 0 && char.IsUpper(character) ? $" {character}" : character.ToString()));
}
