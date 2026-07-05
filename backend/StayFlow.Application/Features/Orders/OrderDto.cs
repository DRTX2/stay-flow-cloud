namespace StayFlow.Application.Features.Orders;

public sealed record OrderLineItemDto(
    Guid ServiceItemId,
    string ServiceName,
    int Quantity,
    decimal UnitPrice,
    decimal Total);

public sealed record OrderDto(
    Guid Id,
    Guid ReservationId,
    string Status,
    string? Notes,
    decimal TotalAmount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? DeliveredAtUtc,
    IReadOnlyCollection<OrderLineItemDto> Items);
