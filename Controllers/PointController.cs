using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointController : ControllerBase
{
    private readonly IPointService _pointService;

    public PointController(IPointService pointService)
    {
        _pointService = pointService;
    }

    [HttpGet]
    [Route("GetAllPoints")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _pointService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
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
        var response = await _pointService.AddPointsForUserAsync(redeemPoints);
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
