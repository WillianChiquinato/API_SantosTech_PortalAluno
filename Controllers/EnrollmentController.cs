using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [Route("GetAllEnrollments")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _enrollmentService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetEnrollmentById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _enrollmentService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
