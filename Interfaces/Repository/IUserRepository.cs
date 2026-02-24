using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAndPassword(string email, string password);
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<float> GetUserPointsAsync(int userId);
    Task<User> UpdateUserAsync(User user);
    Task<ConfigsDTO> GetConfigsAsync(int userId);
    Task<ConfigsDTO> CreateNewConfigAsync(int userId);
    Task<ConfigsDTO> UpdateConfigsAsync(UpdateConfigRequest request);
}
