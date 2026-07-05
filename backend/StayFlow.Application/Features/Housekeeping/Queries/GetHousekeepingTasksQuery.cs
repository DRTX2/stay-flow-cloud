using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Models;
using StayFlow.Domain.Housekeeping;

namespace StayFlow.Application.Features.Housekeeping.Queries;

public sealed record GetHousekeepingTasksQuery(int Page = 1, int PageSize = 20, HousekeepingTaskStatus? Status = null) : IRequest<PagedResult<HousekeepingTaskDto>>;

internal sealed class GetHousekeepingTasksQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetHousekeepingTasksQuery, PagedResult<HousekeepingTaskDto>>
{
    public async Task<PagedResult<HousekeepingTaskDto>> Handle(GetHousekeepingTasksQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.HousekeepingTasks.AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new HousekeepingTaskDto(
                t.Id, t.RoomId, t.TaskType, t.Status.ToString(), t.AssignedToId, t.Notes, t.CreatedAtUtc, t.CompletedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<HousekeepingTaskDto>(items, total, request.Page, request.PageSize);
    }
}
