using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BadgeStudentController : ControllerBase
{
    private readonly IBadgeStudentService _badgeStudentService;

    public BadgeStudentController(IBadgeStudentService badgeStudentService)
    {
        _badgeStudentService = badgeStudentService;
    }

    [HttpGet]
    [Route("GetAllBadgeStudents")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _badgeStudentService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetBadgeStudentById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _badgeStudentService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
