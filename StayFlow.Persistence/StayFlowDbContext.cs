using Microsoft.EntityFrameworkCore;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Domain.Guests;
using StayFlow.Domain.Reservations;
using StayFlow.Domain.Rooms;
using StayFlow.Domain.Tenants;

namespace StayFlow.Persistence;

/// <summary>
/// The application's EF Core context. Enforces multi-tenant isolation and soft-delete via
/// global query filters keyed off the per-request tenant. Auditing, tenant stamping and
/// domain-event dispatch are applied by registered save interceptors.
/// </summary>
public sealed class StayFlowDbContext : DbContext, IApplicationDbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StayFlowDbContext).Assembly);

        // Tenant isolation + soft delete. Tenant itself is not tenant-scoped.
        modelBuilder.Entity<RoomType>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        modelBuilder.Entity<Room>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        modelBuilder.Entity<Guest>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        modelBuilder.Entity<Reservation>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
    }
}
