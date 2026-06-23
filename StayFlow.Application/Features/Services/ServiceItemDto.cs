using StayFlow.Domain.Services;

namespace StayFlow.Application.Features.Services;

public sealed record ServiceItemDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    ServiceCategory Category,
    bool IsActive);
