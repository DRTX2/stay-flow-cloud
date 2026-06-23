using StayFlow.Domain.Common;

namespace StayFlow.Domain.Tenants;

/// <summary>A per-tenant override of a feature's default (plan-derived) state.</summary>
public sealed class TenantFeatureOverride : TenantEntity
{
    private TenantFeatureOverride()
    {
    }

    private TenantFeatureOverride(Feature feature, bool isEnabled)
    {
        Feature = feature;
        IsEnabled = isEnabled;
    }

    public Feature Feature { get; private set; }

    public bool IsEnabled { get; private set; }

    public static TenantFeatureOverride Create(Feature feature, bool isEnabled) => new(feature, isEnabled);

    public void Set(bool isEnabled) => IsEnabled = isEnabled;
}
