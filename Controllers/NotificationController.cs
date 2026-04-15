using API_PortalSantosTech.Interfaces;
using API_PortalSantosTech.Models.DTO;
using API_PortalSantosTech.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_PortalSantosTech.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private const string AdminSecretHeaderName = "x-notification-admin-secret";

    private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;

    public NotificationController(IConfiguration configuration, INotificationService notificationService)
    {
        _configuration = configuration;
        _notificationService = notificationService;
    }

    [HttpGet]
    [Authorize]
    [Route("Inbox")]
    public async Task<IActionResult> GetInbox()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _notificationService.GetInboxAsync(authenticatedUserId.Value);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet]
    [Authorize]
    [Route("UnreadCount")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _notificationService.GetUnreadCountAsync(authenticatedUserId.Value);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [Authorize]
    [Route("MarkAsRead")]
    public async Task<IActionResult> MarkAsRead([FromBody] NotificationMarkAsReadRequest request)
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _notificationService.MarkAsReadAsync(authenticatedUserId.Value, request.NotificationId);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [Authorize]
    [Route("MarkAllAsRead")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var authenticatedUserId = User.GetAuthenticatedUserId();
        if (authenticatedUserId is null)
            return Unauthorized();

        var response = await _notificationService.MarkAllAsReadAsync(authenticatedUserId.Value);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Admin/Templates")]
    public async Task<IActionResult> GetAdminTemplates()
    {
        var errorResult = ValidateInternalAdminRequest();
        if (errorResult != null)
            return errorResult;

        var response = await _notificationService.GetTemplatesAsync();
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Admin/Templates")]
    public async Task<IActionResult> CreateAdminTemplate([FromBody] NotificationTemplateUpsertRequest request)
    {
        var errorResult = ValidateInternalAdminRequest();
        if (errorResult != null)
            return errorResult;

        var response = await _notificationService.CreateTemplateAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPut]
    [AllowAnonymous]
    [Route("Admin/Templates/{id:int}")]
    public async Task<IActionResult> UpdateAdminTemplate(int id, [FromBody] NotificationTemplateUpsertRequest request)
    {
        var errorResult = ValidateInternalAdminRequest();
        if (errorResult != null)
            return errorResult;

        var response = await _notificationService.UpdateTemplateAsync(id, request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete]
    [AllowAnonymous]
    [Route("Admin/Templates/{id:int}")]
    public async Task<IActionResult> DeleteAdminTemplate(int id)
    {
        var errorResult = ValidateInternalAdminRequest();
        if (errorResult != null)
            return errorResult;

        var response = await _notificationService.DeleteTemplateAsync(id);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("Admin/Templates/{id:int}/Dispatch")]
    public async Task<IActionResult> DispatchTemplate(int id, [FromBody] NotificationDispatchRequest request)
    {
        var errorResult = ValidateInternalAdminRequest();
        if (errorResult != null)
            return errorResult;

        var response = await _notificationService.DispatchTemplateAsync(id, request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("Admin/Dispatches")]
    public async Task<IActionResult> GetAdminDispatches([FromQuery] int limit = 10, [FromQuery] int offset = 0, [FromQuery] string? q = null)
    {
        var errorResult = ValidateInternalAdminRequest();
        if (errorResult != null)
            return errorResult;

        limit = Math.Clamp(limit, 1, 100);
        offset = Math.Max(offset, 0);

        var response = await _notificationService.GetDispatchesAsync(limit, offset, q);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpDelete]
    [AllowAnonymous]
    [Route("Admin/Dispatches/{id:int}")]
    public async Task<IActionResult> DeleteAdminDispatch(int id)
    {
        var errorResult = ValidateInternalAdminRequest();
        if (errorResult != null)
            return errorResult;

        var response = await _notificationService.DeleteDispatchAsync(id);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    private IActionResult? ValidateInternalAdminRequest()
    {
        var expectedSecret =
            _configuration["PortalPainel:NotificationsSharedSecret"] ??
            _configuration["PortalPainel:SsoSharedSecret"];
        if (string.IsNullOrWhiteSpace(expectedSecret))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Success = false,
                Errors = new[] { "Segredo interno de notificações não configurado." }
            });
        }

        var providedSecret = Request.Headers[AdminSecretHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(providedSecret) || !string.Equals(expectedSecret, providedSecret, StringComparison.Ordinal))
        {
            return Unauthorized(new
            {
                Success = false,
                Errors = new[] { "Requisição interna não autorizada." }
            });
        }

        return null;
    }
}
