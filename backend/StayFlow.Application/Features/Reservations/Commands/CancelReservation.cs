using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Reservations;

namespace StayFlow.Application.Features.Reservations.Commands;

public sealed record CancelReservationCommand(Guid Id, string? Reason = null) : IRequest;

public sealed class CancelReservationValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class CancelReservationHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CancelReservationCommand>
{
    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.Id);

        reservation.Cancel(request.Reason);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
