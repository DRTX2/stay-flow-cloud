using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.Rooms;

public sealed record RoomDto(
    Guid Id,
    string Number,
    Guid RoomTypeId,
    string RoomTypeName,
    decimal BasePrice,
    int Capacity,
    int Floor,
    RoomStatus Status);
