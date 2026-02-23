using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
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

    [HttpGet]
    [Route("GetDailyTasksForPhase")]
    public async Task<IActionResult> GetDailyTasksForPhase([FromQuery] int phaseId, [FromQuery] int userId)
    {
        var response = await _exerciseService.GetDailyTasksForPhaseAsync(phaseId, userId);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetQuestionsOptionsForExercise")]
    public async Task<IActionResult> GetQuestionsOptionsForExercise([FromQuery] int exerciseId)
    {
        var response = await _exerciseService.GetQuestionsOptionsForExerciseAsync(exerciseId);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [Route("SubmitExerciseAnswers")]
    public async Task<IActionResult> SubmitExerciseAnswers([FromBody] List<ExerciseSubmissionDTO> submission)
    {
        if (submission == null)
        {
            return BadRequest(new { Success = false, Message = "Submission data is required." });
        }

        foreach (var item in submission)
        {
            var response = await _exerciseService.SubmitExerciseAnswersAsync(item);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        return Ok(new { Success = true, Message = "Respostas enviadas com sucesso." });
    }
}
