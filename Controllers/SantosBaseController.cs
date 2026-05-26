using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

public abstract class SantosBaseController : ControllerBase
{
    protected async Task<int?> GetLocalUserIdAsync()
    {
        var santosUser = HttpContext.GetSantosUser();
        if (santosUser is null) return null;

        var repo = HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await repo.GetUserByEmail(santosUser.Email);
        return user?.Id;
    }
}
