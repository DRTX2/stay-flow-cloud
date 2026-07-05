using FluentValidation;
using MediatR;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Application.Features.Staff.Commands;

public sealed record UpdateStaffRolesCommand(Guid Id, IReadOnlyList<string> Roles) : IRequest;

public sealed class UpdateStaffRolesValidator : AbstractValidator<UpdateStaffRolesCommand>
{
    public UpdateStaffRolesValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Roles).NotEmpty();
    }
}

public sealed class UpdateStaffRolesHandler(IStaffAdministrationService staff)
    : IRequestHandler<UpdateStaffRolesCommand>
{
    public Task Handle(UpdateStaffRolesCommand request, CancellationToken cancellationToken)
        => staff.UpdateRolesAsync(request.Id, request.Roles, cancellationToken);
}
