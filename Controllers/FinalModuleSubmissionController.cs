using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinalModuleSubmissionController : ControllerBase
{
    private readonly IFinalModuleSubmissionService _finalModuleSubmissionService;

    public FinalModuleSubmissionController(IFinalModuleSubmissionService finalModuleSubmissionService)
    {
        _finalModuleSubmissionService = finalModuleSubmissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _finalModuleSubmissionService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _finalModuleSubmissionService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
