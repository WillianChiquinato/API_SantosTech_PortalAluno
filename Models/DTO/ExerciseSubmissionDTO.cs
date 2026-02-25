namespace API_PortalSantosTech.Models.DTO;

public class ExerciseSubmissionDTO
{
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public int QuestionId { get; set; }
    public ExerciseSubmissionResultDTO? SubmissionData { get; set; }
}

public class ExerciseSubmissionResultDTO
{
    public int SelectedOption { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}