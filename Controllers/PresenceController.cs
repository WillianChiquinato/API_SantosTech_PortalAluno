using API_PortalSantosTech.Data;
using API_PortalSantosTech.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PresenceController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PresenceController> _logger;
    private readonly AppDbContext _dbContext;

    public PresenceController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PresenceController> logger,
        AppDbContext dbContext)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpPost("socket-ticket")]
    [Authorize]
    [ProducesResponseType(typeof(PresenceSocketTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public Task<IActionResult> GetSocketTicket()
    {
        return ProxyPresencePostAsync(
            "/presence/socket-ticket",
            "socket-ticket",
            "Nao foi possivel obter ticket de presenca",
            "Erro ao obter ticket de presenca");
    }

    [HttpPost("heartbeat")]
    [Authorize]
    [ProducesResponseType(typeof(PresenceHeartbeatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public Task<IActionResult> Heartbeat()
    {
        return ProxyPresencePostAsync(
            "/presence/heartbeat",
            "heartbeat",
            "Nao foi possivel registrar heartbeat de presenca",
            "Erro ao registrar heartbeat de presenca");
    }

    private async Task<IActionResult> ProxyPresencePostAsync(
        string upstreamPath,
        string operationName,
        string upstreamFailureMessage,
        string unexpectedFailureMessage)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token");
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst(ClaimTypes.Upn)?.Value
                ?? User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Unauthorized("User email not found in token");
            }

            var portalPainelBaseUrl =
                _configuration["PortalPainel:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("PortalPainel__BaseUrl")
                ?? throw new InvalidOperationException("PortalPainel:BaseUrl nao configurada");
            var presenceProxySecret =
                _configuration["PortalPainel:PresenceProxySecret"]
                ?? Environment.GetEnvironmentVariable("PortalPainel__PresenceProxySecret");

            var upstreamUrl = $"{portalPainelBaseUrl.TrimEnd('/')}{upstreamPath}";
            var roleId = await ResolvePresenceRoleIdAsync(userIdClaim, userEmail);

            var request = new HttpRequestMessage(HttpMethod.Post, upstreamUrl)
            {
                Content = JsonContent.Create(new PresenceProxyRequest
                {
                    UserId = userIdClaim,
                    Usuario = userEmail,
                    RoleId = roleId
                }),
            };

            if (!string.IsNullOrWhiteSpace(presenceProxySecret))
            {
                request.Headers.Add("X-Presence-Proxy-Secret", presenceProxySecret);

                var clientUserAgent = Request.Headers.UserAgent.ToString();
                if (!string.IsNullOrWhiteSpace(clientUserAgent))
                {
                    request.Headers.TryAddWithoutValidation("X-Presence-Client-User-Agent", clientUserAgent);
                }

                var forwardedFor = GetHeaderValue("X-Forwarded-For");
                var clientIp =
                    !string.IsNullOrWhiteSpace(forwardedFor)
                        ? forwardedFor
                        : HttpContext.Connection.RemoteIpAddress?.ToString();

                if (!string.IsNullOrWhiteSpace(clientIp))
                {
                    request.Headers.TryAddWithoutValidation("X-Presence-Client-Ip", clientIp);
                }
            }
            else
            {
                var authHeader = Request.Headers.Authorization.ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized("Bearer token not found");
                }

                var token = authHeader["Bearer ".Length..];
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var upstreamContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Portal-do-aluno returned {StatusCode} for {Operation}. PresenceProxySecret configured: {HasPresenceProxySecret}. Body: {ResponseBody}",
                    response.StatusCode,
                    operationName,
                    !string.IsNullOrWhiteSpace(presenceProxySecret),
                    upstreamContent);

                return StatusCode(StatusCodes.Status502BadGateway, new { message = upstreamFailureMessage });
            }

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying presence {Operation}", operationName);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = unexpectedFailureMessage });
        }
    }

    private async Task<int> ResolvePresenceRoleIdAsync(string userIdClaim, string userEmail)
    {
        if (int.TryParse(userIdClaim, out var userId))
        {
            var roleById = await _dbContext.Users
                .AsNoTracking()
                .Where(user => user.Id == userId)
                .Select(user => (int?)user.Role)
                .FirstOrDefaultAsync();

            if (roleById is >= 1 and <= 3)
            {
                return roleById.Value;
            }
        }

        var roleByEmail = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Email == userEmail)
            .Select(user => (int?)user.Role)
            .FirstOrDefaultAsync();

        if (roleByEmail is >= 1 and <= 3)
        {
            return roleByEmail.Value;
        }

        return (int)UserRole.Student;
    }

    private string? GetHeaderValue(string headerName)
    {
        if (!Request.Headers.TryGetValue(headerName, out StringValues values))
        {
            return null;
        }

        return values.ToString();
    }
}

public class PresenceSocketTicketResponse
{
    public bool Ok { get; set; }
    public string Ticket { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
}

public class PresenceHeartbeatResponse
{
    public bool Ok { get; set; }
    public string LastSeenAt { get; set; } = string.Empty;
}

public class PresenceProxyRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public int RoleId { get; set; }
}
