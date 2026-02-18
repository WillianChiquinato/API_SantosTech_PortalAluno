using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IClassRepository
{
    Task<List<Class>> GetAllAsync();
    Task<Class?> GetByIdAsync(int id);
    Task<Module?> GetCurrentModuleByClassIdAsync(int classId);
}
