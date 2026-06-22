using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;

namespace StayFlow.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TotalPrice).HasPrecision(18, 2);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.ConfirmationCode).IsRequired().HasMaxLength(20);
        builder.Property(r => r.CancellationReason).HasMaxLength(500);

        // Stay period as an owned value object materialised through its private constructor.
        builder.OwnsOne(r => r.Period, period =>
        {
            period.Property(p => p.CheckIn).HasColumnName("CheckIn").HasColumnType("date").IsRequired();
            period.Property(p => p.CheckOut).HasColumnName("CheckOut").HasColumnType("date").IsRequired();
            period.Ignore(p => p.Nights);
        });
        builder.Navigation(r => r.Period).IsRequired();

        builder.HasIndex(r => r.ConfirmationCode).IsUnique();
        builder.HasIndex(r => new { r.TenantId, r.RoomId });
        builder.HasIndex(r => new { r.TenantId, r.GuestId });

        builder.HasOne<Room>()
            .WithMany()
            .HasForeignKey(r => r.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Guest>()
            .WithMany()
            .HasForeignKey(r => r.GuestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
