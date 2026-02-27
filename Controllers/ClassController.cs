using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    [Route("GetAllClasses")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _classService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetClassById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _classService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetIslandsByUserIdAndCurrentModule")]
    public async Task<IActionResult> GetIslandsByUserIdAndCurrentModule([FromQuery] int userId, [FromQuery] int phaseId)
    {
        var response = await _classService.GetIslandsByUserIdAndCurrentModuleAsync(userId, phaseId);
        return Ok(response);
    }
}
