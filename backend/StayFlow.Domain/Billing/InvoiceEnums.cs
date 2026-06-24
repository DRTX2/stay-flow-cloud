namespace StayFlow.Domain.Billing;

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    Paid = 3,
    Void = 4,
}

/// <summary>The nature of an invoice line, used for grouping and tax treatment.</summary>
public enum LineItemKind
{
    Accommodation = 1,
    Service = 2,
    Tax = 3,
    Discount = 4,
}
