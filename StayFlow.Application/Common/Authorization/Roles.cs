namespace StayFlow.Application.Common.Authorization;

/// <summary>Built-in roles and the permissions each grants. Used to seed role claims.</summary>
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string HotelOwner = "HotelOwner";
    public const string Manager = "Manager";
    public const string Receptionist = "Receptionist";
    public const string Housekeeping = "Housekeeping";
    public const string Customer = "Customer";

    private static readonly Dictionary<string, string[]> RolePermissions = new(StringComparer.Ordinal)
    {
        [SuperAdmin] = [.. Permissions.All],
        [HotelOwner] =
        [
            Permissions.RoomsRead, Permissions.RoomsManage,
            Permissions.GuestsRead, Permissions.GuestsManage,
            Permissions.ReservationsRead, Permissions.ReservationsManage, Permissions.ReservationsCheckInOut,
            Permissions.AnalyticsView,
        ],
        [Manager] =
        [
            Permissions.RoomsRead, Permissions.RoomsManage,
            Permissions.GuestsRead, Permissions.GuestsManage,
            Permissions.ReservationsRead, Permissions.ReservationsManage, Permissions.ReservationsCheckInOut,
            Permissions.AnalyticsView,
        ],
        [Receptionist] =
        [
            Permissions.RoomsRead,
            Permissions.GuestsRead, Permissions.GuestsManage,
            Permissions.ReservationsRead, Permissions.ReservationsManage, Permissions.ReservationsCheckInOut,
        ],
        [Housekeeping] =
        [
            Permissions.RoomsRead,
            Permissions.ReservationsRead,
        ],
        [Customer] =
        [
            Permissions.ReservationsRead,
        ],
    };

    public static IReadOnlyDictionary<string, string[]> PermissionsByRole => RolePermissions;

    public static IReadOnlyList<string> All => [.. RolePermissions.Keys];
}
