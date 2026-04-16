using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] class endpoints are limited to authenticated users
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
    [Route("GetClassByEnrollmentId")]
    public async Task<IActionResult> GetClassByEnrollmentId([FromQuery] int enrollmentId)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _classService.GetClassByEnrollmentIdAsync(enrollmentId, authenticatedUserId.Value);
        
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetIslandsByUserIdAndCurrentModule")]
    public async Task<IActionResult> GetIslandsByUserIdAndCurrentModule([FromQuery] int phaseId)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _classService.GetIslandsByUserIdAndCurrentModuleAsync(authenticatedUserId.Value, phaseId);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetClassRoomsByClassId")]
    public async Task<IActionResult> GetClassRoomsByClassId([FromQuery] int classId)
    {
        var response = await _classService.GetClassRoomsByClassIdAsync(classId);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
