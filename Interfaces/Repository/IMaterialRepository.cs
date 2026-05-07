using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IMaterialRepository
{
    Task<List<Material>> GetAllAsync(int enrollmentId);
    Task<Material?> GetByIdAsync(int id);
}
