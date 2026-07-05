using StayFlow.Domain.Common;

namespace StayFlow.Domain.Maintenance.Events;

public sealed record WorkOrderCreatedEvent(Guid WorkOrderId, Guid TenantId, string Description, WorkOrderPriority Priority) : DomainEvent;

public sealed record WorkOrderResolvedEvent(Guid WorkOrderId, Guid TenantId) : DomainEvent;
