using StayFlow.Domain.Tenants;

namespace StayFlow.Application.Features.Tenants;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    PropertyType PropertyType,
    SubscriptionPlan Plan,
    string DefaultCurrency,
    bool IsActive)
{
    public static TenantDto FromEntity(Tenant tenant) => new(
        tenant.Id,
        tenant.Name,
        tenant.Slug,
        tenant.PropertyType,
        tenant.Plan,
        tenant.DefaultCurrency,
        tenant.IsActive);
}
