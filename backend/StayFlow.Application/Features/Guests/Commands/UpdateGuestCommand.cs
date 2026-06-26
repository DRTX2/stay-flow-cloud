using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Domain.Guests;

namespace StayFlow.Application.Features.Guests.Commands;

public sealed record UpdateGuestCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null,
    string? DocumentNumber = null) : IRequest;

public sealed class UpdateGuestValidator : AbstractValidator<UpdateGuestCommand>
{
    public UpdateGuestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.DocumentNumber).MaximumLength(60);
    }
}

public sealed class UpdateGuestHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateGuestCommand>
{
    public async Task Handle(UpdateGuestCommand request, CancellationToken cancellationToken)
    {
        var guest = await dbContext.Guests.FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Guest), request.Id);

        guest.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.DocumentNumber);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
