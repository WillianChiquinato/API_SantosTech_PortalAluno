using System.Security.Claims;
using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet]
    // [Authorize]
    [Route("GetConfigs")]
    public async Task<IActionResult> GetConfigs([FromQuery] int userId)
    {
        var user = await _userService.GetConfigsAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    // [Authorize]
    [Route("CreateNewConfig")]
    public async Task<IActionResult> CreateNewConfig([FromQuery] int userId)
    {
        var response = await _userService.CreateNewConfigAsync(userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    // [Authorize]
    [Route("UpdateConfigs")]
    public async Task<IActionResult> UpdateConfigs([FromBody] UpdateConfigRequest request)
    {
        var response = await _userService.UpdateConfigsAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}