namespace API_PortalSantosTech.Models.DTO;

public class ClassRoomDTO
{
    public int Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ExerciseDTO> Exercises { get; set; } = new List<ExerciseDTO>();
    public DateTime CreatedAt { get; set; }
    public DateTime TargetLimited { get; set; }
}