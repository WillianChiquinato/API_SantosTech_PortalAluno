using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IUserService
{
    Task<CustomResponse<User>> GetUserByEmailAndPassword(string email, string password);
    Task<CustomResponse<IEnumerable<User>>> GetAllAsync();
    Task<CustomResponse<User>> GetByIdAsync(int id);
}
