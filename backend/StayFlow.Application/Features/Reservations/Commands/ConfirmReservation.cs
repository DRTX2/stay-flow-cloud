using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Reservations.Commands;

public sealed record ConfirmReservationCommand(Guid Id) : IRequest;

public sealed class ConfirmReservationHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ConfirmReservationCommand>
{
    public async Task Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        reservation.Confirm();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
