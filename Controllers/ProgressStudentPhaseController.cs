using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressStudentPhaseController : ControllerBase
{
    private readonly IProgressStudentPhaseService _progressStudentPhaseService;

    public ProgressStudentPhaseController(IProgressStudentPhaseService progressStudentPhaseService)
    {
        _progressStudentPhaseService = progressStudentPhaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _progressStudentPhaseService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _progressStudentPhaseService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
