namespace StayFlow.Contracts;

/// <summary>
/// Integration-event envelope published to the bus for every domain event drained from the outbox.
/// Transport-neutral and self-describing (carries the originating event's type and JSON payload plus
/// tenant/user context) so out-of-process consumers — notifications, ERP/CRM sync, analytics — can
/// react without referencing the domain model.
/// </summary>
/// <remarks>
/// This type lives in a shared contracts assembly referenced by both the publisher (the monolith)
/// and consumers (microservices). MassTransit derives the message URN from the namespace + name, so
/// both sides must use this exact type for routing to work.
/// </remarks>
public sealed record DomainEventOccurred(
    Guid Id,
    Guid? TenantId,
    Guid? UserId,
    string EventType,
    string Payload,
    DateTimeOffset OccurredOnUtc);
