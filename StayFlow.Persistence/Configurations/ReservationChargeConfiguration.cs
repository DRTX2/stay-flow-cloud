using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Billing;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Services;

namespace StayFlow.Persistence.Configurations;

internal sealed class ReservationChargeConfiguration : IEntityTypeConfiguration<ReservationCharge>
{
    public void Configure(EntityTypeBuilder<ReservationCharge> builder)
    {
        builder.ToTable("ReservationCharges");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Description).IsRequired().HasMaxLength(120);
        builder.Property(c => c.UnitPrice).HasPrecision(18, 2);
        builder.Ignore(c => c.Amount);

        builder.HasIndex(c => new { c.TenantId, c.ReservationId });

        builder.HasOne<Reservation>()
            .WithMany()
            .HasForeignKey(c => c.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ServiceItem>()
            .WithMany()
            .HasForeignKey(c => c.ServiceItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
