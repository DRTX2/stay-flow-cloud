using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Reservations.Commands;

public sealed record CheckOutReservationCommand(Guid Id) : IRequest;

public sealed class CheckOutReservationHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CheckOutReservationCommand>
{
    public async Task Handle(CheckOutReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);
        var room = await dbContext.Rooms.FirstOrDefaultAsync(r => r.Id == reservation.RoomId, cancellationToken)
            ?? throw new NotFoundException(nameof(Room), reservation.RoomId);

        reservation.CheckOut();
        room.ReturnToService();
        room.UpdateCleaningStatus(RoomCleaningStatus.Dirty);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
