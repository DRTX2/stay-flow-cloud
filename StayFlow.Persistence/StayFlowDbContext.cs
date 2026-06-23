using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Billing;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;
using StayFlow.Domain.Services;
using StayFlow.Domain.Tenants;
using StayFlow.Persistence.Identity;

namespace StayFlow.Persistence;

/// <summary>
/// The application's EF Core context. Hosts the business model, ASP.NET Identity, and the
/// OpenIddict stores. Enforces multi-tenant isolation and soft-delete via global query filters
/// keyed off the per-request tenant. Auditing, tenant stamping and domain-event dispatch are
/// applied by registered save interceptors.
/// </summary>
public sealed class StayFlowDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    private readonly Guid _tenantId;

    public StayFlowDbContext(DbContextOptions<StayFlowDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        // Captured once per (scoped) context instance. EF re-reads this field at query time,
        // so the cached model stays correct across tenants.
        _tenantId = tenantProvider.TenantId ?? Guid.Empty;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<ReservationCharge> ReservationCharges => Set<ReservationCharge>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(StayFlowDbContext).Assembly);

        // Tenant isolation + soft delete. Tenant itself is not tenant-scoped.
        builder.Entity<RoomType>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Room>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Guest>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Reservation>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<ServiceItem>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<ReservationCharge>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Invoice>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
    }
}
