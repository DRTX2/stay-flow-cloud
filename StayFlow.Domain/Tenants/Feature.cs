namespace StayFlow.Domain.Tenants;

/// <summary>Toggleable product capabilities. Availability is driven by the subscription plan and
/// can be overridden per tenant.</summary>
public enum Feature
{
    DynamicPricing = 1,
    PublicApi = 2,
    AdvancedReports = 3,
    LoyaltyProgram = 4,
    MultiCurrency = 5,
}
