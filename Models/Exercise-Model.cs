using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PortalSantosTech.Models;

public enum DifficultyLevel
{
    Normal = 1,
    Lower = 2
}

public enum ExerciseType
{
    MultipleChoice = 1,
    OpenEnded = 2
}

[Table("exercise")]
public class Exercise
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("phase_id")]
    public int PhaseId { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("video_url")]
    public string? VideoUrl { get; set; }

    [Column("type_exercise")]
    public ExerciseType TypeExercise { get; set; }

    [Column("difficulty")]
    public DifficultyLevel Difficulty { get; set; }

    [Column("index_order")]
    public int IndexOrder { get; set; }

    [Column("is_final_exercise")]
    public bool IsFinalExercise { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    // 🔗 Relacionamento
    [ForeignKey(nameof(PhaseId))]
    public Phase? Phase { get; set; }
}