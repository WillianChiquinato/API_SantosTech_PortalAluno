using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] phase progress endpoints require authentication
public class PhaseController : ControllerBase
{
    private readonly IPhaseService _phaseService;

    public PhaseController(IPhaseService phaseService)
    {
        _phaseService = phaseService;
    }

    [HttpGet]
    [Route("GetAllPhase")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _phaseService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _phaseService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetCurrentModuleUser")]
    public async Task<IActionResult> GetCurrentModuleUser([FromQuery] int enrollmentId)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _phaseService.GetCurrentModuleUserAsync(authenticatedUserId.Value, enrollmentId);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
