namespace API_PortalSantosTech.Models.DTO;

public class AddPointsDTO
{
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
}

public class ExercisePointAwardResult
{
    public bool Success { get; set; }
    public bool AlreadyAwarded { get; set; }
    public int PointsAwarded { get; set; }
    public string Message { get; set; } = string.Empty;
}
