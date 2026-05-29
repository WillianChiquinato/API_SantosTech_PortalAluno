using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;
using API_PortalSantosTech.Services;
using Microsoft.IdentityModel.Tokens;

namespace API_PortalSantosTech.Middlewares;

public class SantosAuthMiddleware
{
    private const string FinalChallengeHubRoute = "/hubs/final-challenge";

    private readonly RequestDelegate _next;
    private readonly string _jwtSecret;
    private readonly string _authApiUrl;
    private readonly IHttpClientFactory _httpFactory;

    public SantosAuthMiddleware(RequestDelegate next, IConfiguration config, IHttpClientFactory httpFactory)
    {
        _next = next;
        _jwtSecret = config["SantosTech:JwtSecret"]
            ?? throw new InvalidOperationException("SantosTech:JwtSecret não configurado");
        _authApiUrl = config["SantosTech:ApiUrl"]
            ?? throw new InvalidOperationException("SantosTech:ApiUrl não configurado");
        _httpFactory = httpFactory;
    }

    public async Task InvokeAsync(HttpContext context, ISantosAuthCacheService cache)
    {
        // Rotas públicas (health, swagger, hangfire)
        var path = context.Request.Path.Value ?? "";
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // 1. Extrai token do cookie httpOnly
        var token = context.Request.Cookies["access_token"];
        if (string.IsNullOrEmpty(token))
        {
            // Tenta Authorization: Bearer como fallback (Swagger / testes)
            var header = context.Request.Headers.Authorization.FirstOrDefault();
            token = header?.StartsWith("Bearer ") == true ? header[7..] : null;
        }
        // Fallback para query string apenas no hub WebSocket — query params vazam em logs/referrer
        if (string.IsNullOrEmpty(token) && path.StartsWith(FinalChallengeHubRoute, StringComparison.OrdinalIgnoreCase))
        {
            var qs = context.Request.Query["access_token"].ToString();
            if (!string.IsNullOrEmpty(qs)) token = qs;
        }

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { code = "UNAUTHORIZED", message = "Não autenticado" });
            return;
        }

        // 2. Valida JWT e extrai claims
        var (userId, emailFromToken) = ParseJwtClaims(token);
        if (userId is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { code = "UNAUTHORIZED", message = "Token inválido ou expirado" });
            return;
        }

        SantosUserProfile? user;

        if (!string.IsNullOrEmpty(emailFromToken))
        {
            // Fast path: email no JWT → sem I/O externo
            user = new SantosUserProfile(userId, emailFromToken, null, emailFromToken, 0, null, null, null, null);
        }
        else
        {
            // Slow path: busca no cache ou auth service
            user = await cache.GetAsync(userId);
            if (user is null)
            {
                user = await FetchFromAuthService(token);
                if (user is null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { code = "UNAUTHORIZED", message = "Sessão inválida" });
                    return;
                }
                await cache.SetAsync(user);
            }
        }

        // 5. Conta suspensa
        if (user.SuspendedAt is not null)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { code = "ACCOUNT_SUSPENDED", message = "Conta suspensa" });
            return;
        }

        // 6. Injeta perfil no contexto
        context.Items["SantosUser"] = user;

        await _next(context);
    }

    private (string? userId, string? email) ParseJwtClaims(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));

        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidAlgorithms = ["HS256"],
            }, out var validated);

            var jwt = (JwtSecurityToken)validated;
            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            return (jwt.Subject, email);
        }
        catch { return (null, null); }
    }

    // Keep for compatibility
    private string? ValidateJwt(string token) => ParseJwtClaims(token).userId;

    private async Task<SantosUserProfile?> FetchFromAuthService(string cookieToken)
    {
        try
        {
            var client = _httpFactory.CreateClient("SantosAuth");
            var req = new HttpRequestMessage(HttpMethod.Get, $"{_authApiUrl}/auth/me");
            req.Headers.Add("Cookie", $"access_token={cookieToken}");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var res = await client.SendAsync(req, cts.Token);

            if (!res.IsSuccessStatusCode) return null;

            var body = await res.Content.ReadFromJsonAsync<AuthMeResponse>();
            if (body?.User is null) return null;

            var u = body.User;
            return new SantosUserProfile(
                u.Id, u.Email, u.Username, u.Name, u.Role,
                u.CustomRoleId, u.AvatarUrl, u.SuspendedAt, u.Permissions
            );
        }
        catch { return null; }
    }

    private static bool IsPublicPath(string path) =>
        path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/hangfire", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/health", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/api/auth/oauth/", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/auth/providers", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/auth/logout", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/api/auth/sso/callback", StringComparison.OrdinalIgnoreCase);

    private record AuthMeResponse(AuthMeUser? User);
    private record AuthMeUser(
        string Id, string Email, string? Username, string Name, int Role,
        string? CustomRoleId, string? AvatarUrl, string? SuspendedAt,
        Dictionary<string, List<string>>? Permissions
    );
}

// Extension helper para leitura nas controllers
public static class SantosUserExtensions
{
    public static SantosUserProfile? GetSantosUser(this HttpContext context)
        => context.Items["SantosUser"] as SantosUserProfile;

    public static bool HasScope(this SantosUserProfile user, string resource, string action)
    {
        if (user.Role == 3) return true; // Admin tem tudo
        if (user.Role == 4 && user.Permissions is not null)
            return user.Permissions.TryGetValue(resource, out var actions) && actions.Contains(action);
        return false;
    }
}
