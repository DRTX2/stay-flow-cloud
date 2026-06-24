using StayFlow.Domain.Common;

namespace StayFlow.Domain.Tenants;

/// <summary>
/// A customer account on the platform (a hotel business, hostel chain, resort, etc.).
/// The tenant is the isolation boundary: every <see cref="TenantEntity"/> belongs to one
/// tenant and is segregated by <c>TenantId</c>.
/// </summary>
public sealed class Tenant : AuditableEntity
{
    private Tenant()
    {
    }

    private Tenant(Guid id, string name, string slug, PropertyType propertyType, string defaultCurrency)
        : base(id)
    {
        Name = name;
        Slug = slug;
        PropertyType = propertyType;
        DefaultCurrency = defaultCurrency;
        Plan = SubscriptionPlan.Basic;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>URL/host-safe identifier used to resolve the tenant from a request.</summary>
    public string Slug { get; private set; } = string.Empty;

    public PropertyType PropertyType { get; private set; }

    public SubscriptionPlan Plan { get; private set; }

    /// <summary>ISO 4217 currency code applied to pricing and invoices.</summary>
    public string DefaultCurrency { get; private set; } = "USD";

    public bool IsActive { get; private set; }

    public static Tenant Create(string name, string slug, PropertyType propertyType, string defaultCurrency = "USD")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Tenant name is required.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Tenant slug is required.");
        }

        if (string.IsNullOrWhiteSpace(defaultCurrency) || defaultCurrency.Length != 3)
        {
            throw new DomainException("Tenant currency must be a 3-letter ISO 4217 code.");
        }

        return new Tenant(Guid.CreateVersion7(), name.Trim(), slug.Trim().ToLowerInvariant(), propertyType, defaultCurrency.ToUpperInvariant());
    }

    public void ChangePlan(SubscriptionPlan plan) => Plan = plan;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
