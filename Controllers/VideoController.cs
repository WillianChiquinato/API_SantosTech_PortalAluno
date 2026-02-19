using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> GetProgressUserVideos([FromQuery] int userId)
    {
        var response = await _videoService.GetProgressUserVideosAsync(userId);
        return Ok(response);
    }

    [HttpPost]
    [Route("SaveProgressVideo")]
    public async Task<IActionResult> SaveProgressVideo([FromBody] VideoProgressDTO progressData)
    {
        var response = await _videoService.SaveProgressVideoAsync(progressData);

        return response != null ? Ok(response) : BadRequest("Erro ao salvar progresso do vídeo");
    }
}
