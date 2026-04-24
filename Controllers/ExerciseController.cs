using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] exercise endpoints require an authenticated user context
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
    public async Task<IActionResult> GetDailyTasksForPhase([FromQuery] int phaseId)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _exerciseService.GetDailyTasksForPhaseAsync(phaseId, authenticatedUserId.Value);
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
        if (submission == null || submission.Count == 0)
            return BadRequest(new { Success = false, Message = "Submission data is required." });

        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        foreach (var item in submission)
        {
            // [SEC] bind submissions to the authenticated user and ignore client-supplied ids
            item.UserId = authenticatedUserId.Value;

            var response = await _exerciseService.SubmitExerciseAnswersAsync(item);
            if (!response.Success)
                return BadRequest(response);
        }

        return Ok(new { Success = true, Message = "Respostas enviadas com sucesso." });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")] // [SEC] phase flow synchronization is a privileged operation
    [Route("SyncMainFlowByPhase")]
    public async Task<IActionResult> SyncMainFlowByPhase([FromQuery] int phaseId)
    {
        var response = await _exerciseService.SyncMainExercisesIntoPhaseFlowsAsync(phaseId);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet]
    [Route("VerifyExistingAnswers")]
    public async Task<IActionResult> VerifyExistingAnswers([FromQuery] int exerciseId)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _exerciseService.VerifyExistingAnswersAsync(exerciseId, authenticatedUserId.Value);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetExercisesAnsweredCategoriesByUser")]
    public async Task<IActionResult> GetExercisesAnsweredCategoriesByUser()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _exerciseService.GetExercisesAnsweredByCategoryForUserAsync(authenticatedUserId.Value);
        return response.Success ? Ok(response) : NotFound(response);
    }
}
