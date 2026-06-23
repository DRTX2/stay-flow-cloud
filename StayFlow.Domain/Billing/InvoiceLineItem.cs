using StayFlow.Domain.Common;

namespace StayFlow.Domain.Billing;

/// <summary>A single line on an <see cref="Invoice"/>. Created and owned by the invoice aggregate.</summary>
public sealed class InvoiceLineItem : Entity
{
    private InvoiceLineItem()
    {
    }

    internal InvoiceLineItem(LineItemKind kind, string description, int quantity, decimal unitPrice)
    {
        Kind = kind;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid InvoiceId { get; private set; }

    public LineItemKind Kind { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal Amount => UnitPrice * Quantity;
}
