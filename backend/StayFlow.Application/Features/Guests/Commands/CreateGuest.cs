using FluentValidation;
using MediatR;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Guests;

namespace StayFlow.Application.Features.Guests.Commands;

public sealed record CreateGuestCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone = null,
    string? DocumentNumber = null) : IRequest<Guid>;

public sealed class CreateGuestValidator : AbstractValidator<CreateGuestCommand>
{
    public CreateGuestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(40);
        RuleFor(x => x.DocumentNumber).MaximumLength(60);
    }
}

public sealed class CreateGuestHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateGuestCommand, Guid>
{
    public async Task<Guid> Handle(CreateGuestCommand request, CancellationToken cancellationToken)
    {
        var guest = Guest.Create(request.FirstName, request.LastName, request.Email, request.Phone, request.DocumentNumber);
        dbContext.Guests.Add(guest);
        await dbContext.SaveChangesAsync(cancellationToken);

        return guest.Id;
    }
}
