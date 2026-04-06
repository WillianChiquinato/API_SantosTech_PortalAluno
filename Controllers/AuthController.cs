using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Utils;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // [SEC] configs and session checks require an authenticated user
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IOAuthService _oauthService;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthController(IUserService userService, IOAuthService oauthService, TokenService tokenService, IConfiguration configuration)
    {
        _userService = userService;
        _oauthService = oauthService;
        _tokenService = tokenService;
        _configuration = configuration;
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
}
