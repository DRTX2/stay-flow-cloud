using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Application.Features.RoomTypes.Queries;

public sealed record GetRoomTypesQuery : IRequest<IReadOnlyList<RoomTypeDto>>;

public sealed class GetRoomTypesHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRoomTypesQuery, IReadOnlyList<RoomTypeDto>>
{
    public async Task<IReadOnlyList<RoomTypeDto>> Handle(GetRoomTypesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.RoomTypes
            .AsNoTracking()
            .OrderBy(rt => rt.Name)
            .Select(rt => new RoomTypeDto(rt.Id, rt.Name, rt.Description, rt.BaseRate, rt.MaxOccupancy))
            .ToListAsync(cancellationToken);
    }
}
