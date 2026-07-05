using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Models;
using StayFlow.Domain.Maintenance;

namespace StayFlow.Application.Features.Maintenance.Queries;

public sealed record GetWorkOrdersQuery(int Page = 1, int PageSize = 20, WorkOrderStatus? Status = null) : IRequest<PagedResult<WorkOrderDto>>;

internal sealed class GetWorkOrdersQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetWorkOrdersQuery, PagedResult<WorkOrderDto>>
{
    public async Task<PagedResult<WorkOrderDto>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.WorkOrders.AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(w => w.Status == request.Status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(w => w.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WorkOrderDto(
                w.Id, w.RoomId, w.Description, w.Priority.ToString(), w.Status.ToString(),
                w.ReportedById, w.AssignedToId, w.ResolutionNotes, w.CreatedAtUtc, w.ResolvedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkOrderDto>(items, total, request.Page, request.PageSize);
    }
}
