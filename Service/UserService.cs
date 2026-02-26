using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;

namespace API_PortalSantosTech.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly ILevelUserRepository _levelUserRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IClassRepository _classRepository;
    private readonly IBadgeRepository _badgeRepository;
    private readonly ICloudflareR2Service _cloudflareR2Service;

    public UserService(ILogger<UserService> logger, IUserRepository userRepository, ILevelUserRepository levelUserRepository, IEnrollmentRepository enrollmentRepository, IClassRepository classRepository, IBadgeRepository badgeRepository, ICloudflareR2Service cloudflareR2Service)
    {
        _logger = logger;
        _userRepository = userRepository;
        _levelUserRepository = levelUserRepository;
        _enrollmentRepository = enrollmentRepository;
        _classRepository = classRepository;
        _badgeRepository = badgeRepository;
        _cloudflareR2Service = cloudflareR2Service;
    }

    public async Task<CustomResponse<User>> GetUserByEmailAndPassword(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return CustomResponse<User>.Fail("Email ou senha inválidos");

            var result = await _userRepository.GetUserByEmail(email);

            if (result == null || string.IsNullOrWhiteSpace(result.PasswordHash))
                return CustomResponse<User>.Fail("Email ou senha inválidos");

            var isValidPassword = VerifyPassword(password, result.PasswordHash);

            return !isValidPassword
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

    public async Task<CustomResponse<UserProfileDataDTO>> GetProfileDataAsync(int id)
    {
        try
        {
            var resultUser = await _userRepository.GetByIdAsync(id);
            //Mapear com UserRole.
            var roleUser = (int?)resultUser?.Role == 1 ? "Student" : (int?)resultUser?.Role == 2 ? "Teacher" : "Admin";

            var resultPoints = await _userRepository.GetUserPointsAsync(id);
            var resultLevel = await _levelUserRepository.GetAllAsync();

            var userLevel = resultLevel
                .OrderByDescending(l => l.PointsRequired)
                .FirstOrDefault(l => resultPoints >= l.PointsRequired);

            if (userLevel == null)
            {
                userLevel = resultLevel.OrderBy(l => l.PointsRequired).FirstOrDefault();
            }

            var getEnrollMentUser = await _enrollmentRepository.GetByUserIdAsync(id);
            var GetClassInUser = await _classRepository.GetByIdAsync(getEnrollMentUser?.ClassId ?? 0);

            var UserBadges = await _badgeRepository.GetByUserIdAsync(id);

            return resultUser == null
                ? CustomResponse<UserProfileDataDTO>.Fail("Usuario não encontrado")
                : CustomResponse<UserProfileDataDTO>.SuccessTrade(new UserProfileDataDTO
                {
                    Id = resultUser.Id,
                    Email = resultUser.Email,
                    Name = resultUser.Name,
                    Bio = resultUser.Bio,
                    Role = roleUser,
                    PasswordHash = resultUser.PasswordHash,
                    ProfilePictureUrl = resultUser.ProfilePictureUrl,
                    CoverPictureUrl = resultUser.CoverPhotoUrl,
                    LevelUser = userLevel!.Name,
                    PointsQuantity = resultPoints,
                    Class = new ClassDTO
                    {
                        Id = GetClassInUser?.Id ?? 0,
                        Name = GetClassInUser?.Name ?? "Sem classe"
                    },
                    StudentBadges = UserBadges?.Select(b => new BadgeDTO
                    {
                        Id = b!.Id,
                        Name = b.Name,
                        Description = b.Description,
                        IconURL = b.IconUrl
                    }).ToList()
                });
        }
        catch (Exception e)
        {
            return CustomResponse<UserProfileDataDTO>.Fail("Ocorreu um erro", e.Message);
        }
    }

    public async Task<CustomResponse<User>> UpdateUserAsync(UpdateUserRequest request)
    {
        try
        {
            var profilePictureUrl = await ResolveUserImageUrlAsync(request.ProfilePictureUrl, "users/profile");
            var coverPictureUrl = await ResolveUserImageUrlAsync(request.CoverPictureUrl, "users/cover");
            string? passwordHash = null;

            if (String.IsNullOrWhiteSpace(request.Password))
            {
                var getPasswordHash = await _userRepository.GetByIdAsync(request.Id);
                passwordHash = getPasswordHash?.PasswordHash;
            }
            else
            {
                passwordHash = ResolvePasswordHash(request.Password);
            }

            var userToUpdate = new User
            {
                Id = request.Id,
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = (UserRole)(request.Role ?? 0),
                Bio = request.Bio,
                ProfilePictureUrl = profilePictureUrl,
                CoverPhotoUrl = coverPictureUrl,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userRepository.UpdateUserAsync(userToUpdate);

            return result == null
                ? CustomResponse<User>.Fail("Falha ao atualizar o usuário")
                : CustomResponse<User>.SuccessTrade(result);
        }
        catch (Exception e)
        {
            return CustomResponse<User>.Fail("Ocorreu um erro", e.Message);
        }
    }

    private async Task<string?> ResolveUserImageUrlAsync(string? imageInput, string folder)
    {
        if (string.IsNullOrWhiteSpace(imageInput))
            return imageInput;

        if (!imageInput.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            return imageInput;

        return await _cloudflareR2Service.UploadBase64ImageAsync(imageInput, folder);
    }

    private static string? ResolvePasswordHash(string? passwordInput)
    {
        if (string.IsNullOrWhiteSpace(passwordInput))
            return null;

        if (IsBcryptHash(passwordInput))
            return passwordInput;

        return BCrypt.Net.BCrypt.HashPassword(passwordInput);
    }

    private static bool VerifyPassword(string rawPassword, string storedHash)
    {
        if (IsBcryptHash(storedHash))
            return BCrypt.Net.BCrypt.Verify(rawPassword, storedHash);

        return false;
    }

    private static bool IsBcryptHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 20)
            return false;

        return value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$");
    }

    public async Task<CustomResponse<ConfigsDTO>> GetConfigsAsync(int id)
    {
        try
        {
            var result = await _userRepository.GetConfigsAsync(id);

            return result == null
                ? CustomResponse<ConfigsDTO>.Fail("Configurações do usuário não encontradas")
                : CustomResponse<ConfigsDTO>.SuccessTrade(result);
        }
        catch (Exception e)
        {
            return CustomResponse<ConfigsDTO>.Fail("Ocorreu um erro", e.Message);
        }
    }

    public async Task<CustomResponse<ConfigsDTO>> CreateNewConfigAsync(int id)
    {
        try
        {
            var result = await _userRepository.CreateNewConfigAsync(id);

            return result == null
                ? CustomResponse<ConfigsDTO>.Fail("Falha ao criar nova configuração")
                : CustomResponse<ConfigsDTO>.SuccessTrade(result);
        }
        catch (Exception e)
        {
            return CustomResponse<ConfigsDTO>.Fail("Ocorreu um erro", e.Message);
        }
    }

    public async Task<CustomResponse<UpdateConfigRequest>> UpdateConfigsAsync(UpdateConfigRequest request)
    {
        try
        {
            var result = await _userRepository.GetConfigsAsync(request.UserId);

            if (result == null)
                return CustomResponse<UpdateConfigRequest>.Fail("Configurações do usuário não encontradas");

            result.ReceiveEmailNotifications = request.ReceiveEmailNotifications;
            result.DarkModeEnabled = request.DarkModeEnabled;
            result.ReportFrequency = request.ReportFrequency;
            result.AcessibilityMode = request.AcessibilityMode;
            result.PreferredLanguage = request.PreferredLanguage;

            var updateResult = await _userRepository.UpdateConfigsAsync(request);

            if (updateResult == null)
                return CustomResponse<UpdateConfigRequest>.Fail("Falha ao atualizar as configurações do usuário");

            return CustomResponse<UpdateConfigRequest>.SuccessTrade(request);
        }
        catch (Exception e)
        {
            return CustomResponse<UpdateConfigRequest>.Fail("Ocorreu um erro", e.Message);
        }
    }

    public async Task<CustomResponse<bool>> SendEmailVerifyAsync(string email)
    {
        try
        {
            var result = await _userRepository.SendEmailVerifyAsync(email);

            return result
                ? CustomResponse<bool>.SuccessTrade(true)
                : CustomResponse<bool>.Fail("Falha ao enviar email de verificação");
        }
        catch (Exception e)
        {
            return CustomResponse<bool>.Fail("Ocorreu um erro", e.Message);
        }
    }
}
