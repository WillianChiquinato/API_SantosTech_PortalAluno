using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IMaterialService
{
    Task<CustomResponse<IEnumerable<Material>>> GetAllAsync();
    Task<CustomResponse<Material>> GetByIdAsync(int id);
}
