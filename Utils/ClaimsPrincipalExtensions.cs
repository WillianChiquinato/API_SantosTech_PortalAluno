using System.Security.Claims;

namespace API_PortalSantosTech.Utils;

public static class ClaimsPrincipalExtensions
{
    public static int? GetAuthenticatedUserId(this ClaimsPrincipal user)
    {
        var claimValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}
