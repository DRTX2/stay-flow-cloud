using FluentAssertions;
using StayFlow.Domain.Billing;
using StayFlow.Domain.Billing.Events;
using StayFlow.Domain.Common;

namespace StayFlow.UnitTests.Domain;

public sealed class InvoiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

    private static Invoice DraftWithAccommodation(decimal taxRate = 0.10m)
    {
        var invoice = Invoice.Create(Guid.NewGuid(), "INV-1", "USD", taxRate);
        invoice.AddLine(LineItemKind.Accommodation, "Stay", 1, 200m);
        return invoice;
    }

    [Fact]
    public void ApplyTax_ComputesTaxLineFromTaxableSubtotal()
    {
        var invoice = DraftWithAccommodation();
        invoice.AddLine(LineItemKind.Service, "Breakfast", 2, 18.50m); // +37 => subtotal 237

        invoice.ApplyTax();

        invoice.Subtotal.Should().Be(237m);
        invoice.TaxTotal.Should().Be(23.70m);
        invoice.Total.Should().Be(260.70m);
    }

    [Fact]
    public void ApplyTax_IsIdempotent_DoesNotStackTaxLines()
    {
        var invoice = DraftWithAccommodation();

        invoice.ApplyTax();
        invoice.ApplyTax();

        invoice.LineItems.Count(l => l.Kind == LineItemKind.Tax).Should().Be(1);
    }

    [Fact]
    public void Issue_SetsIssuedAndRaisesEvent()
    {
        var invoice = DraftWithAccommodation();
        invoice.ApplyTax();

        invoice.Issue(Now);

        invoice.Status.Should().Be(InvoiceStatus.Issued);
        invoice.IssuedAtUtc.Should().Be(Now);
        invoice.DueAtUtc.Should().Be(Now.AddDays(14));
        invoice.DomainEvents.Should().ContainSingle(e => e is InvoiceGeneratedEvent);
    }

    [Fact]
    public void AddLine_AfterIssue_Throws()
    {
        var invoice = DraftWithAccommodation();
        invoice.Issue(Now);

        invoice.Invoking(i => i.AddLine(LineItemKind.Service, "Late", 1, 10m))
            .Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkPaid_OnlyFromIssued()
    {
        var draft = DraftWithAccommodation();
        draft.Invoking(i => i.MarkPaid(Now)).Should().Throw<DomainException>();

        draft.Issue(Now);
        draft.MarkPaid(Now);
        draft.Status.Should().Be(InvoiceStatus.Paid);
        draft.DomainEvents.Should().Contain(e => e is InvoicePaidEvent);
    }

    [Fact]
    public void Void_PaidInvoice_Throws()
    {
        var invoice = DraftWithAccommodation();
        invoice.Issue(Now);
        invoice.MarkPaid(Now);

        invoice.Invoking(i => i.Void()).Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.5)]
    public void Create_InvalidTaxRate_Throws(decimal taxRate)
    {
        var act = () => Invoice.Create(Guid.NewGuid(), "INV-1", "USD", taxRate);

        act.Should().Throw<DomainException>();
    }
}
