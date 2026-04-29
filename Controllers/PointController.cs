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
    public async Task<IActionResult> GetAvailableRankingPerCategory(
        [FromQuery] string? category,
        [FromQuery] int? limit,
        [FromQuery] int offset = 0)
    {
        var response = await _pointService.GetAvailableRankingPerCategoryAsync(category, limit, offset);
        
        return Ok(response);
    }

    [HttpGet]
    [Route("RankingCategories")]
    public async Task<IActionResult> GetRankingCategories()
    {
        var response = await _pointService.GetRankingCategoriesAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetRankingEvent")]
    public async Task<IActionResult> GetRankingEvent([FromQuery] int eventType)
    {
        var response = await _pointService.GetRankingEventAsync(eventType);
        return Ok(response);
    }

    [HttpGet]
    [Route("RankingEventHistory")]
    public async Task<IActionResult> GetRankingEventHistory(
        [FromQuery] int? eventType,
        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0)
    {
        var response = await _pointService.GetRankingEventHistoryAsync(eventType, limit, offset);
        return Ok(response);
    }

    [HttpGet]
    [Route("RankingEvent/{id:int}")]
    public async Task<IActionResult> GetRankingEventById(int id)
    {
        var response = await _pointService.GetRankingEventByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [Route("RankingEvent")]
    public async Task<IActionResult> CreateRankingEvent([FromBody] RankingEventUpsertDTO request)
    {
        var response = await _pointService.CreateRankingEventAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPut]
    [Route("RankingEvent/{id:int}")]
    public async Task<IActionResult> UpdateRankingEvent(int id, [FromBody] RankingEventUpsertDTO request)
    {
        var response = await _pointService.UpdateRankingEventAsync(id, request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete]
    [Route("RankingEvent/{id:int}")]
    public async Task<IActionResult> DeleteRankingEvent(int id)
    {
        var response = await _pointService.DeleteRankingEventAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [Route("ScheduleRankingEvent")]
    public async Task<IActionResult> ScheduleRankingEvent([FromBody] int eventId)
    {
        var response = await _pointService.ScheduleRankingEventAsync(eventId);
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
