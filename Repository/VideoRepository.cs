using API_PortalSantosTech.Data;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace API_PortalSantosTech.Repository;

public class VideoRepository : IVideoRepository
{
    private readonly AppDbContext _efDbContext;

    public VideoRepository(AppDbContext efDbContext)
    {
        _efDbContext = efDbContext;
    }

    public async Task<VideoProgressDTO> AddProgressVideoAsync(VideoProgressDTO progressData)
    {
        var newProgress = new ProgressVideoStudent
        {
            UserId = progressData.UserId,
            VideoId = progressData.VideoId,
            WatchedSeconds = progressData.WatchSeconds,
            IsCompleted = progressData.IsCompleted,
            LastWatchedAt = DateTime.UtcNow
        };

        _efDbContext.ProgressVideoStudents.Add(newProgress);
        await _efDbContext.SaveChangesAsync();

        return new VideoProgressDTO
        {
            UserId = newProgress.UserId,
            VideoId = newProgress.VideoId,
            WatchSeconds = newProgress.WatchedSeconds,
            IsCompleted = newProgress.IsCompleted,
            LastWatched = newProgress.LastWatchedAt ?? DateTime.MinValue
        };
    }

    public async Task<List<Video>> GetAllAsync()
    {
        return await _efDbContext.Videos.AsNoTracking().ToListAsync();
    }

    public async Task<Video?> GetByIdAsync(int id)
    {
        return await _efDbContext.Videos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<VideoProgressDTO>> GetProgressUserVideosAsync(int userId)
    {
        return _efDbContext.ProgressVideoStudents
            .Where(vp => vp.UserId == userId)
            .Select(vp => new VideoProgressDTO
            {
                VideoId = vp.VideoId,
                UserId = vp.UserId,
                WatchSeconds = vp.WatchedSeconds,
                IsCompleted = vp.IsCompleted,
                LastWatched = vp.LastWatchedAt ?? DateTime.MinValue
            })
            .ToListAsync();
    }

    public async Task<VideoProgressDTO> UpdateProgressVideoAsync(VideoProgressDTO progressData)
    {
        var existingProgress = await _efDbContext.ProgressVideoStudents
            .FirstOrDefaultAsync(p => p.UserId == progressData.UserId && p.VideoId == progressData.VideoId);

        if (existingProgress != null)
        {
            existingProgress.WatchedSeconds = progressData.WatchSeconds;
            existingProgress.IsCompleted = progressData.IsCompleted;
            existingProgress.LastWatchedAt = DateTime.UtcNow;

            await _efDbContext.SaveChangesAsync();
        }

        return new VideoProgressDTO
        {
            UserId = existingProgress?.UserId ?? progressData.UserId,
            VideoId = existingProgress?.VideoId ?? progressData.VideoId,
            WatchSeconds = existingProgress?.WatchedSeconds ?? progressData.WatchSeconds,
            IsCompleted = existingProgress?.IsCompleted ?? progressData.IsCompleted,
            LastWatched = existingProgress?.LastWatchedAt ?? DateTime.MinValue
        };
    }
}
