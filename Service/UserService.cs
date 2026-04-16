using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Response;
using API_PortalSantosTech.Utils;

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

    public async Task<CustomResponse<UserSafeDTO>> GetUserByEmailAndPassword(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return CustomResponse<UserSafeDTO>.Fail("Email ou senha inválidos");

            var result = await _userRepository.GetUserByEmail(email);

            if (result == null || string.IsNullOrWhiteSpace(result.PasswordHash))
                return CustomResponse<UserSafeDTO>.Fail("Email ou senha inválidos");

            var isValidPassword = VerifyPassword(password, result.PasswordHash);

            return !isValidPassword
                ? CustomResponse<UserSafeDTO>.Fail("Email ou senha inválidos")
                : CustomResponse<UserSafeDTO>.SuccessTrade(result.ToSafeDto());
        }
        catch (Exception e)
        {
            // [SEC] log error internally, return generic message to client
            _logger.LogError(e, "Erro interno no GetUserByEmailAndPassword");
            return CustomResponse<UserSafeDTO>.Fail("Ocorreu um erro ao processar sua requisição.");
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
            _logger.LogError(e, "Erro interno no GetAllAsync");
            return CustomResponse<IEnumerable<User>>.Fail("Ocorreu um erro ao processar sua requisicao.");
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
            // [SEC] log error internally, return generic message to client
            _logger.LogError(e, "Erro interno no GetByIdAsync");
            return CustomResponse<User>.Fail("Ocorreu um erro ao processar sua requisição.");
        }
    }

    public async Task<CustomResponse<UserProfileDataDTO>> GetProfileDataAsync(int userId, int enrollmentId)
    {
        try
        {
            var resultUser = await _userRepository.GetByIdAsync(userId);
            //Mapear com UserRole.
            var roleUser = (int?)resultUser?.Role == 1 ? "Student" : (int?)resultUser?.Role == 2 ? "Teacher" : "Admin";

            var resultPoints = await _userRepository.GetUserPointsAsync(userId);
            var resultLevel = await _levelUserRepository.GetAllAsync();

            var userLevel = resultLevel
                .OrderByDescending(l => l.PointsRequired)
                .FirstOrDefault(l => resultPoints >= l.PointsRequired);

            if (userLevel == null)
            {
                userLevel = resultLevel.OrderBy(l => l.PointsRequired).FirstOrDefault();
            }

            var getEnrollMentUser = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            var GetClassInUser = await _classRepository.GetByIdAsync(getEnrollMentUser?.ClassId ?? 0);

            var UserBadges = await _badgeRepository.GetByUserIdAsync(userId);

            return resultUser == null
                ? CustomResponse<UserProfileDataDTO>.Fail("Usuario não encontrado")
                : CustomResponse<UserProfileDataDTO>.SuccessTrade(new UserProfileDataDTO
                {
                    Id = resultUser.Id,
                    Email = resultUser.Email,
                    Name = resultUser.Name,
                    Bio = resultUser.Bio,
                    Role = roleUser,
                    // [SEC] PasswordHash removed — never send to client
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
            _logger.LogError(e, "Unexpected error in GetProfileDataAsync");
            return CustomResponse<UserProfileDataDTO>.Fail("Ocorreu um erro ao processar sua requisicao.");
        }
    }

    public async Task<CustomResponse<User>> UpdateUserAsync(UpdateUserRequest request, int authenticatedUserId)
    {
        try
        {
            // [SEC] verify user is updating their own profile (unless admin)
            var currentUser = await _userRepository.GetByIdAsync(authenticatedUserId);
            if (currentUser == null)
                return CustomResponse<User>.Fail("Usuário não encontrado");

            var profilePictureUrl = await ResolveUserImageUrlAsync(request.ProfilePictureUrl, "users/profile");
            var coverPictureUrl = await ResolveUserImageUrlAsync(request.CoverPictureUrl, "users/cover");
            string? passwordHash = null;

            if (String.IsNullOrWhiteSpace(request.Password))
            {
                var getPasswordHash = await _userRepository.GetByIdAsync(authenticatedUserId);
                passwordHash = getPasswordHash?.PasswordHash;
            }
            else
            {
                passwordHash = ResolvePasswordHash(request.Password);
            }

            var userToUpdate = new User
            {
                // [SEC] Id from JWT token, not client input
                Id = authenticatedUserId,
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                // [SEC] Role never changes via this endpoint
                Role = currentUser.Role,
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
            // [SEC] log full error, return generic message to client
            _logger.LogError(e, "Unexpected error in UpdateUserAsync");
            return CustomResponse<User>.Fail("Ocorreu um erro ao processar sua requisição.");
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
            _logger.LogError(e, "Unexpected error in GetConfigsAsync");
            return CustomResponse<ConfigsDTO>.Fail("Ocorreu um erro ao processar sua requisicao.");
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
            _logger.LogError(e, "Unexpected error in CreateNewConfigAsync");
            return CustomResponse<ConfigsDTO>.Fail("Ocorreu um erro ao processar sua requisicao.");
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
            _logger.LogError(e, "Unexpected error in UpdateConfigsAsync");
            return CustomResponse<UpdateConfigRequest>.Fail("Ocorreu um erro ao processar sua requisicao.");
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
            _logger.LogError(e, "Unexpected error in SendEmailVerifyAsync");
            return CustomResponse<bool>.Fail("Ocorreu um erro ao processar sua requisicao.");
        }
    }

    public async Task<CustomResponse<bool>> ConfirmEmailVerifyAsync(string email, string code)
    {
        try
        {
            var result = await _userRepository.ConfirmEmailVerifyAsync(email, code);

            return result
                ? CustomResponse<bool>.SuccessTrade(true)
                : CustomResponse<bool>.Fail("Falha ao confirmar email de verificação");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in ConfirmEmailVerifyAsync");
            return CustomResponse<bool>.Fail("Ocorreu um erro ao processar sua requisicao.");
        }
    }

    public async Task<CustomResponse<bool>> SendPasswordRecoveryEmailAsync(string email)
    {
        try
        {
            var result = await _userRepository.SendPasswordRecoveryEmailAsync(email);

            if (result.Success && !string.IsNullOrWhiteSpace(result.PasswordRecoveryCode))
            {
                await _userRepository.UpdatePasswordHashAsync(email, ResolvePasswordHash(result.PasswordRecoveryCode));
            }

            return result.Success
                ? CustomResponse<bool>.SuccessTrade(true)
                : CustomResponse<bool>.Fail("Falha ao enviar email de recuperação de senha");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in SendPasswordRecoveryEmailAsync");
            return CustomResponse<bool>.Fail("Ocorreu um erro ao processar sua requisicao.");
        }
    }

    public async Task<CustomResponse<IEnumerable<UserWithAbilityDTO>>> GetUsersWithAbilityAsync(IEnumerable<User> users)
    {
        try 
        {
            var usersWithAbilities = new List<UserWithAbilityDTO>();

            foreach (var user in users)
            {
                var abilities = await _userRepository.GetUserAbilitiesAsync(user.Id);

                usersWithAbilities.Add(new UserWithAbilityDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    Abilities = ["Relatório Semanal"]
                });
            }

            return CustomResponse<IEnumerable<UserWithAbilityDTO>>.SuccessTrade(usersWithAbilities);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error in GetUsersWithAbilityAsync");
            return CustomResponse<IEnumerable<UserWithAbilityDTO>>.Fail("Ocorreu um erro ao processar sua requisicao.");
        }
    }
}
