using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Common;

namespace StayFlow.Persistence.Interceptors;

/// <summary>
/// Stamps audit metadata, assigns the tenant on new tenant-scoped rows, and converts hard
/// deletes of soft-deletable entities into soft deletes — all just before changes are saved.
/// </summary>
public sealed class AuditableEntityInterceptor(
    ICurrentUser currentUser,
    IDateTimeProvider clock,
    ITenantProvider tenantProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            Apply(eventData.Context);
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
            Apply(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext context)
    {
        var now = clock.UtcNow;
        var user = currentUser.UserId?.ToString();

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            StampTenant(entry);
            StampAudit(entry, now, user);
            ConvertToSoftDelete(entry, now, user);
        }
    }

    private void StampTenant(EntityEntry<Entity> entry)
    {
        if (entry.State == EntityState.Added
            && entry.Entity is ITenantScoped scoped
            && scoped.TenantId == Guid.Empty
            && tenantProvider.TenantId is { } tenantId
            && tenantId != Guid.Empty)
        {
            entry.Property(nameof(ITenantScoped.TenantId)).CurrentValue = tenantId;
        }
    }

    private static void StampAudit(EntityEntry<Entity> entry, DateTimeOffset now, string? user)
    {
        if (entry.Entity is not IAuditable auditable)
        {
            return;
        }

        switch (entry.State)
        {
            case EntityState.Added:
                auditable.CreatedAtUtc = now;
                auditable.CreatedBy = user;
                break;
            case EntityState.Modified:
                auditable.UpdatedAtUtc = now;
                auditable.UpdatedBy = user;
                break;
            default:
                break;
        }
    }

    private static void ConvertToSoftDelete(EntityEntry<Entity> entry, DateTimeOffset now, string? user)
    {
        if (entry.State != EntityState.Deleted || entry.Entity is not ISoftDeletable softDeletable)
        {
            return;
        }

        entry.State = EntityState.Modified;
        softDeletable.IsDeleted = true;
        softDeletable.DeletedAtUtc = now;
        softDeletable.DeletedBy = user;
    }
}
