using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

[Table("answer")]
public class Answer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("exercise_id")]
    public int ExerciseId { get; set; }

    [Column("user_exercise_flow_id")]
    public int UserExerciseFlowId { get; set; }

    [Column("answer_text")]
    public string? AnswerText { get; set; }

    [Column("selected_option")]
    public int SelectedOption { get; set; }

    [Column("is_correct")]
    public bool IsCorrect { get; set; }

    [Column("answered_at")]
    public DateTime AnsweredAt { get; set; }

    [Column("feedback")]
    public string? Feedback { get; set; }

    // 🔗 Relacionamentos
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public Question? Question { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }

    [ForeignKey(nameof(UserExerciseFlowId))]
    public UserExerciseFlow? UserExerciseFlow { get; set; }
}