using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IPhaseService
{
    Task<CustomResponse<IEnumerable<Phase>>> GetAllAsync();
    Task<CustomResponse<Phase>> GetByIdAsync(int id);
}
