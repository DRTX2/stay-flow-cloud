using StayFlow.Domain.Common;

namespace StayFlow.Domain.Reservations.Events;

public sealed record ReservationCreatedEvent(Guid ReservationId, Guid TenantId, Guid RoomId, Guid GuestId, decimal TotalPrice) : DomainEvent;

public sealed record ReservationConfirmedEvent(Guid ReservationId, Guid TenantId) : DomainEvent;

public sealed record ReservationCheckedInEvent(Guid ReservationId, Guid TenantId) : DomainEvent;

public sealed record ReservationCheckedOutEvent(Guid ReservationId, Guid TenantId) : DomainEvent;

public sealed record ReservationCancelledEvent(Guid ReservationId, Guid TenantId, string? Reason) : DomainEvent;
