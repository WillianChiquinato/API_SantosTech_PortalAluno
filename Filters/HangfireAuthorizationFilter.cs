using Hangfire.Dashboard;

namespace API_PortalSantosTech.Filters;

/// <summary>
/// [SEC] Authorization filter for Hangfire dashboard — requires authenticated admin user
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        // [SEC] only authenticated admins can view Hangfire dashboard
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
    }
}
