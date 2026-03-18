using System.Security.Claims;
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

    /// <summary>

    /// </summary>
    [HttpPost("socket-ticket")]
    [Authorize]
    [ProducesResponseType(typeof(PresenceSocketTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetSocketTicket()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token");

            // Obter o token JWT do header
            var authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized("Bearer token not found");

            var token = authHeader["Bearer ".Length..];

            // Obter URL do Portal-do-aluno do configuration
            var portalAlunioBaseUrl = _configuration["PortalPainel:BaseUrl"]
                ?? throw new InvalidOperationException("PortalPainel:BaseUrl não configurada");

            var ticketUrl = $"{portalAlunioBaseUrl.TrimEnd('/')}/presence/socket-ticket";

            // Fazer requisição para o Portal-do-aluno
            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, ticketUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Portal-do-aluno returned {StatusCode} for socket-ticket", response.StatusCode);
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "Não foi possível obter ticket de presença" });
            }

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting presence socket ticket");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Erro ao obter ticket de presença" });
        }
    }
}

public class PresenceSocketTicketResponse
{
    public bool Ok { get; set; }
    public string Ticket { get; set; } = string.Empty;
    public string ExpiresAt { get; set; } = string.Empty;
}
