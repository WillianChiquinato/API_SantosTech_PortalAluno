using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize]
public class PointController : ControllerBase
{
    private readonly IPointService _pointService;

    public PointController(IPointService pointService)
    {
        _pointService = pointService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    [Route("GetAllPoints")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _pointService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    [Route("GetPointById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _pointService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetRankingPoints")]
    public async Task<IActionResult> GetRanking()
    {
        var response = await _pointService.GetRankingAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetAvailableRankingPerCategory")]
    public async Task<IActionResult> GetAvailableRankingPerCategory()
    {
        var response = await _pointService.GetAvailableRankingPerCategoryAsync();
        
        return Ok(response);
    }

    [HttpGet]
    [Route("GetRankingEvent")]
    public async Task<IActionResult> GetRankingEvent([FromQuery] int eventType)
    {
        var response = await _pointService.GetRankingEventAsync(eventType);
        return Ok(response);
    }

    [HttpPost]
    [Route("AddPointsForUser")]
    public async Task<IActionResult> AddPointsForUser([FromBody] AddPointsDTO redeemPoints)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        redeemPoints.UserId = authenticatedUserId.Value;
        var response = await _pointService.AddPointsForUserAsync(redeemPoints);

        return Ok(response);
    }
}
