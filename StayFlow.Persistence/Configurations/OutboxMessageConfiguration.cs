using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Persistence.Outbox;

namespace StayFlow.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).HasMaxLength(512).IsRequired();
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.Error).HasMaxLength(2048);

        // Drives the relay query: fetch the unprocessed backlog in arrival order.
        builder.HasIndex(m => new { m.ProcessedOnUtc, m.OccurredOnUtc });
    }
}
