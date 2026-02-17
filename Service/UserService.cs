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

    public async Task<CustomResponse<User>> GetUserByEmailAndPassword(string email, string password)
    {
        try
        {
            var hashedPassword = new HashPassword().ComputeSha256Base64(password);

            var result = await _userRepository.GetUserByEmailAndPassword(email, hashedPassword);

            return result == null
                ? CustomResponse<User>.Fail("Email ou senha inválidos")
                : CustomResponse<User>.SuccessTrade(result);
        }
        catch (Exception e)
        {
            return CustomResponse<User>.Fail("Ocorreu um erro", e.Message);
        }
    }

    public async Task<CustomResponse<IEnumerable<User>>> GetAllAsync()
    {
        try
        {
            var result = await _userRepository.GetAllAsync();

            return result == null
                ? CustomResponse<IEnumerable<User>>.Fail("Nenhum usuário encontrado")
                : CustomResponse<IEnumerable<User>>.SuccessTrade(result);
        }
        catch (Exception e)
        {
            return CustomResponse<IEnumerable<User>>.Fail("Ocorreu um erro", e.Message);
        }
    }

    public async Task<CustomResponse<User>> GetByIdAsync(int id)
    {
        try
        {
            var result = await _userRepository.GetByIdAsync(id);
            return result == null
                ? CustomResponse<User>.Fail("Usuario não encontrado")
                : CustomResponse<User>.SuccessTrade(result);
        }
        catch (Exception e)
        {
            return CustomResponse<User>.Fail("Ocorreu um erro", e.Message);
        }
    }
}
