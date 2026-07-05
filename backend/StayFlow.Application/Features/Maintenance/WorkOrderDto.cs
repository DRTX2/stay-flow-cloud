namespace StayFlow.Application.Features.Maintenance;

public sealed record WorkOrderDto(
    Guid Id,
    Guid? RoomId,
    string Description,
    string Priority,
    string Status,
    Guid? ReportedById,
    Guid? AssignedToId,
    string? ResolutionNotes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ResolvedAtUtc);
