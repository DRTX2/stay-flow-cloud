using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Models;
using StayFlow.Domain.Orders;

namespace StayFlow.Application.Features.Orders.Queries;

public sealed record GetOrdersQuery(int Page = 1, int PageSize = 20, OrderStatus? Status = null) : IRequest<PagedResult<OrderDto>>;

internal sealed class GetOrdersQueryHandler(IApplicationDbContext dbContext) : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking();

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderDto(
                o.Id, o.ReservationId, o.Status.ToString(), o.Notes, o.TotalAmount, o.CreatedAtUtc, o.DeliveredAtUtc,
                o.Items.Select(i => new OrderLineItemDto(i.ServiceItemId, i.ServiceName, i.Quantity, i.UnitPrice, i.Total)).ToList()))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>(items, total, request.Page, request.PageSize);
    }
}
