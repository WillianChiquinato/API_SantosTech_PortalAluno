namespace API_PortalSantosTech.Models.DTO;

public class ExerciseDailyTasksDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? PhaseId { get; set; }
    public List<ExerciseDTO> Exercises { get; set; } = new();
}

public class ExerciseDTO
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? VideoUrl { get; set; }
    public int PointsRedeem { get; set; }
    public DateTime TermAt { get; set; }
    public ExerciseType TypeExercise { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public int IndexOrder { get; set; }
    public bool? IsDailyTask { get; set; }
    public bool IsFinalExercise { get; set; }
    public bool IsCompletedAnswer { get; set; }
    public DateTime ExercisePeriod { get; set; }
}