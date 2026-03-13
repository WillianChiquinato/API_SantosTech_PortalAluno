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
    public ContainerExerciseDTO ContainerExercise { get; set; } = new();
    public string? StateContainer { get; set; }
    public int? PhaseId { get; set; }

    public bool IsLocked { get; set; }
    public DateTime? UnlockDate { get; set; }
    public int? DaysRemaining { get; set; }
}

public class ContainerExerciseDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int? ContainerDateTarget { get; set; }
    public List<ExercisesContentDTO> Exercises { get; set; } = new();
}

public class ExercisesContentDTO
{
    public ExerciseDTO Exercise { get; set; } = new();
    public FlowOrigin? Origin { get; set; }
    public string? StateExercise { get; set; }
    public int? UserContainerExerciseFlowId { get; set; }
}

public class IslandPhaseDTO
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Helper { get; set; } = string.Empty;
}