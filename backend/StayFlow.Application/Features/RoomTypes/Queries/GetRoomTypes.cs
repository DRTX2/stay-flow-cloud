using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;

namespace StayFlow.Application.Features.RoomTypes.Queries;

public sealed record GetRoomTypesQuery : IRequest<IReadOnlyList<RoomTypeDto>>;

public sealed class GetRoomTypesHandler(
    IApplicationDbContext dbContext,
    ICacheService cache,
    ITenantProvider tenantProvider)
    : IRequestHandler<GetRoomTypesQuery, IReadOnlyList<RoomTypeDto>>
{
    internal static string CacheKey(Guid? tenantId) => $"roomtypes:{tenantId ?? Guid.Empty}";

    public async Task<IReadOnlyList<RoomTypeDto>> Handle(GetRoomTypesQuery request, CancellationToken cancellationToken)
    {
        // Room types are slow-changing reference data — ideal to serve from cache.
        return await cache.GetOrCreateAsync(
            CacheKey(tenantProvider.TenantId),
            async ct => (IReadOnlyList<RoomTypeDto>)await dbContext.RoomTypes
                .AsNoTracking()
                .OrderBy(rt => rt.Name)
                .Select(rt => new RoomTypeDto(rt.Id, rt.Name, rt.Description, rt.BaseRate, rt.MaxOccupancy))
                .ToListAsync(ct),
            ttl: TimeSpan.FromMinutes(10),
            cancellationToken);
    }
}
