namespace API_PortalSantosTech.Models.DTO;

public class ExerciseAnswerDTO
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public bool IsCorrect { get; set; }
    public string Answer { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}