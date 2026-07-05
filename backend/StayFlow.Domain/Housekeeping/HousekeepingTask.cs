using StayFlow.Domain.Common;

namespace StayFlow.Domain.Housekeeping;

public sealed class HousekeepingTask : TenantEntity
{
    private HousekeepingTask()
    {
    }

    private HousekeepingTask(Guid roomId, string taskType, Guid? assignedToId, string? notes)
    {
        RoomId = roomId;
        TaskType = taskType;
        AssignedToId = assignedToId;
        Notes = notes;
        Status = HousekeepingTaskStatus.Pending;
    }

    public Guid RoomId { get; private set; }
    
    /// <summary>E.g., "Daily Clean", "Deep Clean", "Turn Down Service"</summary>
    public string TaskType { get; private set; } = string.Empty;
    
    public HousekeepingTaskStatus Status { get; private set; }
    
    public Guid? AssignedToId { get; private set; }
    
    public string? Notes { get; private set; }
    
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public static HousekeepingTask Create(Guid roomId, string taskType, Guid? assignedToId = null, string? notes = null)
    {
        if (roomId == Guid.Empty)
        {
            throw new DomainException("RoomId is required.");
        }
        if (string.IsNullOrWhiteSpace(taskType))
        {
            throw new DomainException("TaskType is required.");
        }

        return new HousekeepingTask(roomId, taskType.Trim(), assignedToId, notes?.Trim());
    }

    public void AssignTo(Guid userId)
    {
        AssignedToId = userId;
    }

    public void Start()
    {
        if (Status != HousekeepingTaskStatus.Pending)
        {
            throw new DomainException($"Cannot start a task in {Status} status.");
        }
            
        Status = HousekeepingTaskStatus.InProgress;
    }

    public void Complete()
    {
        if (Status == HousekeepingTaskStatus.Completed)
        {
            return;
        }
            
        Status = HousekeepingTaskStatus.Completed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }
}
