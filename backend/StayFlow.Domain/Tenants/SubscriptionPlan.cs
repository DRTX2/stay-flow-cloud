namespace StayFlow.Domain.Tenants;

/// <summary>Tenant subscription tier. Drives feature flags and usage limits.</summary>
public enum SubscriptionPlan
{
    Basic = 1,
    Professional = 2,
    Enterprise = 3,
}
