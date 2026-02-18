using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface ILevelUserRepository
{
    Task<LevelUser?> GetByIdAsync(int id);
    Task<List<LevelUser>> GetAllAsync();
}