using API_PortalSantosTech.Models;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAndPassword(string email, string password);
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
}
