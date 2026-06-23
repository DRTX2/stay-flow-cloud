namespace StayFlow.Infrastructure.Messaging;

/// <summary>
/// The integration-event envelope published to the bus for every domain event drained from the
/// outbox. Deliberately transport-neutral and self-describing (carries the originating event's type
/// and JSON payload plus the tenant/user context) so downstream consumers — notifications, ERP/CRM
/// sync, analytics — can react without referencing the domain model.
/// </summary>
public sealed record DomainEventOccurred(
    Guid Id,
    Guid? TenantId,
    Guid? UserId,
    string EventType,
    string Payload,
    DateTimeOffset OccurredOnUtc);
