using API_PortalSantosTech.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExerciseController : ControllerBase
{
    private readonly IExerciseService _exerciseService;

    public ExerciseController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    [HttpGet]
    [Route("GetAllExercises")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _exerciseService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetExerciseById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _exerciseService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
