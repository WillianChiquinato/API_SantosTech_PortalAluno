using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IVideoRepository
{
    Task<List<Video>> GetAllAsync();
    Task<Video?> GetByIdAsync(int id);
}
