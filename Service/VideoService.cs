using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class VideoService : IVideoService
{
    private readonly ILogger<VideoService> _logger;
    private readonly IVideoRepository _videoRepository;

    public VideoService(ILogger<VideoService> logger, IVideoRepository videoRepository)
    {
        _logger = logger;
        _videoRepository = videoRepository;
    }

    public async Task<CustomResponse<IEnumerable<Video>>> GetAllAsync()
    {
        var result = await _videoRepository.GetAllAsync();
        return CustomResponse<IEnumerable<Video>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<Video>> GetByIdAsync(int id)
    {
        var result = await _videoRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<Video>.Fail("Video not found")
            : CustomResponse<Video>.SuccessTrade(result);
    }
}
