using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Maintenance;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Maintenance.Commands;

public sealed record CreateWorkOrderCommand(Guid? RoomId, string Description, WorkOrderPriority Priority) : IRequest<Guid>;

internal sealed class CreateWorkOrderCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser) : IRequestHandler<CreateWorkOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        Room? room = null;
        if (request.RoomId.HasValue)
        {
            room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Room), request.RoomId.Value);
        }

        var workOrder = WorkOrder.Create(request.RoomId, request.Description, request.Priority, currentUser.UserId);
        if (room is not null)
        {
            if (request.Priority == WorkOrderPriority.Urgent)
            {
                room.MarkOutOfService();
            }
            else
            {
                room.PutUnderMaintenance();
            }
        }
        
        dbContext.WorkOrders.Add(workOrder);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return workOrder.Id;
    }
}
