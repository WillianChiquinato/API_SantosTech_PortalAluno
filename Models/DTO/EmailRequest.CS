namespace API_PortalSantosTech.Models.DTO;

public class EmailRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ConfirmEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}