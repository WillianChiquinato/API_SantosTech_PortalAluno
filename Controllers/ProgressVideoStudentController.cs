using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressVideoStudentController : ControllerBase
{
    private readonly IProgressVideoStudentService _progressVideoStudentService;

    public ProgressVideoStudentController(IProgressVideoStudentService progressVideoStudentService)
    {
        _progressVideoStudentService = progressVideoStudentService;
    }

    [HttpGet]
    [Route("GetAllProgressVideoStudents")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _progressVideoStudentService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetProgressVideoStudentById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _progressVideoStudentService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
