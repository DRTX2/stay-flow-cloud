using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Reservations.Commands;

public sealed record CheckOutReservationCommand(Guid Id) : IRequest;

public sealed class CheckOutReservationHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CheckOutReservationCommand>
{
    public async Task Handle(CheckOutReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        reservation.CheckOut();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
