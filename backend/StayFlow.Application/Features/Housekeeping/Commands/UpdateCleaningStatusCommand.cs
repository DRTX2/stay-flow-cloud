using MediatR;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Housekeeping.Commands;

public sealed record UpdateCleaningStatusCommand(Guid RoomId, RoomCleaningStatus Status) : IRequest;

internal sealed class UpdateCleaningStatusCommandHandler(IApplicationDbContext dbContext) : IRequestHandler<UpdateCleaningStatusCommand>
{
    public async Task Handle(UpdateCleaningStatusCommand request, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.FindAsync([request.RoomId], cancellationToken)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        room.UpdateCleaningStatus(request.Status);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
