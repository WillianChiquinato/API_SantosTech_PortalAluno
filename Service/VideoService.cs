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

    public async Task<CustomResponse<IEnumerable<VideoProgressDTO>>> GetProgressUserVideosAsync(int userId)
    {
        try
        {
            var result = await _videoRepository.GetProgressUserVideosAsync(userId);
            return CustomResponse<IEnumerable<VideoProgressDTO>>.SuccessTrade(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching video progress for user ID: {UserId}", userId);
            return CustomResponse<IEnumerable<VideoProgressDTO>>.Fail("An error occurred while fetching video progress");
        }
    }

    public async Task<CustomResponse<VideoProgressDTO>> SaveProgressVideoAsync(VideoProgressDTO progressData)
    {
        try
        {
            var existingProgress = await _videoRepository.GetProgressUserVideosAsync(progressData.UserId);
            var progressToUpdate = existingProgress.FirstOrDefault(p => p.VideoId == progressData.VideoId);

            if (progressToUpdate != null)
            {
                var update = await _videoRepository.UpdateProgressVideoAsync(progressData);

                await _logsRepository.AddLogsAsync(new Logs
                {
                    UserId = progressData.UserId,
                    Action = $"Update de progresso em video ID: {progressData.VideoId}",
                    Message = $"Progresso de Update: WatchedSeconds={update.WatchSeconds}, IsCompleted={update.IsCompleted}",
                    EntityName = "ProgressVideoStudent",
                    LogDate = DateTime.UtcNow
                });
            }
            else
            {
                await _videoRepository.AddProgressVideoAsync(progressData);

                await _logsRepository.AddLogsAsync(new Logs
                {
                    UserId = progressData.UserId,
                    Action = $"Adicionado progresso de video para video ID: {progressData.VideoId}",
                    Message = $"Progresso Adicionado: WatchedSeconds={progressData.WatchSeconds}, IsCompleted={progressData.IsCompleted}",
                    EntityName = "ProgressVideoStudent",
                    LogDate = DateTime.UtcNow
                });
            }

            return CustomResponse<VideoProgressDTO>.SuccessTrade(progressData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving video progress for user ID: {UserId} and video ID: {VideoId}", progressData.UserId, progressData.VideoId);
            return CustomResponse<VideoProgressDTO>.Fail("An error occurred while saving video progress");
        }
    }
}
