using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Rooms;

namespace StayFlow.Persistence.Configurations;

internal sealed class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("RoomTypes");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Name).IsRequired().HasMaxLength(100);
        builder.Property(rt => rt.Description).HasMaxLength(1000);
        builder.Property(rt => rt.BaseRate).HasPrecision(18, 2);

        builder.HasIndex(rt => new { rt.TenantId, rt.Name }).IsUnique();
    }
}
