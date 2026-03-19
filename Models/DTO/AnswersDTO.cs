namespace API_PortalSantosTech.Models.DTO;

public class AnswerDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int QuestionId { get; set; }
    public ExerciseDTO? Exercise { get; set; }
    public int? UserExerciseFlowId { get; set; }
    public string? AnswerText { get; set; }
    public string? SelectedOption { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
    public string? Feedback { get; set; }
}

public class AnswerGroupByExerciseDTO
{
    public ExerciseDTO? Exercise { get; set; }
    public IEnumerable<AnswerDTO> Answers { get; set; } = [];
}

public class AnswerAndUnreadResponsesCountDTO
{
    public IEnumerable<AnswerGroupByExerciseDTO> ExerciseGroups { get; set; } = [];
    public int UnreadResponsesCount { get; set; }
}