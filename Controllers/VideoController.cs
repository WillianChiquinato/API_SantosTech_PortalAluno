using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] video progress is scoped to the authenticated user
public class VideoController : ControllerBase
{
    private readonly IVideoService _videoService;

    public VideoController(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [HttpGet]
    [Route("GetAllVideos")]
    public async Task<IActionResult> GetAll()
    {
        var response = await _videoService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet]
    [Route("GetVideoById")]
    public async Task<IActionResult> GetById([FromQuery] int id)
    {
        var response = await _videoService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet]
    [Route("GetProgressUserVideos")]
    public async Task<IActionResult> GetProgressUserVideos()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _videoService.GetProgressUserVideosAsync(authenticatedUserId.Value);
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
        var response = await _videoService.SaveProgressVideoAsync(progressData);

        return response != null ? Ok(response) : BadRequest("Erro ao salvar progresso do video");
    }
}
