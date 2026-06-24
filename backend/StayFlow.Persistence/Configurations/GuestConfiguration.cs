using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Guests;

namespace StayFlow.Persistence.Configurations;

internal sealed class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("Guests");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(g => g.LastName).IsRequired().HasMaxLength(100);
        builder.Property(g => g.Email).IsRequired().HasMaxLength(256);
        builder.Property(g => g.Phone).HasMaxLength(40);
        builder.Property(g => g.DocumentNumber).HasMaxLength(60);

        builder.Ignore(g => g.FullName);

        builder.HasIndex(g => new { g.TenantId, g.Email });
    }
}
