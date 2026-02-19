using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IVideoService
{
    Task<CustomResponse<IEnumerable<Video>>> GetAllAsync();
    Task<CustomResponse<Video>> GetByIdAsync(int id);
    Task<CustomResponse<IEnumerable<VideoProgressDTO>>> GetProgressUserVideosAsync(int userId);
    Task<CustomResponse<VideoProgressDTO>> SaveProgressVideoAsync(VideoProgressDTO progressData);
}
