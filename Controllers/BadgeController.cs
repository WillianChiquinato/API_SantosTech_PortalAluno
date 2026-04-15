using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
// [Authorize]
[Route("api/[controller]")]
public class BadgeController : ControllerBase
{
    private readonly IBadgeService _badgeService;

    public BadgeController(IBadgeService badgeService)
    {
        _badgeService = badgeService;
    }

    [HttpGet]
    [Route("GetAllBadges")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _badgeService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetBadgeById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _badgeService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetGoalsWithBadgesByCourseId")]
    public async Task<IActionResult> GetGoalsWithBadgesByCourseId([FromQuery] int courseId)
    {
        var response = await _badgeService.GetGoalsWithBadgesByCourseIdAsync(courseId);

        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetActivatedGoalsByUserId")]
    public async Task<IActionResult> GetActivatedGoalsByUserId()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _badgeService.GetActivatedGoalsByUserIdAsync(authenticatedUserId.Value);

        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [Route("UpdateActivatedGoalId")]
    public async Task<IActionResult> UpdateActivatedGoalId([FromQuery] int goalRewardId)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _badgeService.UpdateActivatedGoalIdAsync(goalRewardId, authenticatedUserId.Value);

        return response.Success ? Ok(response) : BadRequest(response);
    }
}
