using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Application.Features.Services.Queries;

public sealed record GetServiceItemsQuery(bool ActiveOnly = false) : IRequest<IReadOnlyList<ServiceItemDto>>;

public sealed class GetServiceItemsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetServiceItemsQuery, IReadOnlyList<ServiceItemDto>>
{
    public async Task<IReadOnlyList<ServiceItemDto>> Handle(GetServiceItemsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.ServiceItems.AsNoTracking();
        if (request.ActiveOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        return await query
            .OrderBy(s => s.Category).ThenBy(s => s.Name)
            .Select(s => new ServiceItemDto(s.Id, s.Name, s.Description, s.Price, s.Category, s.IsActive))
            .ToListAsync(cancellationToken);
    }
}
