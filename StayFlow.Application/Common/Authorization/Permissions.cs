namespace StayFlow.Application.Common.Authorization;

/// <summary>
/// Canonical permission strings used for permission-based authorization. Tokens carry the
/// permissions granted to the user's roles; <c>[Authorize(Policy = ...)]</c> checks them.
/// </summary>
public static class Permissions
{
    public const string RoomsRead = "rooms:read";
    public const string RoomsManage = "rooms:manage";

    public const string GuestsRead = "guests:read";
    public const string GuestsManage = "guests:manage";

    public const string ReservationsRead = "reservations:read";
    public const string ReservationsManage = "reservations:manage";
    public const string ReservationsCheckInOut = "reservations:checkinout";

    public const string ServicesRead = "services:read";
    public const string ServicesManage = "services:manage";

    public const string BillingRead = "billing:read";
    public const string BillingManage = "billing:manage";

    public const string AnalyticsView = "analytics:view";

    /// <summary>Platform-level: manage tenants. Reserved for the super-admin.</summary>
    public const string TenantsManage = "tenants:manage";

    private static readonly string[] AllPermissions =
    [
        RoomsRead, RoomsManage,
        GuestsRead, GuestsManage,
        ReservationsRead, ReservationsManage, ReservationsCheckInOut,
        ServicesRead, ServicesManage,
        BillingRead, BillingManage,
        AnalyticsView,
        TenantsManage,
    ];

    public static IReadOnlyList<string> All => AllPermissions;
}
