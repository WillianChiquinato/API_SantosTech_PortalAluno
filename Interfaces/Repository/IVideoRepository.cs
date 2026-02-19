using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IVideoRepository
{
    Task<List<Video>> GetAllAsync();
    Task<Video?> GetByIdAsync(int id);
    Task<List<VideoProgressDTO>> GetProgressUserVideosAsync(int userId);
    Task<VideoProgressDTO> UpdateProgressVideoAsync(VideoProgressDTO progressData);
    Task<VideoProgressDTO> AddProgressVideoAsync(VideoProgressDTO progressData);
}
