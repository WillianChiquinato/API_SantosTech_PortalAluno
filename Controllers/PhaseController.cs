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
    public async Task<IActionResult> GetAll()
    {
        var response = await _phaseService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _phaseService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
