namespace StayFlow.Application.Common.Authorization;

/// <summary>Built-in roles and the permissions each grants. Used to seed role claims.</summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string HotelOwner = "HotelOwner";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string FrontDesk = "FrontDesk";
    public const string Receptionist = "Receptionist";
    public const string Housekeeping = "Housekeeping";
    public const string Maintenance = "Maintenance";
    public const string Staff = "Staff";
    public const string Customer = "Customer";

    private static readonly Dictionary<string, string[]> RolePermissions = new(StringComparer.Ordinal)
    {
        [SuperAdmin] = [.. Permissions.All],
        [HotelOwner] =
        [
            Permissions.RoomsRead, Permissions.RoomsManage,
            Permissions.GuestsRead, Permissions.GuestsManage,
            Permissions.ReservationsRead, Permissions.ReservationsManage, Permissions.ReservationsCheckInOut,
            Permissions.ServicesRead, Permissions.ServicesManage,
            Permissions.BillingRead, Permissions.BillingManage,
            Permissions.AnalyticsView,
            Permissions.FeedbackRead,
            Permissions.HousekeepingManage,
            Permissions.MaintenanceManage,
            Permissions.OrdersManage,
            Permissions.StaffManage,
            Permissions.FeaturesManage,
        ],
        [Admin] =
        [
            Permissions.RoomsRead, Permissions.RoomsManage,
            Permissions.GuestsRead, Permissions.GuestsManage,
            Permissions.ReservationsRead, Permissions.ReservationsManage, Permissions.ReservationsCheckInOut,
            Permissions.ServicesRead, Permissions.ServicesManage,
            Permissions.BillingRead, Permissions.BillingManage,
            Permissions.AnalyticsView,
            Permissions.FeedbackRead,
            Permissions.HousekeepingManage,
            Permissions.MaintenanceManage,
            Permissions.OrdersManage,
            Permissions.StaffManage,
            Permissions.FeaturesManage,
        ],
        [Manager] =
        [
            Permissions.RoomsRead, Permissions.RoomsManage,
            Permissions.GuestsRead, Permissions.GuestsManage,
            Permissions.ReservationsRead, Permissions.ReservationsManage, Permissions.ReservationsCheckInOut,
            Permissions.ServicesRead, Permissions.ServicesManage,
            Permissions.BillingRead, Permissions.BillingManage,
            Permissions.AnalyticsView,
            Permissions.FeedbackRead,
            Permissions.HousekeepingManage,
            Permissions.MaintenanceManage,
            Permissions.OrdersManage,
            Permissions.StaffManage,
        ],
        [FrontDesk] =
        [
            Permissions.RoomsRead,
            Permissions.GuestsRead, Permissions.GuestsManage,
            Permissions.ReservationsRead, Permissions.ReservationsManage, Permissions.ReservationsCheckInOut,
            Permissions.ServicesRead,
            Permissions.BillingRead, Permissions.BillingManage,
            Permissions.HousekeepingManage,
            Permissions.MaintenanceManage,
            Permissions.OrdersManage,
        ],
        [Receptionist] =
        [
            Permissions.RoomsRead,
            Permissions.GuestsRead, Permissions.GuestsManage,
            Permissions.ReservationsRead, Permissions.ReservationsManage, Permissions.ReservationsCheckInOut,
            Permissions.ServicesRead,
            Permissions.BillingRead, Permissions.BillingManage,
            Permissions.HousekeepingManage,
            Permissions.MaintenanceManage,
            Permissions.OrdersManage,
        ],
        [Housekeeping] =
        [
            Permissions.RoomsRead,
            Permissions.ReservationsRead,
            Permissions.HousekeepingManage,
        ],
        [Maintenance] =
        [
            Permissions.RoomsRead,
            Permissions.MaintenanceManage,
        ],
        [Staff] =
        [
            Permissions.RoomsRead,
            Permissions.ServicesRead,
            Permissions.OrdersManage,
        ],
        [Customer] =
        [
            Permissions.ReservationsRead,
            Permissions.BillingRead,
        ],
    };

    public static IReadOnlyDictionary<string, string[]> PermissionsByRole => RolePermissions;

    public static IReadOnlyList<string> All => [.. RolePermissions.Keys];

    public static IReadOnlyList<string> StaffAssignable => [FrontDesk, Housekeeping, Manager, Admin];
}
