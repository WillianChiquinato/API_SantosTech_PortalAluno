using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class VideoService : IVideoService
{
    private readonly ILogger<VideoService> _logger;
    private readonly IVideoRepository _videoRepository;
    private readonly ILogsRepository _logsRepository;

    public VideoService(ILogger<VideoService> logger, IVideoRepository videoRepository, ILogsRepository logsRepository)
    {
        _logger = logger;
        _videoRepository = videoRepository;
        _logsRepository = logsRepository;
    }

    public async Task<CustomResponse<IEnumerable<Video>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all videos");
            var result = await _videoRepository.GetAllAsync();
            return CustomResponse<IEnumerable<Video>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching videos");
            return CustomResponse<IEnumerable<Video>>.Fail("An error occurred while fetching videos");
        }
    }

    public async Task<CustomResponse<Video>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _videoRepository.GetByIdAsync(id);
            return result == null
                ? CustomResponse<Video>.Fail("Video not found")
                : CustomResponse<Video>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching video by ID: {Id}", id);
            return CustomResponse<Video>.Fail("An error occurred while fetching the video");
        }
    }
}
