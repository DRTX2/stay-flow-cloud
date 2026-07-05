using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Models;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Rooms.Queries;

public sealed record GetRoomsQuery(int Page = 1, int PageSize = 20, RoomStatus? Status = null)
    : IRequest<PagedResult<RoomDto>>;

public sealed class GetRoomsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRoomsQuery, PagedResult<RoomDto>>
{
    public async Task<PagedResult<RoomDto>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = dbContext.Rooms.AsNoTracking();
        if (request.Status is { } status)
        {
            query = query.Where(r => r.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(room => room.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(room => new RoomDto(
                room.Id,
                room.Number,
                room.RoomTypeId,
                dbContext.RoomTypes
                    .Where(roomType => roomType.Id == room.RoomTypeId)
                    .Select(roomType => roomType.Name)
                    .FirstOrDefault()!,
                room.BasePrice,
                room.Capacity,
                room.Floor,
                room.Status,
                room.CleaningStatus))
            .ToListAsync(cancellationToken);

        return new PagedResult<RoomDto>(items, page, pageSize, totalCount);
    }
}
