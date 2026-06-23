using Hangfire.Dashboard;
using StayFlow.Application.Common.Authorization;

namespace StayFlow.Infrastructure.Jobs;

/// <summary>
/// Guards the Hangfire dashboard. Open in development for convenience; elsewhere it requires an
/// authenticated SuperAdmin. Note the dashboard is served over cookie/session state, so a
/// production deployment would typically front it with an interactive sign-in.
/// </summary>
public sealed class HangfireDashboardAuthorizationFilter(bool allowAnonymous) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        if (allowAnonymous)
        {
            return true;
        }

        var user = context.GetHttpContext().User;
        return user.Identity?.IsAuthenticated == true && user.IsInRole(Roles.SuperAdmin);
    }
}
