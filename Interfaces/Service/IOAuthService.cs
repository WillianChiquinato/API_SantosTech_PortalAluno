using API_PortalSantosTech.Models.DTO;

namespace API_PortalSantosTech.Interfaces;

public interface IOAuthService
{
    IReadOnlyCollection<OAuthProviderInfoDto> GetAvailableProviders();
    string BuildAuthorizationUrl(string provider, HttpContext httpContext);
    Task<OAuthCallbackResultDto> HandleCallbackAsync(string provider, string code, string? state, HttpContext httpContext);
}
