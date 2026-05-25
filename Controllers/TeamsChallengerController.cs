using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/FinalChallenge")]
public class TeamsChallengerController : ControllerBase
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
        var snapshot = await _teamsChallengerService.GetLiveSnapshotAsync(eventId, User.GetAuthenticatedUserId());

        return Ok(snapshot);
    }

    [HttpGet]
    [Route("GetLeaderboard")]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int eventId)
    {
        var leaderboard = await _teamsChallengerService.GetLeaderboardAsync(eventId, User.GetAuthenticatedUserId());

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
        var snapshot = await _teamsChallengerService.GetActivityEventAsync(User.GetAuthenticatedUserId());

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
        var result = await _teamsChallengerService.SubmitAnswerAsync(answerRequest, User.GetAuthenticatedUserId());

        return Ok(result);
    }

    [HttpGet]
    [Route("GetTeamForPlayerRelationship")]
    public async Task<IActionResult> GetTeamForPlayerRelationship([FromQuery] int classId, [FromQuery] int moduleId)
    {
        var result = await _teamsChallengerService.GetTeamForPlayerRelationship(classId, moduleId, User.GetAuthenticatedUserId());
        return Ok(result);
    }

    [HttpPost]
    [Route("JoinTeam")]
    public async Task<IActionResult> JoinTeam([FromBody] CreateTeamRequest createTeamRequest)
    {
        var result = await _teamsChallengerService.CreateTeamAsync(createTeamRequest, User.GetAuthenticatedUserId());

        return Ok(result);
    }

    [HttpGet]
    [Route("GetRankingToFinalChallenge")]
    public async Task<IActionResult> GetRankingToFinalChallenge([FromQuery] int eventId)
    {
        var result = await _teamsChallengerService.GetRankingToFinalChallengeAsync(eventId, User.GetAuthenticatedUserId());
        return Ok(result);
    }
}
