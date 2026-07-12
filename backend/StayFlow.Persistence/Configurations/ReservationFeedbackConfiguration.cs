using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Feedback;
using StayFlow.Domain.Reservations;

namespace StayFlow.Persistence.Configurations;

internal sealed class ReservationFeedbackConfiguration : IEntityTypeConfiguration<ReservationFeedback>
{
    public void Configure(EntityTypeBuilder<ReservationFeedback> builder)
    {
        builder.ToTable("ReservationFeedback", table =>
            table.HasCheckConstraint("CK_ReservationFeedback_Rating", "\"Rating\" IS NULL OR (\"Rating\" >= 1 AND \"Rating\" <= 5)"));
        builder.HasKey(feedback => feedback.Id);
        builder.Property(feedback => feedback.InvitationTokenHash).IsRequired().HasMaxLength(64);
        builder.Property(feedback => feedback.Comment).HasMaxLength(2000);
        builder.HasIndex(feedback => feedback.InvitationTokenHash).IsUnique();
        builder.HasIndex(feedback => new { feedback.TenantId, feedback.ReservationId }).IsUnique();
        builder.HasOne<Reservation>().WithMany().HasForeignKey(feedback => feedback.ReservationId).OnDelete(DeleteBehavior.Restrict);
    }
}
