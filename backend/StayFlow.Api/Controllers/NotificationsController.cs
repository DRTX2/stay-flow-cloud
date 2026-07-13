using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Infrastructure.Observability;
using StayFlow.Persistence;

namespace StayFlow.Api.Controllers;

[Authorize]
public sealed class NotificationsController(
    StayFlowDbContext dbContext,
    ICurrentUser currentUser,
    StayFlowMetrics metrics) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationListResponse>> List(CancellationToken cancellationToken)
    {
        if (!TryGetScope(out var tenantId, out var userId))
        {
            return Forbid();
        }

        var scoped = dbContext.InAppNotifications
            .Where(notification => notification.TenantId == tenantId && notification.UserId == userId);
        var items = await scoped
            .OrderByDescending(notification => notification.CreatedAtUtc)
            .Take(50)
            .Select(notification => new NotificationDto(
                notification.Id,
                notification.Title,
                notification.Body,
                notification.Type,
                notification.Link,
                notification.CreatedAtUtc,
                notification.ReadAtUtc))
            .ToListAsync(cancellationToken);
        var unreadCount = await scoped.CountAsync(notification => notification.ReadAtUtc == null, cancellationToken);

        metrics.RecordBusinessEvent("notifications_listed");
        return Ok(new NotificationListResponse(items, unreadCount));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetScope(out var tenantId, out var userId))
        {
            return Forbid();
        }

        var notification = await dbContext.InAppNotifications.SingleOrDefaultAsync(
            item => item.Id == id && item.TenantId == tenantId && item.UserId == userId,
            cancellationToken);
        if (notification is null)
        {
            return NotFound();
        }

        if (notification.ReadAtUtc is null)
        {
            notification.ReadAtUtc = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        metrics.RecordBusinessEvent("notification_marked_read");
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!TryGetScope(out var tenantId, out var userId))
        {
            return Forbid();
        }

        await dbContext.InAppNotifications
            .Where(notification => notification.TenantId == tenantId
                && notification.UserId == userId
                && notification.ReadAtUtc == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(notification => notification.ReadAtUtc, DateTimeOffset.UtcNow),
                cancellationToken);

        metrics.RecordBusinessEvent("notifications_marked_read");
        return NoContent();
    }

    private bool TryGetScope(out Guid tenantId, out Guid userId)
    {
        tenantId = currentUser.TenantId.GetValueOrDefault();
        userId = currentUser.UserId.GetValueOrDefault();
        return currentUser.IsAuthenticated && tenantId != Guid.Empty && userId != Guid.Empty;
    }

    public sealed record NotificationDto(
        Guid Id,
        string Title,
        string Body,
        string Type,
        string? Link,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset? ReadAtUtc);

    public sealed record NotificationListResponse(IReadOnlyList<NotificationDto> Items, int UnreadCount);
}
