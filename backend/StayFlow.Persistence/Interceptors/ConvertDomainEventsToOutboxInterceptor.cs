using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Common;
using StayFlow.Persistence.Outbox;

namespace StayFlow.Persistence.Interceptors;

/// <summary>
/// Before changes are committed, serializes each buffered domain event into an
/// <see cref="OutboxMessage"/> row added to the same unit of work. This makes event capture atomic
/// with the aggregate change (transactional outbox), so nothing is lost if the process dies before
/// the bus is reached. Domain events are left intact for the in-process MediatR dispatch that runs
/// post-commit; the outbox handles only out-of-process (integration) relay.
/// </summary>
public sealed class ConvertDomainEventsToOutboxInterceptor(
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            AddOutboxMessages(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            AddOutboxMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddOutboxMessages(DbContext context)
    {
        // Capture the request's tenant/user now, while they're still resolvable — the relay later
        // runs on a background thread with no HTTP context.
        var tenantId = tenantProvider.TenantId;
        var userId = currentUser.UserId;

        var messages = context.ChangeTracker
            .Entries<Entity>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .SelectMany(entry => entry.Entity.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                TenantId = tenantId,
                UserId = userId,
                Type = domainEvent.GetType().FullName!,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredOnUtc = domainEvent.OccurredOnUtc,
            })
            .ToList();

        if (messages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(messages);
        }
    }
}
