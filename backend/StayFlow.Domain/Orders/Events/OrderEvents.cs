using StayFlow.Domain.Common;

namespace StayFlow.Domain.Orders.Events;

public sealed record OrderPlacedEvent(Guid OrderId, Guid TenantId, Guid ReservationId, decimal TotalAmount) : DomainEvent;

public sealed record OrderDeliveredEvent(Guid OrderId, Guid TenantId) : DomainEvent;
