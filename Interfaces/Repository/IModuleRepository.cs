using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IModuleRepository
{
    Task<List<Module>> GetAllAsync();
    Task<Module?> GetByIdAsync(int id);
}
