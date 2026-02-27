namespace API_PortalSantosTech.Models.DTO;

public class IslandDTO
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Helper { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int LowerState { get; set; }
    public double Progress { get; set; }
    public BlipsDTO[] Blips { get; set; } = Array.Empty<BlipsDTO>();
}

public class BlipsDTO
{
    public ExerciseDTO Exercise { get; set; } = new();
    public string? State { get; set; }
}

public class IslandPhaseDTO
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Helper { get; set; } = string.Empty;
}