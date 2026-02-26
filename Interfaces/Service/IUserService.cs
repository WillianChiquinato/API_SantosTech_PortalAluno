using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Interfaces;

public interface IUserService
{
    Task<CustomResponse<User>> GetUserByEmailAndPassword(string email, string password);
    Task<CustomResponse<IEnumerable<User>>> GetAllAsync();
    Task<CustomResponse<User>> GetByIdAsync(int id);
    Task<CustomResponse<UserProfileDataDTO>> GetProfileDataAsync(int id);
    Task<CustomResponse<User>> UpdateUserAsync(UpdateUserRequest request);
    Task<CustomResponse<ConfigsDTO>> GetConfigsAsync(int id);
    Task<CustomResponse<ConfigsDTO>> CreateNewConfigAsync(int id);
    Task<CustomResponse<UpdateConfigRequest>> UpdateConfigsAsync(UpdateConfigRequest request);
    Task<CustomResponse<bool>> SendEmailVerifyAsync(string email);
}
