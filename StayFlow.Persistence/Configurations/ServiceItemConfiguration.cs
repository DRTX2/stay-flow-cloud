using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Services;

namespace StayFlow.Persistence.Configurations;

internal sealed class ServiceItemConfiguration : IEntityTypeConfiguration<ServiceItem>
{
    public void Configure(EntityTypeBuilder<ServiceItem> builder)
    {
        builder.ToTable("ServiceItems");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(120);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.Price).HasPrecision(18, 2);
        builder.Property(s => s.Category).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(s => new { s.TenantId, s.Name }).IsUnique();
    }
}
