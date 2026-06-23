using StayFlow.Domain.Common;

namespace StayFlow.Domain.Billing.Events;

public sealed record InvoiceGeneratedEvent(Guid InvoiceId, Guid TenantId, Guid ReservationId, decimal Total) : DomainEvent;

public sealed record InvoicePaidEvent(Guid InvoiceId, Guid TenantId, decimal Total) : DomainEvent;
