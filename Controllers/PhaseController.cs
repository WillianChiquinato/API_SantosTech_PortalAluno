using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [Route("GetCurrentPhaseUser")]
    public async Task<IActionResult> GetCurrentPhaseUser([FromQuery] int userId)
    {
        var response = await _phaseService.GetCurrentPhaseUserAsync(userId);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
