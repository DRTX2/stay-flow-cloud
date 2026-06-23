using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Models;

namespace StayFlow.Application.Features.Guests.Queries;

public sealed record GetGuestsQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<PagedResult<GuestDto>>;

public sealed class GetGuestsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetGuestsQuery, PagedResult<GuestDto>>
{
    public async Task<PagedResult<GuestDto>> Handle(GetGuestsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = dbContext.Guests.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(g =>
                EF.Functions.Like(g.FirstName, pattern) ||
                EF.Functions.Like(g.LastName, pattern) ||
                EF.Functions.Like(g.Email, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(g => g.LastName).ThenBy(g => g.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GuestDto(g.Id, g.FirstName, g.LastName, g.Email, g.Phone, g.DocumentNumber))
            .ToListAsync(cancellationToken);

        return new PagedResult<GuestDto>(items, page, pageSize, totalCount);
    }
}
