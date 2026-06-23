using Microsoft.EntityFrameworkCore;
using StayFlow.Domain.Billing;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;
using StayFlow.Domain.Services;
using StayFlow.Domain.Tenants;

namespace StayFlow.Application.Common.Abstractions;

/// <summary>
/// Application-facing abstraction over the persistence context. Handlers depend on this
/// rather than the concrete <c>StayFlowDbContext</c>, keeping the application layer free of
/// EF provider concerns and trivially mockable in unit tests.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<RoomType> RoomTypes { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Guest> Guests { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<ServiceItem> ServiceItems { get; }
    DbSet<ReservationCharge> ReservationCharges { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<TenantFeatureOverride> TenantFeatureOverrides { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
