using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
// [Authorize]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProgressController> _logger;
    private readonly IProgressService _progressService;

    public ProgressController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ProgressController> logger,
        IProgressService progressService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _progressService = progressService;
    }

    [HttpPost]
    [Route("UpdateGoalActivedProgress")]
    public async Task<IActionResult> UpdateGoalActivedProgress([FromBody] UpdateGoalProgressRequest request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var updateProgress = await _progressService.UpdateGoalProgressAsync(authenticatedUserId.Value, request.GoalType, request.RewardType);

        return updateProgress.Success ? Ok(updateProgress) : NotFound(updateProgress);
    }

    [HttpPost]
    [Route("EvaluateProgress")]
    public async Task<IActionResult> EvaluateProgress([FromBody] EvaluateProgressRequest request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var result = await _progressService.EvaluateProgressAsync(authenticatedUserId.Value, request.GoalRewardId, request.RewardType);

        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet]
    [Route("GetAllExercisesProgress")]
    public async Task<IActionResult> GetAllExercisesProgress()
    {
        var response = await _progressService.GetAllExerciseAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetExerciseProgressById")]
    public async Task<IActionResult> GetExerciseProgressById([FromQuery] int id)
    {
        var response = await _progressService.GetExerciseByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetAllStudentPhasesProgress")]
    public async Task<IActionResult> GetAllStudentPhasesProgress()
    {
        var response = await _progressService.GetAllStudentPhasesAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetStudentPhaseProgressById")]
    public async Task<IActionResult> GetStudentPhaseProgressById([FromQuery] int id)
    {
        var response = await _progressService.GetStudentPhaseByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetAllVideoStudentsProgress")]
    public async Task<IActionResult> GetAllVideoStudentsProgress()
    {
        var response = await _progressService.GetAllVideoStudentsAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetVideoStudentProgressById")]
    public async Task<IActionResult> GetVideoStudentProgressById([FromQuery] int id)
    {
        var response = await _progressService.GetVideoStudentByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetProgressUserVideos")]
    public async Task<IActionResult> GetProgressUserVideos()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _progressService.GetProgressUserVideosAsync(authenticatedUserId.Value);
        return Ok(response);
    }

    [HttpPost]
    [Route("SaveProgressVideo")]
    public async Task<IActionResult> SaveProgressVideo([FromBody] VideoProgressDTO progressData)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        // [SEC] ignore client-supplied user id and scope progress writes to the authenticated user
        progressData.UserId = authenticatedUserId.Value;
        var response = await _progressService.SaveProgressVideoAsync(progressData);

        return response != null ? Ok(response) : BadRequest("Erro ao salvar progresso do video");
    }

    [HttpGet]
    [Route("GetProgressUserPaidCourses")]
    public async Task<IActionResult> GetProgressUserPaidCourses()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _progressService.GetProgressUserPaidCoursesAsync(authenticatedUserId.Value);
        return response.Success ? Ok(response) : NotFound(response);
    }
}