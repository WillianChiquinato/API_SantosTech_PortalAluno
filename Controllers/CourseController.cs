using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    [Route("GetAllCourses")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _courseService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetFullCoursesPaid")]
    public async Task<IActionResult> GetFullCoursesPaid()
    {
        var response = await _courseService.GetFullCoursesPaidAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetCourseById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _courseService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetProgressUserPaidCourses")]
    public async Task<IActionResult> GetProgressUserPaidCourses([FromQuery] int userId)
    {
        var response = await _courseService.GetProgressUserPaidCoursesAsync(userId);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
