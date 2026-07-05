using StayFlow.Application.Features.Staff;

namespace StayFlow.Application.Common.Abstractions;

public interface IStaffAdministrationService
{
    Task<IReadOnlyList<StaffUserDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        string fullName,
        string email,
        string password,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);

    Task UpdateRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default);
}
