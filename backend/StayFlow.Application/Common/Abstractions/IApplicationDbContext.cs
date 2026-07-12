using Microsoft.EntityFrameworkCore;
using StayFlow.Domain.Billing;
using StayFlow.Domain.BookingEnquiries;
using StayFlow.Domain.Feedback;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Housekeeping;
using StayFlow.Domain.Maintenance;
using StayFlow.Domain.Orders;
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
    DbSet<BookingEnquiry> BookingEnquiries { get; }
    DbSet<ReservationFeedback> ReservationFeedback { get; }
    DbSet<RoomType> RoomTypes { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Guest> Guests { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<ServiceItem> ServiceItems { get; }
    DbSet<ReservationCharge> ReservationCharges { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<TenantFeatureOverride> TenantFeatureOverrides { get; }
    DbSet<HousekeepingTask> HousekeepingTasks { get; }
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<Order> Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
