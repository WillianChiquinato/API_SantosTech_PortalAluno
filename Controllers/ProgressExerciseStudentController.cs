using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressExerciseStudentController : ControllerBase
{
    private readonly IProgressExerciseStudentService _progressExerciseStudentService;

    public ProgressExerciseStudentController(IProgressExerciseStudentService progressExerciseStudentService)
    {
        _progressExerciseStudentService = progressExerciseStudentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _progressExerciseStudentService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _progressExerciseStudentService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
