using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.BookingEnquiries;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;

namespace StayFlow.Persistence.Configurations;

internal sealed class BookingEnquiryConfiguration : IEntityTypeConfiguration<BookingEnquiry>
{
    public void Configure(EntityTypeBuilder<BookingEnquiry> builder)
    {
        builder.ToTable("BookingEnquiries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Reference).IsRequired().HasMaxLength(30);
        builder.Property(e => e.CheckIn).HasColumnType("date");
        builder.Property(e => e.CheckOut).HasColumnType("date");
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(120);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Phone).HasMaxLength(40);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.RejectionReason).HasMaxLength(500);

        builder.HasIndex(e => e.Reference).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.Status, e.CreatedAtUtc });
        builder.HasIndex(e => new { e.TenantId, e.Email });

        builder.HasOne<RoomType>().WithMany().HasForeignKey(e => e.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Reservation>().WithMany().HasForeignKey(e => e.ReservationId).OnDelete(DeleteBehavior.Restrict);
    }
}
