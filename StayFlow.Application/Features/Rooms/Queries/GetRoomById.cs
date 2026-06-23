using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Rooms.Queries;

public sealed record GetRoomByIdQuery(Guid Id) : IRequest<RoomDto>;

public sealed class GetRoomByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetRoomByIdQuery, RoomDto>
{
    public async Task<RoomDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await dbContext.Rooms
            .AsNoTracking()
            .Where(r => r.Id == request.Id)
            .Join(
                dbContext.RoomTypes,
                room => room.RoomTypeId,
                roomType => roomType.Id,
                (room, roomType) => new RoomDto(
                    room.Id,
                    room.Number,
                    room.RoomTypeId,
                    roomType.Name,
                    room.BasePrice,
                    room.Capacity,
                    room.Floor,
                    room.Status))
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new NotFoundException(nameof(Room), request.Id);
    }
}
