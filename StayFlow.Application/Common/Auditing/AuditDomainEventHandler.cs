using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Events;
using StayFlow.Domain.Common;

namespace StayFlow.Application.Common.Auditing;

/// <summary>
/// Records every published domain event to the audit/event store, tagged with the acting tenant
/// and user. Failures here never break the originating request — auditing is best-effort.
/// </summary>
public sealed class DomainEventAuditor<TDomainEvent>(
    IAuditStore store,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser,
    ILogger<DomainEventAuditor<TDomainEvent>> logger)
    : INotificationHandler<DomainEventNotification<TDomainEvent>>
    where TDomainEvent : IDomainEvent
{
    public async Task Handle(DomainEventNotification<TDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var record = new AuditRecord(
                Id: Guid.CreateVersion7(),
                TenantId: tenantProvider.TenantId,
                UserId: currentUser.UserId,
                EventType: typeof(TDomainEvent).Name,
                OccurredOnUtc: domainEvent.OccurredOnUtc,
                Payload: JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));

            await store.AppendAsync(record, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write audit record for {EventType}", typeof(TDomainEvent).Name);
        }
    }
}
