using MediatR;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Authorization;

namespace StayFlow.Application.Features.Staff.Queries;

public sealed record GetStaffUsersQuery : IRequest<StaffUsersDto>;

public sealed record StaffUsersDto(IReadOnlyList<string> AssignableRoles, IReadOnlyList<StaffUserDto> Users);

public sealed class GetStaffUsersHandler(IStaffAdministrationService staff)
    : IRequestHandler<GetStaffUsersQuery, StaffUsersDto>
{
    public async Task<StaffUsersDto> Handle(GetStaffUsersQuery request, CancellationToken cancellationToken)
        => new(Roles.StaffAssignable, await staff.ListAsync(cancellationToken));
}
