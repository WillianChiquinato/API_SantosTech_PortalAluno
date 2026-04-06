namespace API_PortalSantosTech.Models.DTO;

public class OAuthProviderInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}

public class OAuthCallbackResultDto
{
    public bool Success { get; set; }
    public UserSafeDTO? User { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
