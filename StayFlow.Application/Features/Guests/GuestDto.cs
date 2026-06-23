namespace StayFlow.Application.Features.Guests;

public sealed record GuestDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? DocumentNumber);
