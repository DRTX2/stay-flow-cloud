using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Maintenance;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Maintenance.Commands;

public sealed record ResolveWorkOrderCommand(Guid Id, string? Notes = null) : IRequest;

internal sealed class ResolveWorkOrderCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ResolveWorkOrderCommand>
{
    public async Task Handle(ResolveWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await dbContext.WorkOrders.FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.Id);

        workOrder.Resolve(request.Notes);

        if (workOrder.RoomId.HasValue)
        {
            var hasOtherOpenWorkOrders = await dbContext.WorkOrders.AnyAsync(w =>
                w.Id != workOrder.Id
                && w.RoomId == workOrder.RoomId
                && (w.Status == WorkOrderStatus.Open || w.Status == WorkOrderStatus.InProgress), cancellationToken);

            if (!hasOtherOpenWorkOrders)
            {
                var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == workOrder.RoomId.Value, cancellationToken)
                    ?? throw new NotFoundException(nameof(Room), workOrder.RoomId.Value);

                var isOccupied = await dbContext.Reservations.AnyAsync(r =>
                    r.RoomId == room.Id && r.Status == ReservationStatus.CheckedIn, cancellationToken);

                if (isOccupied)
                {
                    room.MarkOccupied();
                }
                else
                {
                    room.ReturnToService();
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
