using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Billing;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Services;
using ValidationException = StayFlow.Application.Common.Exceptions.ValidationException;

namespace StayFlow.Application.Features.Reservations.Commands;

/// <summary>Posts an ancillary service charge (e.g. breakfast, spa) against a reservation's folio.</summary>
public sealed record AddReservationChargeCommand(Guid ReservationId, Guid ServiceItemId, int Quantity = 1)
    : IRequest<Guid>;

public sealed class AddReservationChargeValidator : AbstractValidator<AddReservationChargeCommand>
{
    public AddReservationChargeValidator()
    {
        RuleFor(x => x.ReservationId).NotEmpty();
        RuleFor(x => x.ServiceItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1);
    }
}

public sealed class AddReservationChargeHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AddReservationChargeCommand, Guid>
{
    public async Task<Guid> Handle(AddReservationChargeCommand request, CancellationToken cancellationToken)
    {
        var reservation = await dbContext.Reservations
            .FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Reservation), request.ReservationId);

        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.CheckedOut)
        {
            throw new ValidationException(
            [
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ReservationId),
                    $"Cannot add charges to a {reservation.Status} reservation."),
            ]);
        }

        var service = await dbContext.ServiceItems
            .FirstOrDefaultAsync(s => s.Id == request.ServiceItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(ServiceItem), request.ServiceItemId);

        var charge = ReservationCharge.Create(reservation.Id, service, request.Quantity);
        dbContext.ReservationCharges.Add(charge);
        await dbContext.SaveChangesAsync(cancellationToken);

        return charge.Id;
    }
}
