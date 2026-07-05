using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Housekeeping;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Housekeeping.Commands;

public sealed record CompleteHousekeepingTaskCommand(Guid Id, RoomCleaningStatus? CleaningStatus = null) : IRequest;

internal sealed class CompleteHousekeepingTaskCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CompleteHousekeepingTaskCommand>
{
    public async Task Handle(CompleteHousekeepingTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await dbContext.HousekeepingTasks.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(HousekeepingTask), request.Id);
        var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == task.RoomId, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), task.RoomId);

        task.Complete();
        room.UpdateCleaningStatus(request.CleaningStatus ?? RoomCleaningStatus.Inspected);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
