using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Tenants;

namespace StayFlow.Persistence.Configurations;

internal sealed class TenantFeatureOverrideConfiguration : IEntityTypeConfiguration<TenantFeatureOverride>
{
    public void Configure(EntityTypeBuilder<TenantFeatureOverride> builder)
    {
        builder.ToTable("TenantFeatureOverrides");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Feature).HasConversion<string>().HasMaxLength(40);

        builder.HasIndex(f => new { f.TenantId, f.Feature }).IsUnique();
    }
}
