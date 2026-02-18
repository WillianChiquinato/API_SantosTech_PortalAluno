using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModuleController : ControllerBase
{
    private readonly IModuleService _moduleService;

    public ModuleController(IModuleService moduleService)
    {
        _moduleService = moduleService;
    }

    [HttpGet]
    [Route("GetAllModules")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _moduleService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetModuleById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _moduleService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
