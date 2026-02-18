using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialController : ControllerBase
{
    private readonly IMaterialService _materialService;

    public MaterialController(IMaterialService materialService)
    {
        _materialService = materialService;
    }

    [HttpGet]
    [Route("GetAllMaterials")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _materialService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetMaterialById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _materialService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
