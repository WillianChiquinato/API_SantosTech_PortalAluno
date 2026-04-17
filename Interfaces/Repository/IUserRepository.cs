using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces.Repository;

public interface IUserRepository
{
    Task<User?> GetUserByEmail(string email);
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<UserIdentity?> GetUserIdentityAsync(string provider, string providerUserId);
    Task<UserIdentity?> GetUserIdentityByUserIdAndProviderAsync(int userId, string provider);
    Task<UserIdentity> CreateUserIdentityAsync(UserIdentity identity);
    Task<float> GetUserPointsAsync(int userId);
    Task<User> UpdateUserAsync(User user);
    Task<ConfigsDTO> GetConfigsAsync(int userId);
    Task<ConfigsDTO> CreateNewConfigAsync(int userId);
    Task<ConfigsDTO> UpdateConfigsAsync(UpdateConfigRequest request);
    Task<bool> SendEmailVerifyAsync(string email);
    Task<bool> ConfirmEmailVerifyAsync(string email, string code);
    Task<PasswordRecoveryResult> SendPasswordRecoveryEmailAsync(string email);
    Task UpdatePasswordHashAsync(string email, string? passwordHash);
    Task<IEnumerable<UserWithAbilityDTO>> GetUserAbilitiesAsync(int userId);
    Task<bool> UpdateLastSeenAsync(int userId);
}
