using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
}
