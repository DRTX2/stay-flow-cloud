using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
using StayFlow.Application.Pricing;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;

namespace StayFlow.Application.Features.PublicCatalog;

public sealed record PublicRoomTypeDto(
    Guid Id,
    string Name,
    string? Description,
    decimal BaseRate,
    int MaxOccupancy);

public sealed record PublicHotelDto(
    string Slug,
    string Name,
    string PropertyType,
    string Currency,
    IReadOnlyList<PublicRoomTypeDto> RoomTypes);

public sealed record PublicAvailabilityDto(
    string HotelSlug,
    Guid RoomTypeId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests,
    int Nights,
    int AvailableRoomCount,
    decimal? EstimatedTotal,
    decimal? AverageNightlyRate,
    string Currency);

public sealed record GetPublicHotelsQuery : IRequest<IReadOnlyList<PublicHotelDto>>;
public sealed record GetPublicHotelQuery(string Slug) : IRequest<PublicHotelDto>;
public sealed record GetPublicAvailabilityQuery(
    string HotelSlug,
    Guid RoomTypeId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Guests) : IRequest<PublicAvailabilityDto>;

public sealed class GetPublicAvailabilityValidator : AbstractValidator<GetPublicAvailabilityQuery>
{
    public GetPublicAvailabilityValidator()
    {
        RuleFor(x => x.HotelSlug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoomTypeId).NotEmpty();
        RuleFor(x => x.CheckIn).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.CheckOut).GreaterThan(x => x.CheckIn);
        RuleFor(x => x.Guests).InclusiveBetween(1, 20);
    }
}

public sealed class GetPublicHotelsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPublicHotelsQuery, IReadOnlyList<PublicHotelDto>>
{
    public Task<IReadOnlyList<PublicHotelDto>> Handle(GetPublicHotelsQuery request, CancellationToken cancellationToken)
        => PublicCatalog.LoadAsync(dbContext, null, cancellationToken);
}

public sealed class GetPublicHotelHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPublicHotelQuery, PublicHotelDto>
{
    public async Task<PublicHotelDto> Handle(GetPublicHotelQuery request, CancellationToken cancellationToken)
    {
        var hotels = await PublicCatalog.LoadAsync(dbContext, request.Slug.Trim().ToLowerInvariant(), cancellationToken);
        return hotels.SingleOrDefault() ?? throw new NotFoundException("Hotel", request.Slug);
    }
}

public sealed class GetPublicAvailabilityHandler(IApplicationDbContext dbContext, IPricingService pricingService)
    : IRequestHandler<GetPublicAvailabilityQuery, PublicAvailabilityDto>
{
    private static readonly ReservationStatus[] ActiveStatuses =
        [ReservationStatus.Pending, ReservationStatus.Confirmed, ReservationStatus.CheckedIn];

    public async Task<PublicAvailabilityDto> Handle(GetPublicAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var slug = request.HotelSlug.Trim().ToLowerInvariant();
        var tenant = await dbContext.Tenants.AsNoTracking()
            .SingleOrDefaultAsync(t => t.Slug == slug && t.IsActive, cancellationToken)
            ?? throw new NotFoundException("Hotel", request.HotelSlug);

        var roomType = await dbContext.RoomTypes.IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(rt => rt.Id == request.RoomTypeId
                                        && rt.TenantId == tenant.Id
                                        && !rt.IsDeleted,
                cancellationToken)
            ?? throw new NotFoundException("Room type", request.RoomTypeId);

        PublicAvailabilityDto Empty(string currency) => new(
            slug, roomType.Id, request.CheckIn, request.CheckOut, request.Guests,
            request.CheckOut.DayNumber - request.CheckIn.DayNumber, 0, null, null, currency);

        if (request.Guests > roomType.MaxOccupancy)
        {
            return Empty(tenant.DefaultCurrency);
        }

        var activeReservations = dbContext.Reservations.IgnoreQueryFilters().AsNoTracking()
            .Where(r => r.TenantId == tenant.Id
                        && !r.IsDeleted
                        && ActiveStatuses.Contains(r.Status)
                        && r.Period.CheckIn < request.CheckOut
                        && request.CheckIn < r.Period.CheckOut);

        var availablePrices = await dbContext.Rooms.IgnoreQueryFilters().AsNoTracking()
            .Where(room => room.TenantId == tenant.Id
                           && room.RoomTypeId == roomType.Id
                           && !room.IsDeleted
                           && room.Capacity >= request.Guests
                           && room.Status != RoomStatus.Maintenance
                           && room.Status != RoomStatus.OutOfService
                           && !activeReservations.Any(reservation => reservation.RoomId == room.Id))
            .Select(room => room.BasePrice)
            .ToListAsync(cancellationToken);

        if (availablePrices.Count == 0)
        {
            return Empty(tenant.DefaultCurrency);
        }

        var totalOperationalRooms = await dbContext.Rooms.IgnoreQueryFilters().AsNoTracking()
            .CountAsync(room => room.TenantId == tenant.Id
                                && !room.IsDeleted
                                && room.Status != RoomStatus.Maintenance
                                && room.Status != RoomStatus.OutOfService,
                cancellationToken);
        var occupiedRooms = await activeReservations.Select(r => r.RoomId).Distinct().CountAsync(cancellationToken);
        var occupancy = totalOperationalRooms == 0 ? 0d : (double)occupiedRooms / totalOperationalRooms;
        var quote = pricingService.Quote(new PricingRequest(
            availablePrices.Min(), request.CheckIn, request.CheckOut, occupancy, request.Guests));

        return new PublicAvailabilityDto(
            slug, roomType.Id, request.CheckIn, request.CheckOut, request.Guests, quote.Nights,
            availablePrices.Count, quote.TotalPrice, quote.AverageNightlyRate, tenant.DefaultCurrency);
    }
}

internal static class PublicCatalog
{
    public static async Task<IReadOnlyList<PublicHotelDto>> LoadAsync(
        IApplicationDbContext dbContext,
        string? slug,
        CancellationToken cancellationToken)
    {
        var tenantsQuery = dbContext.Tenants.AsNoTracking().Where(tenant => tenant.IsActive);
        if (slug is not null)
        {
            tenantsQuery = tenantsQuery.Where(tenant => tenant.Slug == slug);
        }

        var tenants = await tenantsQuery.OrderBy(tenant => tenant.Name).ToListAsync(cancellationToken);
        var tenantIds = tenants.Select(tenant => tenant.Id).ToArray();
        var roomTypes = await dbContext.RoomTypes.IgnoreQueryFilters().AsNoTracking()
            .Where(roomType => tenantIds.Contains(roomType.TenantId) && !roomType.IsDeleted)
            .OrderBy(roomType => roomType.Name)
            .Select(roomType => new
            {
                roomType.TenantId,
                roomType.Id,
                roomType.Name,
                roomType.Description,
                roomType.BaseRate,
                roomType.MaxOccupancy,
            })
            .ToListAsync(cancellationToken);

        return tenants.Select(tenant => new PublicHotelDto(
            tenant.Slug,
            tenant.Name,
            tenant.PropertyType.ToString(),
            tenant.DefaultCurrency,
            roomTypes.Where(roomType => roomType.TenantId == tenant.Id)
                .Select(roomType => new PublicRoomTypeDto(
                    roomType.Id, roomType.Name, roomType.Description, roomType.BaseRate, roomType.MaxOccupancy))
                .ToList())).ToList();
    }
}
