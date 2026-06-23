using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StayFlow.Domain.Billing;

namespace StayFlow.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Number).IsRequired().HasMaxLength(40);
        builder.Property(i => i.Currency).IsRequired().HasMaxLength(3);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.TaxRate).HasPrecision(5, 4);
        builder.Ignore(i => i.Subtotal);
        builder.Ignore(i => i.TaxTotal);
        builder.Ignore(i => i.Total);

        builder.HasIndex(i => i.Number).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.ReservationId });

        builder.HasMany(i => i.LineItems)
            .WithOne()
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Invoice.LineItems))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("InvoiceLineItems");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Description).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 2);
        builder.Ignore(l => l.Amount);
    }
}
