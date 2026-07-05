using FluentValidation;
using MediatR;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Application.Features.Staff.Commands;

public sealed record CreateStaffUserCommand(string FullName, string Email, string Password, IReadOnlyList<string> Roles) : IRequest<Guid>;

public sealed class CreateStaffUserValidator : AbstractValidator<CreateStaffUserCommand>
{
    public CreateStaffUserValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public sealed class CreateStaffUserHandler(IStaffAdministrationService staff)
    : IRequestHandler<CreateStaffUserCommand, Guid>
{
    public Task<Guid> Handle(CreateStaffUserCommand request, CancellationToken cancellationToken)
        => staff.CreateAsync(request.FullName, request.Email, request.Password, request.Roles, cancellationToken);
}
