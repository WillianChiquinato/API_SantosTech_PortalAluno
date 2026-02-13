using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IBadgeService
{
    Task<CustomResponse<IEnumerable<Badge>>> GetAllAsync();
    Task<CustomResponse<Badge>> GetByIdAsync(int id);
}
