using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Interfaces.Repository;
using API_PortalSantosTech.Models;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace API_PortalSantosTech.Services;

public class OAuthService : IOAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IUserRepository userRepository,
        TokenService tokenService,
        ILogger<OAuthService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public IReadOnlyCollection<OAuthProviderInfoDto> GetAvailableProviders()
    {
        return
        [
            new OAuthProviderInfoDto
            {
                Id = "google",
                Label = "Google",
                Enabled = IsProviderEnabled("google")
            },
            new OAuthProviderInfoDto
            {
                Id = "github",
                Label = "GitHub",
                Enabled = IsProviderEnabled("github")
            }
        ];
    }

    public string BuildAuthorizationUrl(string provider, HttpContext httpContext)
    {
        provider = NormalizeProvider(provider);

        if (!IsProviderEnabled(provider))
            throw new ArgumentException("Provedor OAuth nao configurado.");

        var state = GenerateState();
        _tokenService.AppendOAuthStateCookie(httpContext.Response, provider, state, httpContext.Request.IsHttps);

        return provider switch
        {
            "google" => BuildGoogleAuthorizationUrl(state, httpContext),
            "github" => BuildGithubAuthorizationUrl(state, httpContext),
            _ => throw new ArgumentException("Provedor OAuth invalido.")
        };
    }

    public async Task<OAuthCallbackResultDto> HandleCallbackAsync(string provider, string code, string? state, HttpContext httpContext)
    {
        provider = NormalizeProvider(provider);
        _logger.LogInformation("OAuth callback started for provider {Provider}", provider);

        var expectedState = GetExpectedState(provider, httpContext.Request.Cookies);
        _tokenService.DeleteOAuthStateCookie(httpContext.Response, provider, httpContext.Request.IsHttps);

        if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(expectedState) || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(state), Encoding.UTF8.GetBytes(expectedState)))
        {
            _logger.LogWarning("OAuth callback invalid state for provider {Provider}. ReceivedStatePresent={ReceivedStatePresent}, ExpectedStatePresent={ExpectedStatePresent}", provider, !string.IsNullOrWhiteSpace(state), !string.IsNullOrWhiteSpace(expectedState));
            return Fail("invalid_state", "Nao foi possivel validar sua autenticacao. Tente novamente.");
        }

        if (!IsProviderEnabled(provider))
        {
            _logger.LogWarning("OAuth provider {Provider} is disabled or misconfigured", provider);
            return Fail("provider_disabled", "Esse provedor de login nao esta disponivel no momento.");
        }

        var identityData = provider switch
        {
            "google" => await ResolveGoogleIdentityAsync(code, httpContext),
            "github" => await ResolveGithubIdentityAsync(code, httpContext),
            _ => null
        };

        if (identityData == null)
        {
            _logger.LogWarning("OAuth identity resolution failed for provider {Provider}", provider);
            return Fail("oauth_failed", "Nao foi possivel obter seus dados de autenticacao.");
        }

        if (string.IsNullOrWhiteSpace(identityData.Email))
        {
            _logger.LogWarning("OAuth provider {Provider} did not return a usable email. ProviderUserId={ProviderUserId}", provider, identityData.ProviderUserId);
            return Fail("email_missing", "Sua conta no provedor nao retornou um email valido.");
        }

        _logger.LogInformation("OAuth identity resolved for provider {Provider}. ProviderUserId={ProviderUserId}, Email={Email}", provider, identityData.ProviderUserId, identityData.Email);

        var existingIdentity = await _userRepository.GetUserIdentityAsync(provider, identityData.ProviderUserId);
        if (existingIdentity != null)
        {
            _logger.LogInformation("Existing OAuth identity found for provider {Provider}. UserId={UserId}", provider, existingIdentity.UserId);
            var linkedUser = await _userRepository.GetByIdAsync(existingIdentity.UserId);
            if (linkedUser == null)
            {
                _logger.LogWarning("OAuth identity exists for provider {Provider}, but linked user {UserId} was not found", provider, existingIdentity.UserId);
                return Fail("account_not_found", "Sua conta nao esta cadastrada no sistema.");
            }

            return AuthenticateUser(linkedUser.ToSafeDto(), httpContext);
        }

        var existingUser = await _userRepository.GetUserByEmail(identityData.Email);
        if (existingUser == null)
        {
            _logger.LogWarning("No local account found for OAuth email {Email} on provider {Provider}", identityData.Email, provider);
            return Fail("account_not_found", "Sua conta ainda nao foi cadastrada no portal.");
        }

        _logger.LogInformation("Local account found for OAuth email {Email}. UserId={UserId}", identityData.Email, existingUser.Id);

        var linkedIdentity = await _userRepository.GetUserIdentityByUserIdAndProviderAsync(existingUser.Id, provider);
        if (linkedIdentity == null)
        {
            _logger.LogInformation("Creating OAuth identity link for provider {Provider}. UserId={UserId}", provider, existingUser.Id);
            await _userRepository.CreateUserIdentityAsync(new UserIdentity
            {
                UserId = existingUser.Id,
                Provider = provider,
                ProviderUserId = identityData.ProviderUserId,
                ProviderEmail = identityData.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            _logger.LogInformation("OAuth identity link already exists for provider {Provider}. UserId={UserId}", provider, existingUser.Id);
        }

        return AuthenticateUser(existingUser.ToSafeDto(), httpContext);
    }

    private OAuthCallbackResultDto AuthenticateUser(UserSafeDTO user, HttpContext httpContext)
    {
        var token = _tokenService.GenerateToken(user);
        _tokenService.AppendAuthCookie(httpContext.Response, token, httpContext.Request.IsHttps);
        _logger.LogInformation("OAuth authentication completed. Auth cookie emitted for UserId={UserId}, Email={Email}", user.Id, user.Email);

        return new OAuthCallbackResultDto
        {
            Success = true,
            User = user
        };
    }

    private OAuthCallbackResultDto Fail(string code, string message)
    {
        return new OAuthCallbackResultDto
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }

    private string BuildGoogleAuthorizationUrl(string state, HttpContext httpContext)
    {
        return QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", new Dictionary<string, string?>
        {
            ["client_id"] = _configuration["OAuth:Google:ClientId"],
            ["redirect_uri"] = BuildCallbackUrl("google", httpContext),
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["state"] = state,
            ["prompt"] = "select_account"
        });
    }

    private string BuildGithubAuthorizationUrl(string state, HttpContext httpContext)
    {
        return QueryHelpers.AddQueryString("https://github.com/login/oauth/authorize", new Dictionary<string, string?>
        {
            ["client_id"] = _configuration["OAuth:Github:ClientId"],
            ["redirect_uri"] = BuildCallbackUrl("github", httpContext),
            ["scope"] = "read:user user:email",
            ["state"] = state
        });
    }

    private async Task<OAuthIdentityData?> ResolveGoogleIdentityAsync(string code, HttpContext httpContext)
    {
        using var client = _httpClientFactory.CreateClient();
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["client_id"] = _configuration["OAuth:Google:ClientId"],
                ["client_secret"] = _configuration["OAuth:Google:ClientSecret"],
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = BuildCallbackUrl("google", httpContext)
            }!)
        };

        using var tokenResponse = await client.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Google token exchange failed with status code {StatusCode}", (int)tokenResponse.StatusCode);
            return null;
        }

        var tokenPayload = JsonSerializer.Deserialize<GoogleTokenResponse>(
            await tokenResponse.Content.ReadAsStringAsync(),
            JsonOptions
        );

        if (string.IsNullOrWhiteSpace(tokenPayload?.AccessToken))
        {
            _logger.LogWarning("Google token response did not contain an access token");
            return null;
        }

        using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://openidconnect.googleapis.com/v1/userinfo");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenPayload.AccessToken);

        using var userResponse = await client.SendAsync(userRequest);
        if (!userResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Google userinfo request failed with status code {StatusCode}", (int)userResponse.StatusCode);
            return null;
        }

        var profile = JsonSerializer.Deserialize<GoogleUserInfoResponse>(
            await userResponse.Content.ReadAsStringAsync(),
            JsonOptions
        );

        if (profile == null || !profile.EmailVerified || string.IsNullOrWhiteSpace(profile.Sub))
        {
            _logger.LogWarning("Google userinfo response missing required fields. EmailPresent={EmailPresent}, EmailVerified={EmailVerified}, SubjectPresent={SubjectPresent}", !string.IsNullOrWhiteSpace(profile?.Email), profile?.EmailVerified ?? false, !string.IsNullOrWhiteSpace(profile?.Sub));
            return null;
        }

        return new OAuthIdentityData
        {
            ProviderUserId = profile.Sub,
            Email = profile.Email.Trim().ToLowerInvariant()
        };
    }

    private async Task<OAuthIdentityData?> ResolveGithubIdentityAsync(string code, HttpContext httpContext)
    {
        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("PortalSantosTech");

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["client_id"] = _configuration["OAuth:Github:ClientId"],
                ["client_secret"] = _configuration["OAuth:Github:ClientSecret"],
                ["code"] = code,
                ["redirect_uri"] = BuildCallbackUrl("github", httpContext)
            }!)
        };
        tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var tokenResponse = await client.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHub token exchange failed with status code {StatusCode}", (int)tokenResponse.StatusCode);
            return null;
        }

        var tokenPayload = JsonSerializer.Deserialize<GithubTokenResponse>(
            await tokenResponse.Content.ReadAsStringAsync(),
            JsonOptions
        );

        if (string.IsNullOrWhiteSpace(tokenPayload?.AccessToken))
        {
            _logger.LogWarning("GitHub token response did not contain an access token");
            return null;
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenPayload.AccessToken);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var userResponse = await client.GetAsync("https://api.github.com/user");
        if (!userResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHub user request failed with status code {StatusCode}", (int)userResponse.StatusCode);
            return null;
        }

        var userProfile = JsonSerializer.Deserialize<GithubUserResponse>(
            await userResponse.Content.ReadAsStringAsync(),
            JsonOptions
        );

        var emailsResponse = await client.GetAsync("https://api.github.com/user/emails");
        if (!emailsResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHub emails request failed with status code {StatusCode}", (int)emailsResponse.StatusCode);
            return null;
        }

        var emails = JsonSerializer.Deserialize<List<GithubEmailResponse>>(
            await emailsResponse.Content.ReadAsStringAsync(),
            JsonOptions
        ) ?? [];

        var primaryEmail = emails.FirstOrDefault(email => email.Primary && email.Verified)
            ?? emails.FirstOrDefault(email => email.Verified);

        if (userProfile == null || primaryEmail == null || userProfile.Id <= 0)
        {
            _logger.LogWarning("GitHub identity data missing required fields. UserIdPresent={UserIdPresent}, EmailPresent={EmailPresent}", (userProfile?.Id ?? 0) > 0, primaryEmail != null);
            return null;
        }

        return new OAuthIdentityData
        {
            ProviderUserId = userProfile.Id.ToString(),
            Email = primaryEmail.Email.Trim().ToLowerInvariant()
        };
    }

    private string BuildCallbackUrl(string provider, HttpContext httpContext)
    {
        var configuredBaseUrl = _configuration["OAuth:BackendBaseUrl"];
        var baseUri = string.IsNullOrWhiteSpace(configuredBaseUrl)
            ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
            : configuredBaseUrl.TrimEnd('/');

        return $"{baseUri}/api/Auth/oauth/{provider}/callback";
    }

    private bool IsProviderEnabled(string provider)
    {
        return provider switch
        {
            "google" => !string.IsNullOrWhiteSpace(_configuration["OAuth:Google:ClientId"]) &&
                        !string.IsNullOrWhiteSpace(_configuration["OAuth:Google:ClientSecret"]),
            "github" => !string.IsNullOrWhiteSpace(_configuration["OAuth:Github:ClientId"]) &&
                        !string.IsNullOrWhiteSpace(_configuration["OAuth:Github:ClientSecret"]),
            _ => false
        };
    }

    private static string NormalizeProvider(string provider)
    {
        return provider.Trim().ToLowerInvariant() switch
        {
            "google" => "google",
            "github" => "github",
            _ => throw new ArgumentException("Provedor OAuth invalido.")
        };
    }

    private static string GenerateState()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    }

    private static string? GetExpectedState(string provider, IRequestCookieCollection cookies)
    {
        return cookies.TryGetValue($"{TokenService.OAuthStateCookiePrefix}{provider}", out var state)
            ? state
            : null;
    }

    private sealed class OAuthIdentityData
    {
        public string ProviderUserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class GoogleUserInfoResponse
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }
    }

    private sealed class GithubTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class GithubUserResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

    private sealed class GithubEmailResponse
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }
    }
}
