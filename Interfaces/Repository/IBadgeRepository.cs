using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IBadgeRepository
{
    Task<List<Badge>> GetAllAsync();
    Task<Badge?> GetByIdAsync(int id);
    Task<List<Badge?>> GetByUserIdAsync(int userId);
    
}
