using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/FinalChallenge")]
public class TeamsChallengerController : SantosBaseController
{
    private readonly ITeamsChallengerService _teamsChallengerService;

    public TeamsChallengerController(ITeamsChallengerService teamsChallengerService)
    {
        _teamsChallengerService = teamsChallengerService;
    }

    [HttpGet]
    [Route("GetLiveSnapshot")]
    public async Task<IActionResult> GetLiveSnapshot([FromQuery] int eventId)
    {
        var authenticatedUserId = await GetLocalUserIdAsync();
        var snapshot = await _teamsChallengerService.GetLiveSnapshotAsync(eventId, authenticatedUserId);

        return Ok(snapshot);
    }

    [HttpGet]
    [Route("GetLeaderboard")]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int eventId)
    {
        var authenticatedUserId = await GetLocalUserIdAsync();
        var leaderboard = await _teamsChallengerService.GetLeaderboardAsync(eventId, authenticatedUserId);

        return Ok(leaderboard);
    }

    [HttpGet]
    [Route("GetClanTasks")]
    public async Task<IActionResult> GetClanTasks([FromQuery] int eventId, [FromQuery] int clanId)
    {
        var tasks = await _teamsChallengerService.GetClanTasksAsync(eventId, clanId);

        return Ok(tasks);
    }

    [HttpGet]
    [Route("GetActivityEvent")]
    public async Task<IActionResult> GetActivityEvent()
    {
        var authenticatedUserId = await GetLocalUserIdAsync();
        var snapshot = await _teamsChallengerService.GetActivityEventAsync(authenticatedUserId);

        return Ok(snapshot);
    }

    [HttpGet]
    [Route("GetActivityFeed")]
    public async Task<IActionResult> GetActivityFeed([FromQuery] int eventId)
    {
        var activityFeed = await _teamsChallengerService.GetActivityFeedAsync(eventId);

        return Ok(activityFeed);
    }

    [HttpPost]
    [Route("SubmitAnswer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] FinalChallengeAnswerRequest answerRequest)
    {
        var authenticatedUserId = await GetLocalUserIdAsync();
        var result = await _teamsChallengerService.SubmitAnswerAsync(answerRequest, authenticatedUserId);

        return Ok(result);
    }

    [HttpGet]
    [Route("GetTeamForPlayerRelationship")]
    public async Task<IActionResult> GetTeamForPlayerRelationship([FromQuery] int classId, [FromQuery] int moduleId)
    {
        var authenticatedUserId = await GetLocalUserIdAsync();
        var result = await _teamsChallengerService.GetTeamForPlayerRelationship(classId, moduleId, authenticatedUserId);
        return Ok(result);
    }

    [HttpPost]
    [Route("JoinTeam")]
    public async Task<IActionResult> JoinTeam([FromBody] CreateTeamRequest createTeamRequest)
    {
        var authenticatedUserId = await GetLocalUserIdAsync();
        var result = await _teamsChallengerService.CreateTeamAsync(createTeamRequest, authenticatedUserId);

        return Ok(result);
    }

    [HttpGet]
    [Route("GetRankingToFinalChallenge")]
    public async Task<IActionResult> GetRankingToFinalChallenge([FromQuery] int eventId)
    {
        var authenticatedUserId = await GetLocalUserIdAsync();
        var result = await _teamsChallengerService.GetRankingToFinalChallengeAsync(eventId, authenticatedUserId);
        return Ok(result);
    }

    [HttpGet]
    [Route("FinalChallengeAccess")]
    public async Task<IActionResult> FinalChallengeAccess([FromQuery] int courseId)
    {
        var result = await _teamsChallengerService.FinalChallengeAccessAsync(courseId);
        return Ok(result);
    }
}
