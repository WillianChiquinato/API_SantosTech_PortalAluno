using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(ILogger<UserService> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<CustomResponse<IEnumerable<User>>> GetAllAsync()
    {
        var result = await _userRepository.GetAllAsync();
        return CustomResponse<IEnumerable<User>>.SuccessTrade(result);
    }

    public async Task<CustomResponse<User>> GetByIdAsync(int id)
    {
        var result = await _userRepository.GetByIdAsync(id);
        return result == null
            ? CustomResponse<User>.Fail("User not found")
            : CustomResponse<User>.SuccessTrade(result);
    }
}
