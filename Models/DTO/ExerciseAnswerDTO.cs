using System.Text.Json.Serialization;

namespace API_PortalSantosTech.Models.DTO;

public class ExerciseAnswerDTO
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public int UserExerciseFlowId { get; set; }
    public bool IsCorrect { get; set; }
    public string Answer { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class VerifyDTO
{
    public bool ExistingAnswers { get; set; }
}

public class ExerciseRepeatDTO
{
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public int PhaseId { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class ExerciseMultipleChoiceAIResponse
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<ExerciseOptionAI>? Options { get; set; }
}

public class ExerciseOptionAI
{
    public int Id { get; set; }
    [JsonPropertyName("option_text")]
    public string? OptionText { get; set; }
    public bool IsCorrect { get; set; }
}

public class ExerciseDissertativeAIResponse
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    [JsonPropertyName("video_url")]
    public string? VideoUrl { get; set; }
}

public class CreateMultipleChoiceOptionDTO
{
    public int? QuestionId { get; set; }
    public int ExerciseId { get; set; }
    public string? OptionText { get; set; }
    public bool IsCorrect { get; set; }
}