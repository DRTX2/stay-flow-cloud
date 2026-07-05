using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Application.Features.Analytics.Queries;

public sealed record GetSetupChecklistQuery : IRequest<SetupChecklistDto>;

public sealed record SetupChecklistDto(int CompletedSteps, int TotalSteps, int PercentComplete, IReadOnlyList<SetupStepDto> Steps);

public sealed record SetupStepDto(string Key, string Label, bool Completed, int Count, string NextHref);

public sealed class GetSetupChecklistHandler(
    IApplicationDbContext dbContext,
    ITenantProvider tenantProvider,
    ICurrentUser currentUser) : IRequestHandler<GetSetupChecklistQuery, SetupChecklistDto>
{
    public async Task<SetupChecklistDto> Handle(GetSetupChecklistQuery request, CancellationToken cancellationToken)
    {
        var tenantConfigured = tenantProvider.TenantId.HasValue
            && await dbContext.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantProvider.TenantId.Value, cancellationToken);
        var roomTypes = await dbContext.RoomTypes.CountAsync(cancellationToken);
        var rooms = await dbContext.Rooms.CountAsync(cancellationToken);
        var services = await dbContext.ServiceItems.CountAsync(cancellationToken);
        var staffConfigured = currentUser.UserId.HasValue;

        var steps = new List<SetupStepDto>
        {
            new("tenant", "Create tenant and hotel profile", tenantConfigured, tenantConfigured ? 1 : 0, "/dashboard/settings"),
            new("room-types", "Create room types", roomTypes > 0, roomTypes, "/dashboard/room-types"),
            new("rooms", "Load physical room inventory", rooms > 0, rooms, "/dashboard/rooms"),
            new("services", "Configure sellable services", services > 0, services, "/dashboard/services"),
            new("staff", "Sign in as an operational staff user", staffConfigured, staffConfigured ? 1 : 0, "/dashboard"),
        };

        var completed = steps.Count(step => step.Completed);
        var percent = steps.Count == 0 ? 100 : (int)Math.Round(completed * 100m / steps.Count);
        return new SetupChecklistDto(completed, steps.Count, percent, steps);
    }
}
