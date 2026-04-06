using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] all endpoints require authenticated user
public class PointController : ControllerBase
{
    private readonly IPointService _pointService;

    public PointController(IPointService pointService)
    {
        _pointService = pointService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")] // [SEC] full point ledger is restricted to privileged roles
    [Route("GetAllPoints")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _pointService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")] // [SEC] direct point lookup is restricted to privileged roles
    [Route("GetPointById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _pointService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetRanking")]
    public async Task<IActionResult> GetRanking()
    {
        var response = await _pointService.GetRankingAsync();
        return Ok(response);
    }

    [HttpPost]
    [Route("AddPointsForUser")]
    public async Task<IActionResult> AddPointsForUser([FromBody] AddPointsDTO redeemPoints)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        // [SEC] bind the point award to the authenticated user and compute points on the server
        redeemPoints.UserId = authenticatedUserId.Value;
        var response = await _pointService.AddPointsForUserAsync(redeemPoints);

        return Ok(response);
    }
}
