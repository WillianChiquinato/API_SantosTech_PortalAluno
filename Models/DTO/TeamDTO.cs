namespace API_PortalSantosTech.Models.DTO;

public class CreateTeamRequest
{
    public int ClassId { get; set; }
    public int ModuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClanColor { get; set; } = string.Empty;
    public string BoatName { get; set; } = string.Empty;

    public List<int> UserIds { get; set; } = new List<int>();
}