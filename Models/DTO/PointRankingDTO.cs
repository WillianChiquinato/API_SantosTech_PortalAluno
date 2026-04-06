namespace API_PortalSantosTech.Models.DTO;

public class PointRankingDTO
{
    public int UserId { get; set; }
    public float TotalPoints { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}
