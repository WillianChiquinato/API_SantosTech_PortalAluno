using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

public abstract class SantosBaseController : ControllerBase
{
    private const string LocalUserIdKey = "LocalUserId";

    protected async Task<int?> GetLocalUserIdAsync()
    {
        if (HttpContext.Items.TryGetValue(LocalUserIdKey, out var cached))
            return cached as int?;

        var santosUser = HttpContext.GetSantosUser();
        if (santosUser is null) return null;

        var repo = HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetUserByEmail(santosUser.Email);
        HttpContext.Items[LocalUserIdKey] = user?.Id;
        return user?.Id;
    }
}
