using StayFlow.Domain.Billing;

namespace StayFlow.Application.Features.Billing;

public sealed record InvoiceLineItemDto(
    LineItemKind Kind,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Amount);

public sealed record InvoiceDto(
    Guid Id,
    string Number,
    Guid ReservationId,
    InvoiceStatus Status,
    string Currency,
    decimal Subtotal,
    decimal TaxTotal,
    decimal Total,
    DateTimeOffset? IssuedAtUtc,
    DateTimeOffset? DueAtUtc,
    DateTimeOffset? PaidAtUtc,
    IReadOnlyList<InvoiceLineItemDto> LineItems)
{
    public static InvoiceDto FromEntity(Invoice invoice) => new(
        invoice.Id,
        invoice.Number,
        invoice.ReservationId,
        invoice.Status,
        invoice.Currency,
        invoice.Subtotal,
        invoice.TaxTotal,
        invoice.Total,
        invoice.IssuedAtUtc,
        invoice.DueAtUtc,
        invoice.PaidAtUtc,
        invoice.LineItems
            .Select(l => new InvoiceLineItemDto(l.Kind, l.Description, l.Quantity, l.UnitPrice, l.Amount))
            .ToList());
}
