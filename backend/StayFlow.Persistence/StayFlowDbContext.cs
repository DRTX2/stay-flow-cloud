using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StayFlow.Application.Common.Abstractions;
using StayFlow.Application.Common.Exceptions;
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
using StayFlow.Persistence.Identity;
using StayFlow.Persistence.Notifications;
using StayFlow.Persistence.Outbox;

namespace StayFlow.Persistence;

/// <summary>
/// The application's EF Core context. Hosts the business model, ASP.NET Identity, and the
/// OpenIddict stores. Enforces multi-tenant isolation and soft-delete via global query filters
/// keyed off the per-request tenant. Auditing, tenant stamping and domain-event dispatch are
/// applied by registered save interceptors.
/// </summary>
public sealed class StayFlowDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext, IDataProtectionKeyContext
{
    internal const string ActiveReservationOverlapConstraint = "EX_Reservations_ActiveRoomPeriod";

    private readonly Guid _tenantId;

    public StayFlowDbContext(DbContextOptions<StayFlowDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        // Captured once per (scoped) context instance. EF re-reads this field at query time,
        // so the cached model stays correct across tenants.
        _tenantId = tenantProvider.TenantId ?? Guid.Empty;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<BookingEnquiry> BookingEnquiries => Set<BookingEnquiry>();
    public DbSet<ReservationFeedback> ReservationFeedback => Set<ReservationFeedback>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<ReservationCharge> ReservationCharges => Set<ReservationCharge>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<TenantFeatureOverride> TenantFeatureOverrides => Set<TenantFeatureOverride>();
    public DbSet<HousekeepingTask> HousekeepingTasks => Set<HousekeepingTask>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<PortalGuestInvitation> PortalGuestInvitations => Set<PortalGuestInvitation>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<InAppNotification> InAppNotifications => Set<InAppNotification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(StayFlowDbContext).Assembly);

        builder.Entity<ApplicationUser>()
            .HasIndex(user => new { user.TenantId, user.GuestId })
            .IsUnique()
            .HasFilter("\"GuestId\" IS NOT NULL");

        builder.Entity<PortalGuestInvitation>(entity =>
        {
            entity.ToTable("PortalGuestInvitations");
            entity.HasKey(invitation => invitation.Id);
            entity.Property(invitation => invitation.TokenHash).IsRequired().HasMaxLength(64);
            entity.HasIndex(invitation => invitation.TokenHash).IsUnique();
            entity.HasIndex(invitation => new { invitation.TenantId, invitation.GuestId });
        });

        builder.Entity<InAppNotification>(entity =>
        {
            entity.ToTable("InAppNotifications");
            entity.HasKey(notification => notification.Id);
            entity.Property(notification => notification.Title).IsRequired().HasMaxLength(160);
            entity.Property(notification => notification.Body).IsRequired().HasMaxLength(500);
            entity.Property(notification => notification.Type).IsRequired().HasMaxLength(50);
            entity.Property(notification => notification.Link).HasMaxLength(500);
            entity.HasIndex(notification => new
            {
                notification.TenantId,
                notification.UserId,
                notification.SourceEventId,
            }).IsUnique();
            entity.HasIndex(notification => new
            {
                notification.TenantId,
                notification.UserId,
                notification.CreatedAtUtc,
            });
        });

        // Tenant isolation + soft delete. Tenant itself is not tenant-scoped.
        builder.Entity<RoomType>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<BookingEnquiry>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<ReservationFeedback>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Room>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Guest>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Reservation>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<ServiceItem>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<ReservationCharge>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Invoice>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<TenantFeatureOverride>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<HousekeepingTask>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<WorkOrder>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<Order>().HasQueryFilter(e => e.TenantId == _tenantId && !e.IsDeleted);
        builder.Entity<InAppNotification>().HasQueryFilter(e => e.TenantId == _tenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ExclusionViolation,
            ConstraintName: ActiveReservationOverlapConstraint,
        })
        {
            throw new ReservationConflictException();
        }
    }
}
