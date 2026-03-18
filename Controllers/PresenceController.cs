using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PresenceController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PresenceController> _logger;

    public PresenceController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PresenceController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token");
            }

            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Bearer token not found");
            }

            var token = authHeader["Bearer ".Length..];
            var portalPainelBaseUrl = _configuration["PortalPainel:BaseUrl"]
                ?? throw new InvalidOperationException("PortalPainel:BaseUrl nao configurada");

            var upstreamUrl = $"{portalPainelBaseUrl.TrimEnd('/')}{upstreamPath}";

            var request = new HttpRequestMessage(HttpMethod.Post, upstreamUrl)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Portal-do-aluno returned {StatusCode} for {Operation}",
                    response.StatusCode,
                    operationName);

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
