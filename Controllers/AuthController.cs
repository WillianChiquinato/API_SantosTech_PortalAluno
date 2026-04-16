using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Utils;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] configs and session checks require an authenticated user
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IOAuthService _oauthService;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(
        IUserService userService,
        IUserRepository userRepository,
        IOAuthService oauthService,
        TokenService tokenService,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _userService = userService;
        _userRepository = userRepository;
        _oauthService = oauthService;
        _tokenService = tokenService;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    [Route("Logged")]
    public async Task<IActionResult> Logged()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var user = await _userService.GetByIdAsync(authenticatedUserId.Value);
        if (!user.Success || user.Result == null)
            return Unauthorized();

        return Ok(new
        {
            success = true,
            errors = Array.Empty<string>(),
            result = user.Result.ToSafeDto()
        });
    }

    [HttpGet]
    [Route("Session")]
    public Task<IActionResult> Session()
    {
        return Logged();
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Providers")]
    public IActionResult Providers()
    {
        return Ok(new
        {
            success = true,
            errors = Array.Empty<string>(),
            result = _oauthService.GetAvailableProviders()
        });
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("oauth/{provider}/start")]
    public IActionResult StartOAuth([FromRoute] string provider)
    {
        try
        {
            return Redirect(_oauthService.BuildAuthorizationUrl(provider, HttpContext));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                success = false,
                errors = new[] { exception.Message }
            });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("sso/callback")]
    public async Task<IActionResult> StudentViewSsoCallback([FromQuery] string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Redirect(BuildStudentViewErrorRedirect("Codigo SSO ausente."));

        var portalPainelBaseUrl =
            _configuration["PortalPainel:BaseUrl"]
            ?? Environment.GetEnvironmentVariable("PortalPainel__BaseUrl");
        var sharedSecret =
            _configuration["PortalPainel:SsoSharedSecret"]
            ?? Environment.GetEnvironmentVariable("PortalPainel__SsoSharedSecret");

        if (string.IsNullOrWhiteSpace(portalPainelBaseUrl) || string.IsNullOrWhiteSpace(sharedSecret))
            return Redirect(BuildStudentViewErrorRedirect("SSO do admin portal nao configurado."));

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{portalPainelBaseUrl.TrimEnd('/')}/auth/student-view/exchange");

            request.Headers.Add("x-sso-shared-secret", sharedSecret);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { code }),
                Encoding.UTF8,
                "application/json");

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return Redirect(BuildStudentViewErrorRedirect("Nao foi possivel validar o acesso SSO."));
            }

            var payload = JsonSerializer.Deserialize<StudentViewExchangeResponse>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payload?.User?.Email == null)
                return Redirect(BuildStudentViewErrorRedirect("Usuario SSO nao identificado."));

            var localUser = await ResolveLocalStudentViewUserAsync(payload.User.Id, payload.User.Email);
            if (localUser == null)
                return Redirect(BuildStudentViewErrorRedirect("Usuario nao encontrado no portal do aluno."));

            var token = _tokenService.GenerateToken(localUser);
            _tokenService.AppendAuthCookie(Response, token, Request.IsHttps);

            return Redirect(BuildStudentViewSuccessRedirect(payload.ReturnTo));
        }
        catch
        {
            return Redirect(BuildStudentViewErrorRedirect("Erro ao concluir a visao do aluno."));
        }
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("oauth/{provider}/callback")]
    public async Task<IActionResult> OAuthCallback([FromRoute] string provider, [FromQuery] string? code, [FromQuery] string? state)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Redirect(BuildFrontendErrorRedirect("missing_code", "Nao foi possivel concluir a autenticacao."));

        var result = await _oauthService.HandleCallbackAsync(provider, code, state, HttpContext);
        if (!result.Success || result.User == null)
        {
            return Redirect(BuildFrontendErrorRedirect(
                result.ErrorCode ?? "oauth_failed",
                result.ErrorMessage ?? "Nao foi possivel concluir a autenticacao."
            ));
        }

        return Redirect(BuildFrontendSuccessRedirect());
    }

    [HttpGet]
    [Route("GetConfigs")]
    public async Task<IActionResult> GetConfigs()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var user = await _userService.GetConfigsAsync(authenticatedUserId.Value);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    [Route("CreateNewConfig")]
    public async Task<IActionResult> CreateNewConfig()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _userService.CreateNewConfigAsync(authenticatedUserId.Value);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    [Route("UpdateConfigs")]
    public async Task<IActionResult> UpdateConfigs([FromBody] UpdateConfigRequest request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        // [SEC] ignore client-supplied user id and bind config updates to the authenticated user
        request.UserId = authenticatedUserId.Value;
        var response = await _userService.UpdateConfigsAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Logout")]
    public IActionResult Logout()
    {
        _tokenService.DeleteAuthCookie(Response, Request.IsHttps);

        return Ok(new
        {
            success = true,
            errors = Array.Empty<string>(),
            result = "Logout realizado com sucesso."
        });
    }

    private string BuildFrontendSuccessRedirect()
    {
        return _configuration["OAuth:FrontendCallbackSuccessUrl"]
            ?? "http://localhost:3000/auth/callback";
    }

    private string BuildFrontendErrorRedirect(string errorCode, string message)
    {
        var baseUrl = _configuration["OAuth:FrontendCallbackErrorUrl"]
            ?? "http://localhost:3000/auth/callback";

        return QueryHelpers.AddQueryString(baseUrl, new Dictionary<string, string?>
        {
            ["error"] = errorCode,
            ["message"] = message
        });
    }

    private string BuildStudentViewSuccessRedirect(string? returnTo)
    {
        var successPath = _configuration["PortalPainel:SsoSuccessPath"]
            ?? Environment.GetEnvironmentVariable("PortalPainel__SsoSuccessPath")
            ?? "/dashboard";

        var redirectPath = successPath.StartsWith("/")
            ? successPath
            : $"/{successPath}";

        if (string.IsNullOrWhiteSpace(returnTo))
            return redirectPath;

        return QueryHelpers.AddQueryString(redirectPath, new Dictionary<string, string?>
        {
            ["returnTo"] = returnTo
        });
    }

    private string BuildStudentViewErrorRedirect(string message)
    {
        var basePath = _configuration["PortalPainel:SsoErrorPath"]
            ?? Environment.GetEnvironmentVariable("PortalPainel__SsoErrorPath")
            ?? "/";

        return QueryHelpers.AddQueryString(basePath, new Dictionary<string, string?>
        {
            ["message"] = message
        });
    }

    private async Task<UserSafeDTO?> ResolveLocalStudentViewUserAsync(string? sourceUserId, string email)
    {
        if (int.TryParse(sourceUserId, out var userId))
        {
            var byId = await _userRepository.GetByIdAsync(userId);
            if (byId?.Email != null && string.Equals(byId.Email, email, StringComparison.OrdinalIgnoreCase))
                return byId.ToSafeDto();
        }

        var byEmail = await _userRepository.GetUserByEmail(email);
        var userSafeMapping = byEmail == null ? null : new UserSafeDTO
        {
            Id = byEmail.Id,
            Name = byEmail.Name,
            Email = byEmail.Email,
            Role = byEmail.Role,
            ProfilePictureUrl = byEmail.ProfilePictureUrl,
            CreatedAt = byEmail.CreatedAt,
            UpdatedAt = byEmail.UpdatedAt
        };

        return userSafeMapping;
    }

    private sealed class StudentViewExchangeResponse
    {
        public StudentViewExchangeUser? User { get; set; }
        public string? ReturnTo { get; set; }
    }

    private sealed class StudentViewExchangeUser
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}
