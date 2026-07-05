namespace StayFlow.Application.Features.Staff;

public sealed record StaffUserDto(
    Guid Id,
    string FullName,
    string Email,
    bool IsActive,
    IReadOnlyList<string> Roles);
