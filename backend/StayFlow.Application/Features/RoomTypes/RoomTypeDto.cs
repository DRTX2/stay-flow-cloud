namespace StayFlow.Application.Features.RoomTypes;

public sealed record RoomTypeDto(
    Guid Id,
    string Name,
    string? Description,
    decimal BaseRate,
    int MaxOccupancy);
