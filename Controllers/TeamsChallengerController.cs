using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsChallengerController : ControllerBase
{
    private readonly ITeamsChallengerService _teamsChallengerService;

    public TeamsChallengerController(ITeamsChallengerService teamsChallengerService)
    {
        _teamsChallengerService = teamsChallengerService;
    }

    [HttpGet]
    [Route("GetAllTeamsChallengers")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _teamsChallengerService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetTeamChallengerById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _teamsChallengerService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
