namespace StayFlow.Application.Features.Housekeeping;

public sealed record HousekeepingTaskDto(
    Guid Id,
    Guid RoomId,
    string TaskType,
    string Status,
    Guid? AssignedToId,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);
