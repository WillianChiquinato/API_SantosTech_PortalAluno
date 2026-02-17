using System.Security.Claims;
using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    // [Authorize]
    [Route("Logged")]
    public async Task<IActionResult> Logged()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        var user = await _userService.GetByIdAsync(int.Parse(userIdClaim));

        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            user
        });
    }
}