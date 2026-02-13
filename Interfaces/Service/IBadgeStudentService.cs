using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IBadgeStudentService
{
    Task<CustomResponse<IEnumerable<BadgeStudent>>> GetAllAsync();
    Task<CustomResponse<BadgeStudent>> GetByIdAsync(int id);
}
