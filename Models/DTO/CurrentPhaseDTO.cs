namespace API_PortalSantosTech.Models.DTO;

public class CurrentPhaseDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CurrentModuleDTO? Module { get; set; }
}

public class CurrentModuleDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}