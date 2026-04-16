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

public class ClassCoursesDTO
{
    public int Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime ClassStartedAt { get; set; }
    public DateTime ClassFinishedAt { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseDescription { get; set; } = string.Empty;
    public int CourseDuration { get; set; }
    public string CourseLevel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}