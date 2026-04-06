namespace API_PortalSantosTech.Models.DTO;

public class UpdateUserRequest
{
    // [SEC] Id and Role removed — backend derives Id from JWT token, Role never changes via this endpoint
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? CoverPictureUrl { get; set; }
    public DateTime? UpdatedAt { get; set; }
}