namespace StayFlow.Domain.Tenants;

/// <summary>The usage limits and default feature set granted by a subscription plan.</summary>
public sealed record PlanLimits(int MaxRooms, int MaxUsers, int MaxServiceItems, IReadOnlySet<Feature> Features)
{
    public const int Unlimited = int.MaxValue;

    public bool Includes(Feature feature) => Features.Contains(feature);
}

/// <summary>Maps each <see cref="SubscriptionPlan"/> to its limits and default features.</summary>
public static class PlanCatalog
{
    private static readonly Dictionary<SubscriptionPlan, PlanLimits> Plans =
        new()
        {
            [SubscriptionPlan.Basic] = new(
                MaxRooms: 20, MaxUsers: 5, MaxServiceItems: 20,
                Features: new HashSet<Feature> { Feature.DynamicPricing }),

            [SubscriptionPlan.Professional] = new(
                MaxRooms: 100, MaxUsers: 25, MaxServiceItems: 100,
                Features: new HashSet<Feature> { Feature.DynamicPricing, Feature.PublicApi, Feature.AdvancedReports }),

            [SubscriptionPlan.Enterprise] = new(
                MaxRooms: PlanLimits.Unlimited, MaxUsers: PlanLimits.Unlimited, MaxServiceItems: PlanLimits.Unlimited,
                Features: new HashSet<Feature>
                {
                    Feature.DynamicPricing, Feature.PublicApi, Feature.AdvancedReports,
                    Feature.LoyaltyProgram, Feature.MultiCurrency,
                }),
        };

    public static PlanLimits For(SubscriptionPlan plan) => Plans[plan];

    public static SubscriptionPlan RequiredPlanFor(Feature feature)
        => Plans
            .Where(plan => plan.Value.Includes(feature))
            .OrderBy(plan => plan.Key)
            .Select(plan => plan.Key)
            .First();
}
