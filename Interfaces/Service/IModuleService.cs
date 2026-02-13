using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IModuleService
{
    Task<CustomResponse<IEnumerable<Module>>> GetAllAsync();
    Task<CustomResponse<Module>> GetByIdAsync(int id);
}
