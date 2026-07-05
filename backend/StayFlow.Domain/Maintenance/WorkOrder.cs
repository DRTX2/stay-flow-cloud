using StayFlow.Domain.Common;
using StayFlow.Domain.Maintenance.Events;

namespace StayFlow.Domain.Maintenance;

public sealed class WorkOrder : TenantEntity
{
    private WorkOrder()
    {
    }

    private WorkOrder(Guid? roomId, string description, WorkOrderPriority priority, Guid? reportedById)
    {
        RoomId = roomId;
        Description = description;
        Priority = priority;
        ReportedById = reportedById;
        Status = WorkOrderStatus.Open;
    }

    public Guid? RoomId { get; private set; }
    
    public string Description { get; private set; } = string.Empty;
    
    public WorkOrderPriority Priority { get; private set; }
    
    public WorkOrderStatus Status { get; private set; }
    
    public Guid? ReportedById { get; private set; }
    
    public Guid? AssignedToId { get; private set; }
    
    public string? ResolutionNotes { get; private set; }
    
    public DateTimeOffset? ResolvedAtUtc { get; private set; }

    public static WorkOrder Create(Guid? roomId, string description, WorkOrderPriority priority, Guid? reportedById = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Description is required.");
        }

        var workOrder = new WorkOrder(roomId, description.Trim(), priority, reportedById);
        workOrder.RaiseDomainEvent(new WorkOrderCreatedEvent(workOrder.Id, workOrder.TenantId, workOrder.Description, priority));
        return workOrder;
    }

    public void AssignTo(Guid userId)
    {
        AssignedToId = userId;
    }

    public void StartWork()
    {
        if (Status != WorkOrderStatus.Open)
        {
            throw new DomainException($"Cannot start a work order in {Status} status.");
        }
            
        Status = WorkOrderStatus.InProgress;
    }

    public void Resolve(string? notes = null)
    {
        if (Status == WorkOrderStatus.Resolved)
        {
            return;
        }
            
        Status = WorkOrderStatus.Resolved;
        ResolutionNotes = notes?.Trim();
        ResolvedAtUtc = DateTimeOffset.UtcNow;
        
        RaiseDomainEvent(new WorkOrderResolvedEvent(Id, TenantId));
    }

    public void Cancel()
    {
        if (Status == WorkOrderStatus.Resolved)
        {
            throw new DomainException("Cannot cancel a resolved work order.");
        }
            
        Status = WorkOrderStatus.Cancelled;
    }
}
