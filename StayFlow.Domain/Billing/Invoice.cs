using StayFlow.Domain.Billing.Events;
using StayFlow.Domain.Common;

namespace StayFlow.Domain.Billing;

/// <summary>
/// Invoice aggregate for a reservation. Owns its line items and a Draft → Issued → Paid (or Void)
/// lifecycle. Totals are derived from the lines so they can never drift out of sync.
/// </summary>
public sealed class Invoice : TenantEntity
{
    private readonly List<InvoiceLineItem> _lineItems = [];

    private Invoice()
    {
    }

    private Invoice(Guid reservationId, string number, string currency, decimal taxRate)
    {
        ReservationId = reservationId;
        Number = number;
        Currency = currency;
        TaxRate = taxRate;
        Status = InvoiceStatus.Draft;
    }

    public Guid ReservationId { get; private set; }

    public string Number { get; private set; } = string.Empty;

    public InvoiceStatus Status { get; private set; }

    public string Currency { get; private set; } = "USD";

    public decimal TaxRate { get; private set; }

    public DateTimeOffset? IssuedAtUtc { get; private set; }

    public DateTimeOffset? DueAtUtc { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    public decimal Subtotal => _lineItems.Where(l => l.Kind != LineItemKind.Tax).Sum(l => l.Amount);

    public decimal TaxTotal => _lineItems.Where(l => l.Kind == LineItemKind.Tax).Sum(l => l.Amount);

    public decimal Total => _lineItems.Sum(l => l.Amount);

    public static Invoice Create(Guid reservationId, string number, string currency, decimal taxRate)
    {
        if (reservationId == Guid.Empty)
        {
            throw new DomainException("Invoice must reference a reservation.");
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("Invoice number is required.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
        {
            throw new DomainException("Invoice currency must be a 3-letter ISO 4217 code.");
        }

        if (taxRate is < 0 or > 1)
        {
            throw new DomainException("Tax rate must be between 0 and 1.");
        }

        return new Invoice(reservationId, number.Trim(), currency.ToUpperInvariant(), taxRate);
    }

    public void AddLine(LineItemKind kind, string description, int quantity, decimal unitPrice)
    {
        EnsureDraft();

        if (kind == LineItemKind.Tax)
        {
            throw new DomainException("Tax lines are managed via ApplyTax.");
        }

        if (quantity < 1)
        {
            throw new DomainException("Line quantity must be at least 1.");
        }

        _lineItems.Add(new InvoiceLineItem(kind, description, quantity, unitPrice));
    }

    /// <summary>Recomputes the single tax line from the current taxable subtotal.</summary>
    public void ApplyTax()
    {
        EnsureDraft();
        _lineItems.RemoveAll(l => l.Kind == LineItemKind.Tax);

        if (TaxRate <= 0)
        {
            return;
        }

        var tax = Math.Round(Subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        _lineItems.Add(new InvoiceLineItem(LineItemKind.Tax, $"Tax ({TaxRate:P0})", 1, tax));
    }

    public void Issue(DateTimeOffset now, int dueInDays = 14)
    {
        EnsureDraft();

        if (_lineItems.Count == 0)
        {
            throw new DomainException("Cannot issue an invoice with no lines.");
        }

        Status = InvoiceStatus.Issued;
        IssuedAtUtc = now;
        DueAtUtc = now.AddDays(dueInDays);
        RaiseDomainEvent(new InvoiceGeneratedEvent(Id, TenantId, ReservationId, Total));
    }

    public void MarkPaid(DateTimeOffset now)
    {
        if (Status != InvoiceStatus.Issued)
        {
            throw new DomainException($"Only issued invoices can be paid (current: {Status}).");
        }

        Status = InvoiceStatus.Paid;
        PaidAtUtc = now;
        RaiseDomainEvent(new InvoicePaidEvent(Id, TenantId, Total));
    }

    public void Void()
    {
        if (Status == InvoiceStatus.Paid)
        {
            throw new DomainException("A paid invoice cannot be voided.");
        }

        Status = InvoiceStatus.Void;
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
        {
            throw new DomainException($"Invoice can only be modified while in Draft (current: {Status}).");
        }
    }
}
