using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IUserService
{
    Task<CustomResponse<IEnumerable<User>>> GetAllAsync();
    Task<CustomResponse<User>> GetByIdAsync(int id);
}
