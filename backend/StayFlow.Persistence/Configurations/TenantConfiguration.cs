using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Tenants;

namespace StayFlow.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(100);
        builder.Property(t => t.DefaultCurrency).IsRequired().HasMaxLength(3).IsFixedLength();
        builder.Property(t => t.PropertyType).HasConversion<string>().HasMaxLength(30);
        builder.Property(t => t.Plan).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(t => t.Slug).IsUnique();
    }
}
