using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Housekeeping;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Housekeeping.Commands;

public sealed record CreateHousekeepingTaskCommand(Guid RoomId, string TaskType, Guid? AssignedToId = null, string? Notes = null)
    : IRequest<Guid>;

internal sealed class CreateHousekeepingTaskCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateHousekeepingTaskCommand, Guid>
{
    public async Task<Guid> Handle(CreateHousekeepingTaskCommand request, CancellationToken cancellationToken)
    {
        var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), request.RoomId);

        var task = HousekeepingTask.Create(request.RoomId, request.TaskType, request.AssignedToId, request.Notes);
        room.UpdateCleaningStatus(RoomCleaningStatus.Dirty);

        dbContext.HousekeepingTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
